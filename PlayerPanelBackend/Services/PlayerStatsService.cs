using MySqlConnector;

namespace PlayerPanelBackend.Services;

public record PlayerProfile(string Username, string Uuid, Dictionary<string, string> Stats);

/// <summary>
/// Looks up one player's full stat line for the player-profile page. Unlike
/// LeaderboardMySqlSyncService (which only ranks the top N), this queries
/// on demand for a single UUID, resolved by username via PlayerNameResolver.
/// </summary>
public class PlayerStatsService
{
    private readonly PlayerNameResolver _names;
    private readonly IConfiguration _config;

    public PlayerStatsService(PlayerNameResolver names, IConfiguration config)
    {
        _names = names;
        _config = config;
    }

    public async Task<PlayerProfile?> GetProfileAsync(string username, CancellationToken ct = default)
    {
        var uuid = _names.TryResolveUuid(username);
        if (uuid is null) return null; // this player hasn't been seen online since the panel started tracking

        var leaderboards = _config.GetSection("Leaderboards").Get<LeaderboardsConfig>() ?? new();
        var placeholderToCategory = leaderboards.Sources.ToDictionary(s => s.PlaceholderKey, s => s);

        await using var connection = new MySqlConnection(BuildConnectionString());
        await connection.OpenAsync(ct);

        var sql = $"SELECT `{leaderboards.PlaceholderColumn}`, `{leaderboards.ValueColumn}` " +
                  $"FROM `{leaderboards.TableName}` WHERE `{leaderboards.IdColumn}` = @id";

        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", uuid);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var stats = new Dictionary<string, string>();
        while (await reader.ReadAsync(ct))
        {
            var placeholder = reader.GetString(0);
            var rawValue = reader.GetString(1);

            if (!placeholderToCategory.TryGetValue(placeholder, out var source)) continue; // unmapped stat, skip
            if (!double.TryParse(rawValue, out var numeric)) continue;

            stats[source.Category] = LeaderboardFormatting.FormatValue(numeric, source);
        }

        return new PlayerProfile(_names.TryResolve(uuid) ?? username, uuid, stats);
    }

    private string BuildConnectionString()
    {
        var host = _config["Mysql:Host"] ?? throw new InvalidOperationException("Mysql:Host missing");
        var port = _config.GetValue<int>("Mysql:Port", 3306);
        var database = _config["Mysql:Database"] ?? throw new InvalidOperationException("Mysql:Database missing");
        var username = _config["Mysql:Username"] ?? throw new InvalidOperationException("Mysql:Username missing");
        var password = _config["Mysql:Password"] ?? throw new InvalidOperationException("Mysql:Password missing");

        return new MySqlConnectionStringBuilder
        {
            Server = host,
            Port = (uint)port,
            Database = database,
            UserID = username,
            Password = password,
            ConnectionTimeout = 5,
        }.ConnectionString;
    }
}