// Real data layer, talking to PlayerPanelBackend. Replaces lib/mock-data.ts,
// which is kept in the repo only as a reference for the original shapes.

export type LeaderboardCategory =
  | "money"
  | "kills"
  | "deaths"
  | "playtime"
  | "shards"
  | "placedBlocks"
  | "brokenBlocks"
  | "mobKills";

// All categories the UI knows how to display. Not all of them are
// necessarily tracked on the server yet (see PlayerPanelBackend's README —
// money/shards have no economy plugin decided, placedBlocks has no
// confirmed placeholder yet). Categories missing from the API response
// render as "not tracked yet" instead of being hidden, so it's obvious
// what still needs a plugin/placeholder decision.
export const categories: { key: LeaderboardCategory; label: string }[] = [
  { key: "kills", label: "Kills" },
  { key: "deaths", label: "Deaths" },
  { key: "playtime", label: "Playtime" },
  { key: "mobKills", label: "Mob Kills" },
  { key: "brokenBlocks", label: "Blocks Broken" },
  { key: "placedBlocks", label: "Blocks Placed" },
  { key: "money", label: "Money" },
  { key: "shards", label: "Shards" },
];

export interface LeaderboardEntry {
  rank: number;
  username: string;
  value: string;
}

export interface OnlinePlayer {
  username: string;
  // The vanilla Server List Ping protocol (what /api/players uses) only
  // gives us a name sample — no ping, world, or per-player playtime.
  // Those stay undefined until a small in-game plugin exposes them.
  ping?: number;
  playtimeToday?: string;
  world?: string;
}

export interface ServerInfo {
  name: string;
  ip: string;
  playersOnline: number;
  maxPlayers: number;
  version: string;
}

const API_BASE = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5225";

export const FALLBACK_SERVER_INFO: ServerInfo = {
  name: "Koki Server",
  ip: "cookie-smp.minetro.net",
  playersOnline: 0,
  maxPlayers: 0,
  version: "Unknown",
};

interface PlayersResponse {
  serverInfo: ServerInfo;
  players: OnlinePlayer[];
  online: boolean;
}

/** GET /api/players — live server status + online player sample. */
export async function getPlayers(): Promise<PlayersResponse> {
  try {
    const res = await fetch(`${API_BASE}/api/players`, { cache: "no-store" });
    if (!res.ok) {
      return { serverInfo: FALLBACK_SERVER_INFO, players: [], online: false };
    }

    const data = await res.json();
    return {
      serverInfo: {
        name: "Koki Server",
        ip: "cookie-smp.minetro.net",
        playersOnline: data.playersOnline ?? 0,
        maxPlayers: data.maxPlayers ?? 0,
        version: data.version ?? "Unknown",
      },
      players: (data.players ?? []).map((p: { username: string }) => ({
        username: p.username,
      })),
      online: true,
    };
  } catch {
    return { serverInfo: FALLBACK_SERVER_INFO, players: [], online: false };
  }
}

/**
 * GET /api/leaderboards — every category the backend currently has data
 * for. Categories not tracked yet are simply absent from the result;
 * callers should treat a missing key as "not tracked yet", not an error.
 */
export async function getLeaderboards(): Promise<
  Partial<Record<LeaderboardCategory, LeaderboardEntry[]>>
> {
  try {
    const res = await fetch(`${API_BASE}/api/leaderboards`, {
      cache: "no-store",
    });
    if (!res.ok) return {};
    return (await res.json()) as Partial<
      Record<LeaderboardCategory, LeaderboardEntry[]>
    >;
  } catch {
    return {};
  }
}


export interface PlayerProfile {
  username: string;
  uuid: string;
  stats: Partial<Record<LeaderboardCategory, string>>;
}

export async function getPlayerProfile(
  username: string
): Promise<PlayerProfile | null> {
  try {
    const res = await fetch(
      `${API_BASE}/api/players/${encodeURIComponent(username)}`,
      { cache: "no-store" }
    );
    if (!res.ok) return null;
    return (await res.json()) as PlayerProfile;
  } catch {
    return null;
  }
}