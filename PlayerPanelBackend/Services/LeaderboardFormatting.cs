using System.Text.RegularExpressions;

namespace PlayerPanelBackend.Services;

/// <summary>Shared value-formatting logic (used by both the leaderboard sync and per-player lookups).</summary>
public static class LeaderboardFormatting
{
    private static readonly Regex DurationRegex = new(
        @"^(?:(?<days>\d+)\s*d)?\s*(?:(?<hours>\d+)\s*h)?\s*(?:(?<minutes>\d+)\s*m)?\s*(?:(?<seconds>\d+)\s*s)?\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Parses a raw DB value into a number, honoring the source's format.
    /// Handles both plain numbers and pre-formatted duration strings like "1d 2h 29m 56s".
    /// </summary>
    public static bool TryParseRawValue(string raw, LeaderboardSource source, out double value)
    {
        if (double.TryParse(raw, out value))
            return true;

        if (string.Equals(source.NumberFormat, "duration", StringComparison.OrdinalIgnoreCase))
            return TryParseDurationToSeconds(raw, out value);

        value = 0;
        return false;
    }

    public static bool TryParseDurationToSeconds(string raw, out double totalSeconds)
    {
        totalSeconds = 0;
        if (string.IsNullOrWhiteSpace(raw)) return false;

        var match = DurationRegex.Match(raw.Trim());
        if (!match.Success) return false;

        var hasAny = match.Groups["days"].Success || match.Groups["hours"].Success ||
                     match.Groups["minutes"].Success || match.Groups["seconds"].Success;
        if (!hasAny) return false;

        double days = match.Groups["days"].Success ? double.Parse(match.Groups["days"].Value) : 0;
        double hours = match.Groups["hours"].Success ? double.Parse(match.Groups["hours"].Value) : 0;
        double minutes = match.Groups["minutes"].Success ? double.Parse(match.Groups["minutes"].Value) : 0;
        double seconds = match.Groups["seconds"].Success ? double.Parse(match.Groups["seconds"].Value) : 0;

        totalSeconds = days * 86400 + hours * 3600 + minutes * 60 + seconds;
        return true;
    }

    public static string FormatValue(double rawValue, LeaderboardSource source)
    {
        var value = source.Divisor is > 0 ? rawValue / source.Divisor.Value : rawValue;

        if (string.Equals(source.NumberFormat, "duration", StringComparison.OrdinalIgnoreCase))
            return FormatDuration(value);

        if (string.Equals(source.NumberFormat, "compact", StringComparison.OrdinalIgnoreCase))
            return FormatCompact(value);

        return value.ToString(source.NumberFormat ?? "N0");
    }

    public static string FormatDuration(double totalSeconds)
    {
        var span = TimeSpan.FromSeconds(Math.Max(0, totalSeconds));
        return span.TotalDays >= 1
            ? $"{(int)span.TotalDays}d {span.Hours}h"
            : $"{span.Hours}h {span.Minutes}m";
    }

    /// <summary>Formats large numbers compactly: 100000 -> "100K", 1500000 -> "1.5M".</summary>
    public static string FormatCompact(double value)
    {
        var abs = Math.Abs(value);
        if (abs >= 1_000_000_000)
            return (value / 1_000_000_000).ToString("0.#") + "B";
        if (abs >= 1_000_000)
            return (value / 1_000_000).ToString("0.#") + "M";
        if (abs >= 1_000)
            return (value / 1_000).ToString("0.#") + "K";
        return value.ToString("N0");
    }
}
