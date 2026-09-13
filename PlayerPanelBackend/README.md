# PlayerPanelBackend

Backend for the Cookie SMP panel (Next.js frontend). Serves two things:

- `GET /api/players` — live online player count, pulled directly from the Minecraft
  server via the Server List Ping protocol. No plugin needed, works right now,
  against the real production server.
- `GET /api/leaderboards` and `GET /api/leaderboards/{category}` — leaderboard data,
  synced periodically straight from the MySQL database ajLeaderboards is
  configured to use.

## Run it locally (full pipeline, no production access needed)

1. Start a local MySQL with sample data:
   ```bash
   docker compose up -d
   ```
   This creates 5 tables (`ajlb_statistic_player_kills`, `ajlb_statistic_deaths`,
   `ajlb_statistic_time_played`, `ajlb_statistic_mob_kills`,
   `ajlb_statistic_mine_block`) seeded with sample rows — see `sql/init.sql`.

2. Run the API:
   ```bash
   cd PlayerPanelBackend
   dotnet restore
   dotnet run
   ```
   It listens on `http://localhost:5225` (see `Properties/launchSettings.json`)
   and, in the `Development` environment (the default for `dotnet run`),
   automatically points at the docker-compose MySQL via
   `appsettings.Development.json` — nothing else to configure.

3. Check it:
   ```bash
   curl http://localhost:5225/api/players
   curl http://localhost:5225/api/leaderboards
   ```

## What's finished and working

- `Services/OnlinePlayersService.cs` — full Server List Ping implementation,
  used as-is against `cookie-smp.minetro.net:25565` (real server, no plugin needed).
- `Services/LeaderboardMySqlSyncService.cs` — connects to MySQL on a timer, reads
  each configured table, and refreshes an in-memory cache. Table/column names
  are entirely config-driven (`Leaderboards:Sources` in `appsettings.json`).

## Going to production: what changes, and what doesn't

The categories, table names, and column names in `appsettings.json` are our
best guess based on ajLeaderboards' documented `ajlb_` table-prefix
convention — **not yet confirmed against a real database**, because the
hosting plan couldn't create one (see below). Once a real MySQL database is
reachable from the game server and `cache_storage.yml` is pointed at it:

1. Let a few players rack up stats, then run `SHOW TABLES LIKE 'ajlb_%';`
   and `DESCRIBE <table>;` against the real database.
2. If the real table/column names differ from the guesses here, update the
   `TableName` / `NameColumn` / `ValueColumn` fields in
   `Leaderboards:Sources` — this is a config change, not a code change.
3. Double-check the `playtime` category's `Divisor: 20` — that assumes the
   stored value is in ticks (Minecraft's own convention), which is
   unverified until we see real numbers.
4. Point `Mysql:*` config at the real database (via environment variables or
   `dotnet user-secrets`, same pattern as below — **do not** commit real
   credentials).

```bash
dotnet user-secrets set "Mysql:Host" "<real host>"
dotnet user-secrets set "Mysql:Database" "<real database>"
dotnet user-secrets set "Mysql:Username" "<real username>"
dotnet user-secrets set "Mysql:Password" "<real password>"
```

### Why MySQL instead of reading files over SFTP

The original plan was to pull ajLeaderboards' output files over SFTP. That
turned out to be a dead end: by default the plugin stores its data in an H2
database file (`cache.mv.db`), which isn't a plain text/JSON file — reading
it needs a Java-specific library, not something to reach for in a .NET
backend. Switching `cache_storage.yml` to `method: mysql` and reading the
resulting tables directly is the supported alternative and is simpler
overall (no timed file transfers, no format-guessing parser, closer to
real-time).

The hosting plan for the actual game server (Minetro) doesn't allow
creating a database on it ("Databases cannot be created for this server."),
so the MySQL database itself needs to live somewhere else the game server
can reach — e.g. a database on the same machine as this backend, or a
managed MySQL service — with `cache_storage.yml`'s `ip` pointed at it.

## Money / Shards categories

Not wired up, same as before. No confirmed economy plugin is installed
(Vault is absent). Once a decision is made (install e.g. MoneySystem for a
Vault-free economy + a PAPI placeholder, or drop these categories from the
frontend), add/remove the corresponding entries in `Leaderboards:Sources`
and re-add `money`/`shards` to the frontend's category list.
