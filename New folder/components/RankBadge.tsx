const medalStyles: Record<number, string> = {
  1: "bg-highlight text-black",
  2: "bg-mutedForeground/40 text-white",
  3: "bg-tierRuby/70 text-white",
};

export default function RankBadge({ rank }: { rank: number }) {
  const style = medalStyles[rank];

  if (style) {
    return (
      <span
        className={`flex h-6 w-6 shrink-0 items-center justify-center rounded-full text-xs font-bold ${style}`}
      >
        {rank}
      </span>
    );
  }

  return (
    <span className="flex h-6 w-6 shrink-0 items-center justify-center text-xs font-medium text-mutedForeground">
      {rank}
    </span>
  );
}
