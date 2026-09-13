namespace PlayerPanelBackend.Services;

/// <summary>
/// Holds the most recently synced leaderboard data in memory so API requests
/// never wait on an SFTP round trip. Updated by LeaderboardSyncService.
/// </summary>
public class LeaderboardCache
{
    private readonly object _lock = new();
    private Dictionary<string, List<LeaderboardEntry>> _byCategory = new();

    public DateTime LastUpdatedUtc { get; private set; }

    public void Set(Dictionary<string, List<LeaderboardEntry>> data)
    {
        lock (_lock)
        {
            _byCategory = data;
            LastUpdatedUtc = DateTime.UtcNow;
        }
    }

    public IReadOnlyDictionary<string, List<LeaderboardEntry>> GetAll()
    {
        lock (_lock)
        {
            return _byCategory;
        }
    }

    public List<LeaderboardEntry>? GetCategory(string category)
    {
        lock (_lock)
        {
            return _byCategory.TryGetValue(category, out var list) ? list : null;
        }
    }

    public bool HasData
    {
        get { lock (_lock) { return _byCategory.Count > 0; } }
    }
}
