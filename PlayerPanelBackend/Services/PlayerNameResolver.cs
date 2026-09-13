using System.Collections.Concurrent;

namespace PlayerPanelBackend.Services;

/// <summary>
/// ajLeaderboards' `ajlb add` command refuses to store the %player_name%
/// placeholder because it isn't numeric, so the shared "ajlb_extras" table
/// only ever has a player's UUID, never their username.
///
/// Instead of asking the plugin for names, we get them for free from
/// another source we already query: the Server List Ping response's
/// player "sample" includes each online player's UUID *and* name
/// (OnlinePlayersService / PlayerSample). Every time we see a player
/// online, we remember that (UUID -> name) mapping here. Over time, as
/// players log in, this cache fills in and leaderboard rows resolve to
/// real usernames instead of being skipped. Not persisted across restarts,
/// but recovers itself within a few minutes of the server having any
/// players online.
/// </summary>
public class PlayerNameResolver
{
    private readonly ConcurrentDictionary<string, string> _uuidToName = new(StringComparer.OrdinalIgnoreCase);

    public void Learn(string uuid, string username)
    {
        if (string.IsNullOrWhiteSpace(uuid) || string.IsNullOrWhiteSpace(username)) return;
        _uuidToName[uuid] = username;
    }

    public string? TryResolve(string uuid) =>
        _uuidToName.TryGetValue(uuid, out var name) ? name : null;

    public int KnownCount => _uuidToName.Count;
}