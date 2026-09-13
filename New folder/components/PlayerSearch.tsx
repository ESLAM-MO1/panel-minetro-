"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";

export default function PlayerSearch({
  placeholder = "Look up a player by username...",
}: {
  placeholder?: string;
}) {
  const [value, setValue] = useState("");
  const router = useRouter();

  const go = () => {
    const name = value.trim();
    if (name.length < 3) return;
    router.push(`/players/${encodeURIComponent(name)}`);
  };

  return (
    <div className="flex gap-2">
      <input
        value={value}
        onChange={(e) => setValue(e.target.value)}
        onKeyDown={(e) => e.key === "Enter" && go()}
        placeholder={placeholder}
        className="w-full rounded-xl border border-border bg-card px-4 py-2.5 text-sm placeholder:text-mutedForeground focus:outline-none focus:ring-1 focus:ring-primary"
      />
      <button
        onClick={go}
        className="rounded-xl bg-primary px-5 text-sm font-semibold hover:bg-accentText"
      >
        Search
      </button>
    </div>
  );
}