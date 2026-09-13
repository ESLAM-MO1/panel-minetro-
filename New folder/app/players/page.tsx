import PlayerHead from "@/components/PlayerHead";
import { getPlayers } from "@/lib/api";

export default async function PlayersPage() {
  const { serverInfo, players, online } = await getPlayers();

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Players Online</h1>
        <p className="mt-1 text-sm text-mutedForeground">
          {online
            ? `${serverInfo.playersOnline} of ${serverInfo.maxPlayers} players connected right now.`
            : "Couldn't reach the server right now — try again in a moment."}
        </p>
      </div>

      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
        {players.map((player) => (
          <div
            key={player.username}
            className="flex items-center gap-3 rounded-2xl border border-border bg-card p-4"
          >
            <div className="relative">
              <PlayerHead username={player.username} size={40} />
              <span className="absolute -bottom-1 -right-1 h-3 w-3 rounded-full border-2 border-card bg-online" />
            </div>
            <div className="min-w-0 flex-1">
              <p className="truncate text-sm font-semibold">
                {player.username}
              </p>
              {/* Ping/world/today's-playtime need a small in-game plugin to
                  expose — Server List Ping alone only gives us the name. */}
              <p className="text-xs text-mutedForeground">Online now</p>
            </div>
          </div>
        ))}

        {online && players.length === 0 && (
          <p className="col-span-full py-10 text-center text-sm text-mutedForeground">
            No players online right now.
          </p>
        )}

        {!online && (
          <p className="col-span-full py-10 text-center text-sm text-mutedForeground">
            Server status unavailable.
          </p>
        )}
      </div>
    </div>
  );
}
