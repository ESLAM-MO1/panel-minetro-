import Link from "next/link";
import PlayerHead from "./PlayerHead";
import RankBadge from "./RankBadge";
import type { LeaderboardEntry } from "@/lib/api";

export default function LeaderboardCard({
  title,
  subtitle,
  statLabel,
  entries,
  href,
}: {
  title: string;
  subtitle: string;
  statLabel: string;
  entries: LeaderboardEntry[];
  href: string;
}) {
  return (
    <div className="rounded-2xl border border-border bg-card p-5">
      <Link href={href} className="hover:text-accentText">
        <h3 className="text-base font-semibold">{title}</h3>
      </Link>
      <p className="mt-0.5 text-sm text-mutedForeground">{subtitle}</p>

      {entries.length === 0 && (
        <p className="mt-4 text-sm text-mutedForeground">
          No data yet — check back once the backend has synced.
        </p>
      )}

      <ul className="mt-4 space-y-1">
        {entries.map((entry) => (
          <li
            key={entry.username}
            className="flex items-center gap-3 rounded-lg px-2 py-1.5 hover:bg-muted/50"
          >
            <RankBadge rank={entry.rank} />
            <PlayerHead username={entry.username} size={28} />
            <span className="flex-1 truncate text-sm font-medium">
              {entry.username}
            </span>
            <span className="text-right">
              <span className="block text-[10px] uppercase tracking-wide text-mutedForeground">
                {statLabel}
              </span>
              <span className="font-mono text-sm tabular-nums">
                {entry.value}
              </span>
            </span>
          </li>
        ))}
      </ul>

      <Link
        href={href}
        className="mt-3 block rounded-lg border border-border py-2 text-center text-sm text-mutedForeground hover:bg-muted/50 hover:text-foreground"
      >
        See the full leaderboard
      </Link>
    </div>
  );
}
