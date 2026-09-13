namespace PlayerPanelBackend.Services;

/// <summary>
/// Mirrors the shape the Next.js frontend already expects
/// (see lib/mock-data.ts -> LeaderboardEntry) so the API can be swapped
/// in for the mock data with zero frontend changes.
/// </summary>
public record LeaderboardEntry(int Rank, string Username, string Value);

/// <summary>
/// One category to sync — one MySQL table that ajLeaderboards (in mysql
/// storage mode, see cache_storage.yml) writes to. Configured under
/// "Leaderboards:Sources" in appsettings.json, e.g.:
///
///   {
///     "Category": "kills",
///     "TableName": "ajlb_statistic_player_kills",
///     "NameColumn": "name",
///     "ValueColumn": "value",
///     "NumberFormat": "N0"
///   }
///
/// TableName/NameColumn/ValueColumn are our best guess based on
/// ajLeaderboards' documented "ajlb_" table prefix convention. Once the
/// plugin is actually pointed at a MySQL database (production or local),
/// run `SHOW TABLES LIKE 'ajlb_%';` and `DESCRIBE <table>;` against it and
/// correct these three fields if the real names differ — that's a config
/// change only, no code change needed.
/// </summary>
public class LeaderboardSource
{
    public string Category { get; set; } = "";
    public string TableName { get; set; } = "";
    public string NameColumn { get; set; } = "name";
    public string ValueColumn { get; set; } = "value";

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
    /// Useful if a stat is stored in a different unit than we want to
    /// display (e.g. Minecraft's played-time statistic is stored in ticks —
    /// 20 ticks/second — so Divisor: 20 converts it to seconds). Unverified
    /// against real data yet; adjust once we see actual values coming through.
    /// </summary>
    public double? Divisor { get; set; }
}
