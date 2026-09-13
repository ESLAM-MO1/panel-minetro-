import LeaderboardCard from "@/components/LeaderboardCard";
import { getLeaderboards, getPlayers } from "@/lib/api";

export default async function HomePage() {
  const [{ serverInfo, online }, leaderboards] = await Promise.all([
    getPlayers(),
    getLeaderboards(),
  ]);

  return (
    <div className="space-y-10">
      <section className="rounded-2xl border border-border bg-gradient-to-br from-card to-background p-8 text-center">
        <span
          className={`inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-xs font-medium ${
            online ? "bg-online/15 text-online" : "bg-muted text-mutedForeground"
          }`}
        >
          <span
            className={`h-1.5 w-1.5 rounded-full ${online ? "bg-online" : "bg-mutedForeground"}`}
          />
          {online ? "Server online" : "Status unavailable"}
        </span>

        <h1 className="mt-4 text-4xl font-bold tracking-tight">
          {serverInfo.name}
        </h1>
        <p className="mx-auto mt-2 max-w-md text-sm text-mutedForeground">
          Track every player&apos;s kills, playtime, mob kills and blocks, and
          see who&apos;s online right now.
        </p>

        <div className="mx-auto mt-6 flex max-w-sm items-center justify-between gap-3 rounded-xl border border-border bg-card px-4 py-3">
          <span className="font-mono text-sm">{serverInfo.ip}</span>
          <button className="rounded-lg bg-primary px-3 py-1.5 text-xs font-semibold hover:bg-accentText">
            Copy
          </button>
        </div>

        <div className="mx-auto mt-6 flex max-w-sm justify-center gap-8 text-sm">
          <div>
            <p className="font-mono text-lg font-bold tabular-nums">
              {online ? `${serverInfo.playersOnline}/${serverInfo.maxPlayers}` : "—"}
            </p>
            <p className="text-xs text-mutedForeground">Players online</p>
          </div>
          <div>
            <p className="font-mono text-lg font-bold">
              {online ? serverInfo.version : "—"}
            </p>
            <p className="text-xs text-mutedForeground">Server version</p>
          </div>
        </div>
      </section>

      <section>
        <h2 className="mb-4 text-lg font-semibold">Top players right now</h2>
        <div className="grid gap-4 md:grid-cols-2">
          <LeaderboardCard
            title="Top PvP Players"
            subtitle="Players with the most kills"
            statLabel="Kills"
            entries={leaderboards.kills ?? []}
            href="/leaderboards?tab=kills"
          />
          <LeaderboardCard
            title="Top Active Players"
            subtitle="Players with the most playtime"
            statLabel="Playtime"
            entries={leaderboards.playtime ?? []}
            href="/leaderboards?tab=playtime"
          />
        </div>
      </section>
    </div>
  );
}
