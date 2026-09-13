using System.Text.RegularExpressions;
using MySqlConnector;

namespace PlayerPanelBackend.Services;

/// <summary>
/// Periodically connects to MySQL and reads leaderboard data out of
/// ajLeaderboards' shared "ajlb_extras" table (confirmed against the real
/// production server on 2026-09-13 — see LeaderboardsConfig for the shape).
/// One row per (player UUID, placeholder) pair; categories are just
/// different placeholder keys filtered out of the same table, and player
/// names come from a "player_name" placeholder row joined in by UUID.
/// </summary>
public class LeaderboardMySqlSyncService : BackgroundService
{
    private static readonly Regex ValidIdentifier = new("^[A-Za-z0-9_]+$", RegexOptions.Compiled);

    private readonly LeaderboardCache _cache;
    private readonly IConfiguration _config;
    private readonly ILogger<LeaderboardMySqlSyncService> _logger;

    public LeaderboardMySqlSyncService(LeaderboardCache cache, IConfiguration config, ILogger<LeaderboardMySqlSyncService> logger)
    {
        _cache = cache;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = _config.GetValue<int>("Mysql:PollIntervalSeconds", 60);
        var leaderboards = _config.GetSection("Leaderboards").Get<LeaderboardsConfig>() ?? new();

        if (leaderboards.Sources.Count == 0)
        {
            _logger.LogWarning(
                "No leaderboard sources configured under Leaderboards:Sources — sync loop is idle.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            if (leaderboards.Sources.Count > 0)
            {
                try
                {
                    await SyncOnceAsync(leaderboards, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Leaderboard MySQL sync failed, will retry next cycle.");
                }
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // shutting down
            }
        }
    }

    private string BuildConnectionString()
    {
        var host = _config["Mysql:Host"] ?? throw new InvalidOperationException("Mysql:Host missing");
        var port = _config.GetValue<int>("Mysql:Port", 3306);
        var database = _config["Mysql:Database"] ?? throw new InvalidOperationException("Mysql:Database missing");
        var username = _config["Mysql:Username"] ?? throw new InvalidOperationException("Mysql:Username missing");
        var password = _config["Mysql:Password"] ?? throw new InvalidOperationException("Mysql:Password missing");

        var builder = new MySqlConnectionStringBuilder
        {
            Server = host,
            Port = (uint)port,
            Database = database,
            UserID = username,
            Password = password,
            ConnectionTimeout = 5,
        };
        return builder.ConnectionString;
    }

    private async Task SyncOnceAsync(LeaderboardsConfig leaderboards, CancellationToken ct)
    {
        if (!ValidIdentifier.IsMatch(leaderboards.TableName) ||
            !ValidIdentifier.IsMatch(leaderboards.IdColumn) ||
            !ValidIdentifier.IsMatch(leaderboards.PlaceholderColumn) ||
            !ValidIdentifier.IsMatch(leaderboards.ValueColumn))
        {
            _logger.LogWarning(
                "Leaderboards table/column names in config are invalid (letters, digits, underscores only) — skipping sync.");
            return;
        }

        await using var connection = new MySqlConnection(BuildConnectionString());
        await connection.OpenAsync(ct);

        var result = new Dictionary<string, List<LeaderboardEntry>>();

        foreach (var source in leaderboards.Sources)
        {
            try
            {
                result[source.Category] = await ReadCategoryAsync(connection, leaderboards, source, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed syncing category '{Category}' (placeholder '{Key}')",
                    source.Category, source.PlaceholderKey);
                var previous = _cache.GetCategory(source.Category);
                if (previous != null) result[source.Category] = previous;
            }
        }

        _cache.Set(result);
    }

    private async Task<List<LeaderboardEntry>> ReadCategoryAsync(
        MySqlConnection connection, LeaderboardsConfig cfg, LeaderboardSource source, CancellationToken ct)
    {
        var sql = $@"
            SELECT n.`{cfg.ValueColumn}` AS name, v.`{cfg.ValueColumn}` AS value
            FROM `{cfg.TableName}` v
            JOIN `{cfg.TableName}` n
              ON n.`{cfg.IdColumn}` = v.`{cfg.IdColumn}`
             AND n.`{cfg.PlaceholderColumn}` = @namePlaceholder
            WHERE v.`{cfg.PlaceholderColumn}` = @valuePlaceholder
              AND v.`{cfg.ValueColumn}` REGEXP '^[0-9.]+$'
            ORDER BY CAST(v.`{cfg.ValueColumn}` AS DECIMAL(30,4)) DESC
            LIMIT {source.Limit}";

        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@namePlaceholder", cfg.NamePlaceholderKey);
        cmd.Parameters.AddWithValue("@valuePlaceholder", source.PlaceholderKey);

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var raw = new List<(string Name, double Value)>();
        while (await reader.ReadAsync(ct))
        {
            var name = reader.IsDBNull(0) ? null : reader.GetString(0);
            if (string.IsNullOrWhiteSpace(name)) continue;
            if (!double.TryParse(reader.GetString(1), out var value)) continue;
            raw.Add((name!, value));
        }

        return raw
            .Select((p, i) => new LeaderboardEntry(i + 1, p.Name, FormatValue(p.Value, source)))
            .ToList();
    }

    private static string FormatValue(double rawValue, LeaderboardSource source)
    {
        var value = source.Divisor is > 0 ? rawValue / source.Divisor.Value : rawValue;

        if (string.Equals(source.NumberFormat, "duration", StringComparison.OrdinalIgnoreCase))
            return FormatDuration(value);

        return value.ToString(source.NumberFormat ?? "N0");
    }

    private static string FormatDuration(double totalSeconds)
    {
        var span = TimeSpan.FromSeconds(Math.Max(0, totalSeconds));
        return span.TotalDays >= 1
            ? $"{(int)span.TotalDays}d {span.Hours}h"
            : $"{span.Hours}h {span.Minutes}m";
    }
}