using System.Text.RegularExpressions;
using MySqlConnector;

namespace PlayerPanelBackend.Services;

/// <summary>
/// Periodically connects directly to the MySQL database ajLeaderboards is
/// configured to use (cache_storage.yml -> method: mysql) and reads each
/// configured category's table straight out of it. Replaces the earlier
/// SFTP + file-parsing approach, which had to be abandoned because
/// ajLeaderboards' default (h2) storage isn't a plain text/JSON file and
/// the hosting panel doesn't offer plain file inspection of it.
///
/// Credentials come from configuration (appsettings + environment variables
/// / user-secrets in production) — never hardcoded. See appsettings.json
/// and appsettings.Development.json for the expected keys.
/// </summary>
public class LeaderboardMySqlSyncService : BackgroundService
{
    // Table and column names are config-driven (see LeaderboardSource), but
    // we still validate them against a strict identifier pattern before
    // building SQL with them, since they can't be passed as query
    // parameters the way values can.
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
        var sources = _config.GetSection("Leaderboards:Sources").Get<List<LeaderboardSource>>() ?? new();

        if (sources.Count == 0)
        {
            _logger.LogWarning(
                "No leaderboard sources configured under Leaderboards:Sources — sync loop is idle. " +
                "Add one entry per category once the real ajLeaderboards table names are confirmed.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            if (sources.Count > 0)
            {
                try
                {
                    await SyncOnceAsync(sources, stoppingToken);
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

    private async Task SyncOnceAsync(List<LeaderboardSource> sources, CancellationToken ct)
    {
        await using var connection = new MySqlConnection(BuildConnectionString());
        await connection.OpenAsync(ct);

        var result = new Dictionary<string, List<LeaderboardEntry>>();

        foreach (var source in sources)
        {
            try
            {
                result[source.Category] = await ReadCategoryAsync(connection, source, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed syncing category '{Category}' from table '{Table}'",
                    source.Category, source.TableName);
                // Keep previous cached data for this category rather than wiping it on a transient failure.
                var previous = _cache.GetCategory(source.Category);
                if (previous != null) result[source.Category] = previous;
            }
        }

        _cache.Set(result);
    }

    private async Task<List<LeaderboardEntry>> ReadCategoryAsync(MySqlConnection connection, LeaderboardSource source, CancellationToken ct)
    {
        if (!ValidIdentifier.IsMatch(source.TableName) ||
            !ValidIdentifier.IsMatch(source.NameColumn) ||
            !ValidIdentifier.IsMatch(source.ValueColumn))
        {
            throw new InvalidOperationException(
                $"Category '{source.Category}' has an invalid TableName/NameColumn/ValueColumn in config " +
                "(only letters, digits and underscores are allowed).");
        }

        // Table/column names are validated above (can't be bound as parameters), the LIMIT value is an int
        // we control from config, and everything else is a real bound parameter.
        var sql = $"SELECT `{source.NameColumn}`, `{source.ValueColumn}` FROM `{source.TableName}` " +
                  $"ORDER BY `{source.ValueColumn}` DESC LIMIT {source.Limit}";

        await using var cmd = new MySqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var raw = new List<(string Name, double Value)>();
        while (await reader.ReadAsync(ct))
        {
            var name = reader.GetString(0);
            var value = reader.GetDouble(1);
            raw.Add((name, value));
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
