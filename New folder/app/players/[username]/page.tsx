import Link from "next/link";
import { getPlayerProfile, type LeaderboardCategory } from "@/lib/api";

const statLabels: Record<LeaderboardCategory, string> = {
  kills: "Kills",
  deaths: "Deaths",
  playtime: "Playtime",
  mobKills: "Mob Kills",
  brokenBlocks: "Blocks Broken",
  placedBlocks: "Blocks Placed",
  money: "Money",
  shards: "Shards",
};

const statOrder: LeaderboardCategory[] = [
  "kills",
  "deaths",
  "playtime",
  "mobKills",
  "brokenBlocks",
  "placedBlocks",
  "money",
  "shards",
];

export default async function PlayerProfilePage({
  params,
}: {
  params: { username: string };
}) {
  const username = decodeURIComponent(params.username);
  const profile = await getPlayerProfile(username);

  if (!profile) {
    return (
      <div className="mx-auto max-w-lg space-y-4 py-16 text-center">
        <h1 className="text-2xl font-bold">Player not found yet</h1>
        <p className="text-sm text-mutedForeground">
          We only know a player's stats once they've been online while the
          panel is running. If <span className="font-mono">{username}</span>{" "}
          is a real player on the server, ask them to join once — their
          profile will start working right after.
        </p>
        <Link
          href="/players"
          className="inline-block rounded-lg border border-border px-4 py-2 text-sm hover:bg-muted/50"
        >
          Back to Players Online
        </Link>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col items-center gap-4 rounded-2xl border border-border bg-gradient-to-br from-card to-background p-8 text-center sm:flex-row sm:text-left">
        <img
          src={`https://mc-heads.net/body/${encodeURIComponent(profile.username)}/100`}
          alt={`${profile.username}'s Minecraft skin`}
          width={100}
          height={182}
          className="drop-shadow-lg"
        />
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            {profile.username}
          </h1>
          <p className="mt-1 font-mono text-xs text-mutedForeground">
            {profile.uuid}
          </p>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
        {statOrder.map((key) => {
          const value = profile.stats[key];
          return (
            <div
              key={key}
              className="rounded-xl border border-border bg-card p-4"
            >
              <p className="text-[11px] uppercase tracking-wide text-mutedForeground">
                {statLabels[key]}
              </p>
              <p className="mt-1 font-mono text-lg font-bold tabular-nums">
                {value ?? "—"}
              </p>
            </div>
          );
        })}
      </div>
    </div>
  );
}