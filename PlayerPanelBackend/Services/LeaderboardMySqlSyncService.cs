using System.Text.RegularExpressions;
using MySqlConnector;

namespace PlayerPanelBackend.Services;

/// <summary>
/// Periodically connects to MySQL and reads leaderboard data out of
/// ajLeaderboards' shared "ajlb_extras" table. Each row is (player UUID,
/// placeholder, value); categories are just different placeholder keys.
/// Usernames are NOT stored by the plugin (it refuses non-numeric
/// placeholders like %player_name%), so they're resolved via
/// PlayerNameResolver, which learns UUID->name pairs from the Server List
/// Ping "sample" every cycle.
/// </summary>
public class LeaderboardMySqlSyncService : BackgroundService
{
    private static readonly Regex ValidIdentifier = new("^[A-Za-z0-9_]+$", RegexOptions.Compiled);

    private readonly LeaderboardCache _cache;
    private readonly PlayerNameResolver _names;
    private readonly OnlinePlayersService _onlinePlayers;
    private readonly IConfiguration _config;
    private readonly ILogger<LeaderboardMySqlSyncService> _logger;

    public LeaderboardMySqlSyncService(
        LeaderboardCache cache,
        PlayerNameResolver names,
        OnlinePlayersService onlinePlayers,
        IConfiguration config,
        ILogger<LeaderboardMySqlSyncService> logger)
    {
        _cache = cache;
        _names = names;
        _onlinePlayers = onlinePlayers;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = _config.GetValue<int>("Mysql:PollIntervalSeconds", 60);
        var leaderboards = _config.GetSection("Leaderboards").Get<LeaderboardsConfig>() ?? new();

        if (leaderboards.Sources.Count == 0)
        {
            _logger.LogWarning("No leaderboard sources configured under Leaderboards:Sources — sync loop is idle.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await LearnOnlinePlayerNamesAsync(stoppingToken);

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

    private async Task LearnOnlinePlayerNamesAsync(CancellationToken ct)
    {
        try
        {
            var status = await _onlinePlayers.QueryAsync(ct);
            if (status is null) return;
            foreach (var p in status.Sample)
                _names.Learn(p.Id, p.Name);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to refresh the player name cache from Server List Ping.");
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
            _logger.LogWarning("Leaderboards table/column names in config are invalid — skipping sync.");
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
            SELECT `{cfg.IdColumn}` AS id, `{cfg.ValueColumn}` AS value
            FROM `{cfg.TableName}`
            WHERE `{cfg.PlaceholderColumn}` = @placeholderKey";

        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@placeholderKey", source.PlaceholderKey);

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var raw = new List<(string Id, double Value)>();
        while (await reader.ReadAsync(ct))
        {
            var id = reader.GetString(0);
            if (!LeaderboardFormatting.TryParseRawValue(reader.GetString(1), source, out var value)) continue;
            raw.Add((id, value));
        }

        raw.Sort((a, b) => b.Value.CompareTo(a.Value));
        if (raw.Count > source.Limit) raw = raw.GetRange(0, source.Limit);

        var entries = new List<LeaderboardEntry>();
        var rank = 0;
        foreach (var row in raw)
        {
            var name = _names.TryResolve(row.Id);
            if (name is null) continue; // haven't seen this player online yet -- skip rather than show a raw UUID
            rank++;
                        entries.Add(new LeaderboardEntry(rank, name, LeaderboardFormatting.FormatValue(row.Value, source)));
        }

        return entries;
    }
}