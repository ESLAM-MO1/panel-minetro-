"use client";

import { useEffect, useMemo, useState } from "react";
import { useSearchParams } from "next/navigation";
import PlayerHead from "@/components/PlayerHead";
import RankBadge from "@/components/RankBadge";
import {
  categories,
  getLeaderboards,
  type LeaderboardCategory,
  type LeaderboardEntry,
} from "@/lib/api";

const categoryLabels: Record<LeaderboardCategory, { title: string; stat: string }> = {
  money: { title: "Top Money Holders", stat: "Money" },
  kills: { title: "Top PvP Players", stat: "Kills" },
  deaths: { title: "Top Deaths", stat: "Deaths" },
  playtime: { title: "Top Active Players", stat: "Playtime" },
  shards: { title: "Top Shard Collectors", stat: "Shards" },
  placedBlocks: { title: "Top Builders", stat: "Blocks" },
  brokenBlocks: { title: "Top Block Breakers", stat: "Broken" },
  mobKills: { title: "Top Mob Hunters", stat: "Mobs" },
};

export default function LeaderboardsClient() {
  const searchParams = useSearchParams();
  const initialTab =
    (searchParams.get("tab") as LeaderboardCategory) || "kills";

  const [activeTab, setActiveTab] = useState<LeaderboardCategory>(initialTab);
  const [search, setSearch] = useState("");
  const [data, setData] = useState<Partial<Record<LeaderboardCategory, LeaderboardEntry[]>>>({});
  const [status, setStatus] = useState<"loading" | "ready" | "error">("loading");

  useEffect(() => {
    let cancelled = false;

    getLeaderboards()
      .then((result) => {
        if (cancelled) return;
        setData(result);
        setStatus("ready");
      })
      .catch(() => {
        if (!cancelled) setStatus("error");
      });

    return () => {
      cancelled = true;
    };
  }, []);

  const activeList = data[activeTab];

  const entries = useMemo(() => {
    const list = activeList ?? [];
    if (!search.trim()) return list;
    return list.filter((entry) =>
      entry.username.toLowerCase().includes(search.trim().toLowerCase())
    );
  }, [activeList, search]);

  const active = categoryLabels[activeTab];
  const isTracked = activeList !== undefined;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">
          Game Server Leaderboards
        </h1>
        <p className="mt-1 text-sm text-mutedForeground">
          Browse the top Game Server players ranked by kills, deaths, playtime,
          mob kills, and blocks.
        </p>
      </div>

      <div className="flex flex-wrap gap-2">
        {categories.map((category) => (
          <button
            key={category.key}
            onClick={() => setActiveTab(category.key)}
            className={`rounded-full px-4 py-1.5 text-sm font-medium transition-colors ${
              activeTab === category.key
                ? "bg-primary text-white"
                : "bg-muted text-mutedForeground hover:text-foreground"
            }`}
          >
            {category.label}
          </button>
        ))}
      </div>

      <div className="flex gap-2">
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder={`Search top ${activeList?.length ?? 0} ranked players...`}
          className="w-full rounded-xl border border-border bg-card px-4 py-2.5 text-sm placeholder:text-mutedForeground focus:outline-none focus:ring-1 focus:ring-primary"
        />
        <button className="rounded-xl bg-primary px-5 text-sm font-semibold hover:bg-accentText">
          Search
        </button>
      </div>

      <div className="rounded-2xl border border-border bg-card">
        <div className="border-b border-border px-5 py-4">
          <h2 className="text-base font-semibold">{active.title}</h2>
        </div>

        {status === "loading" && (
          <p className="px-5 py-8 text-center text-sm text-mutedForeground">
            Loading leaderboard...
          </p>
        )}

        {status === "error" && (
          <p className="px-5 py-8 text-center text-sm text-mutedForeground">
            Couldn&apos;t reach the backend. Make sure it&apos;s running on{" "}
            {process.env.NEXT_PUBLIC_API_URL || "http://localhost:5225"}.
          </p>
        )}

        {status === "ready" && !isTracked && (
          <p className="px-5 py-8 text-center text-sm text-mutedForeground">
            This category isn&apos;t tracked on the server yet.
          </p>
        )}

        {status === "ready" && isTracked && (
          <ul className="divide-y divide-border">
            {entries.map((entry) => (
              <li
                key={entry.username}
                className="flex items-center gap-4 px-5 py-3 hover:bg-muted/50"
              >
                <RankBadge rank={entry.rank} />
                <PlayerHead username={entry.username} size={32} />
                <span className="flex-1 truncate text-sm font-medium">
                  {entry.username}
                </span>
                <span className="text-right">
                  <span className="block text-[10px] uppercase tracking-wide text-mutedForeground">
                    {active.stat}
                  </span>
                  <span className="font-mono text-sm tabular-nums">
                    {entry.value}
                  </span>
                </span>
              </li>
            ))}
            {entries.length === 0 && (
              <li className="px-5 py-8 text-center text-sm text-mutedForeground">
                No players match &quot;{search}&quot;.
              </li>
            )}
          </ul>
        )}
      </div>
    </div>
  );
}
