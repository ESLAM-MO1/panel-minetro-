# Cookie SMP Dashboard

Next.js 14 (App Router) + TypeScript + Tailwind frontend for the Cookie SMP
panel. Talks to `PlayerPanelBackend` for real data (`lib/api.ts`);
`lib/mock-data.ts` is kept only as a reference for the original shapes and
is no longer imported anywhere.

## Run it

1. Make sure `PlayerPanelBackend` is running (see its README — `docker compose up -d`
   then `dotnet run`, listening on `http://localhost:5225` by default).
2. ```bash
   npm install
   npm run dev
   ```
3. Open http://localhost:3000

`.env.local` already points `NEXT_PUBLIC_API_URL` at `http://localhost:5225`
for local dev — change it (or set it in your hosting provider) for staging/production.

## Pages

- `/` — Home (server hero, IP, quick leaderboard preview)
- `/leaderboards` — full leaderboards with category tabs + search
- `/players` — players currently online

## File map

```
app/layout.tsx              root layout, fonts, navbar
app/globals.css             base styles
app/page.tsx                Home page (Server Component, fetches from lib/api.ts)
app/leaderboards/page.tsx   Leaderboards route (wraps client component)
app/leaderboards/LeaderboardsClient.tsx   tabs + search + fetches leaderboard data
app/players/page.tsx        Players Online page (Server Component)
components/Navbar.tsx       top navigation
components/PlayerHead.tsx   Minecraft head avatar (mc-heads.net)
components/RankBadge.tsx    gold/silver/bronze rank badge
components/LeaderboardCard.tsx   "Top X" preview card (used on Home)
lib/api.ts                  real data layer -- fetches from PlayerPanelBackend
lib/mock-data.ts            OLD placeholder data, unused, kept for reference only
tailwind.config.ts          design tokens extracted from the reference site's CSS
```

## Known gaps (tracked in PlayerPanelBackend's README too)

- **Money / Shards** categories have no backend data yet (no economy plugin
  decided) — the tab renders "not tracked on the server yet".
- **Placed blocks** has no confirmed PlaceholderAPI placeholder added yet,
  same "not tracked yet" state.
- Online-player cards show name + avatar only — ping/world/today's-playtime
  need a small in-game plugin to expose (Server List Ping alone can't give
  us those).

## Design tokens (extracted from the reference site's real CSS, dark mode)

| Token | Hex |
|---|---|
| background | #000000 |
| card | #1C1C1C |
| muted | #262626 |
| border | #333333 |
| primary / accent | #A91955 |
| accent text (hover/links) | #F04C90 |
| highlight (gold) | #F99406 |
| online (green) | #28AF60 |
| tier: diamond | #0A76A9 |
| tier: media | #B52DCD |
| tier: ruby | #D9174E |
| tier: emerald | #0B7F58 |

Fonts: Inter (UI text) + JetBrains Mono (numbers/stats).
