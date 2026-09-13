namespace PlayerPanelBackend.Services;

/// <summary>Shared value-formatting logic (used by both the leaderboard sync and per-player lookups).</summary>
public static class LeaderboardFormatting
{
    public static string FormatValue(double rawValue, LeaderboardSource source)
    {
        var value = source.Divisor is > 0 ? rawValue / source.Divisor.Value : rawValue;

        if (string.Equals(source.NumberFormat, "duration", StringComparison.OrdinalIgnoreCase))
            return FormatDuration(value);

        return value.ToString(source.NumberFormat ?? "N0");
    }

    public static string FormatDuration(double totalSeconds)
    {
        var span = TimeSpan.FromSeconds(Math.Max(0, totalSeconds));
        return span.TotalDays >= 1
            ? $"{(int)span.TotalDays}d {span.Hours}h"
            : $"{span.Hours}h {span.Minutes}m";
    }
}