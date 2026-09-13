using System.Collections.Concurrent;

namespace PlayerPanelBackend.Services;

public class PlayerNameResolver
{
    private readonly ConcurrentDictionary<string, string> _uuidToName = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> _nameToUuid = new(StringComparer.OrdinalIgnoreCase);

    public void Learn(string uuid, string username)
    {
        if (string.IsNullOrWhiteSpace(uuid) || string.IsNullOrWhiteSpace(username)) return;
        _uuidToName[uuid] = username;
        _nameToUuid[username] = uuid;
    }

    public string? TryResolve(string uuid) =>
        _uuidToName.TryGetValue(uuid, out var name) ? name : null;

    public string? TryResolveUuid(string username) =>
        _nameToUuid.TryGetValue(username, out var uuid) ? uuid : null;

    public int KnownCount => _uuidToName.Count;
}