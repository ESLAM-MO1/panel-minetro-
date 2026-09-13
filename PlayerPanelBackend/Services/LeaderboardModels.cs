namespace PlayerPanelBackend.Services;

/// <summary>
/// Mirrors the shape the Next.js frontend already expects
/// (see lib/mock-data.ts -> LeaderboardEntry) so the API can be swapped
/// in for the mock data with zero frontend changes.
/// </summary>
public record LeaderboardEntry(int Rank, string Username, string Value);

/// <summary>
/// Global settings for how leaderboard data is laid out in MySQL. Confirmed
/// against the real server: ajLeaderboards doesn't create one table per
/// stat — it writes everything into a single table (here: "ajlb_extras")
/// with one row per (player, placeholder) pair. Configured under
/// "Leaderboards" in appsettings.json:
///
///   "Leaderboards": {
///     "TableName": "ajlb_extras",
///     "IdColumn": "id",
///     "PlaceholderColumn": "placeholder",
///     "ValueColumn": "value",
///     "NamePlaceholderKey": "player_name",
///     "Sources": [ ... ]
///   }
/// </summary>
public class LeaderboardsConfig
{
    public string TableName { get; set; } = "ajlb_extras";
    public string IdColumn { get; set; } = "id";
    public string PlaceholderColumn { get; set; } = "placeholder";
    public string ValueColumn { get; set; } = "value";

    /// <summary>
    /// The placeholder key that stores each player's display name (added via
    /// `ajlb add %player_name%` in the server console). Rows for this key
    /// are joined in to resolve a UUID to a username.
    /// </summary>
    public string NamePlaceholderKey { get; set; } = "player_name";

    public List<LeaderboardSource> Sources { get; set; } = new();
}

/// <summary>
/// One category to sync — one placeholder key stored in the shared
/// ajlb_extras table (e.g. "statistic_player_kills"), added on the server
/// via `ajlb add %statistic_player_kills%`.
/// </summary>
public class LeaderboardSource
{
    public string Category { get; set; } = "";
    public string PlaceholderKey { get; set; } = "";

    /// <summary>How many rows to pull, highest value first.</summary>
    public int Limit { get; set; } = 100;

    /// <summary>
    /// How to format the raw numeric value for display: a standard .NET
    /// numeric format string (e.g. "N0", "0.##"), or the special value
    /// "duration" to render seconds as "12d 4h" (used for playtime).
    /// </summary>
    public string? NumberFormat { get; set; }

    /// <summary>
    /// Optional: divide the raw stored value by this before formatting.
    /// Minecraft's played-time statistic is stored in ticks (20/second),
    /// so Divisor: 20 converts it to seconds before formatting as a duration.
    /// </summary>
    public double? Divisor { get; set; }
}