using System.Collections.Concurrent;
using System.Text.Json;

namespace PlayerPanelBackend.Services;

public class PlayerNameResolver
{
    private readonly ConcurrentDictionary<string, string> _uuidToName = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> _nameToUuid = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _storePath;
    private readonly object _fileLock = new();

    public PlayerNameResolver(IConfiguration config)
    {
        _storePath = config["PlayerNames:StorePath"] ?? "player-names.json";
        Load();
    }

    public void Learn(string uuid, string username)
    {
        if (string.IsNullOrWhiteSpace(uuid) || string.IsNullOrWhiteSpace(username)) return;

        var isNew = !_uuidToName.TryGetValue(uuid, out var existing) ||
                    !string.Equals(existing, username, StringComparison.OrdinalIgnoreCase);

        _uuidToName[uuid] = username;
        _nameToUuid[username] = uuid;

        if (isNew) Save();
    }

    public string? TryResolve(string uuid) =>
        _uuidToName.TryGetValue(uuid, out var name) ? name : null;

    public string? TryResolveUuid(string username) =>
        _nameToUuid.TryGetValue(username, out var uuid) ? uuid : null;

    public int KnownCount => _uuidToName.Count;

    private void Load()
    {
        try
        {
            if (!File.Exists(_storePath)) return;
            var json = File.ReadAllText(_storePath);
            var map = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (map is null) return;
            foreach (var (uuid, name) in map)
            {
                _uuidToName[uuid] = name;
                _nameToUuid[name] = uuid;
            }
        }
        catch
        {
            // Corrupt or unreadable file -- start fresh rather than crash the app.
        }
    }

    private void Save()
    {
        try
        {
            lock (_fileLock)
            {
                var json = JsonSerializer.Serialize(_uuidToName);
                File.WriteAllText(_storePath, json);
            }
        }
        catch
        {
            // Best-effort persistence -- an occasional failed write just means
            // that one mapping isn't durable yet, not a reason to crash.
        }
    }
}