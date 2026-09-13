using PlayerPanelBackend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<OnlinePlayersService>();
builder.Services.AddSingleton<LeaderboardCache>();
builder.Services.AddHostedService<LeaderboardMySqlSyncService>();

// Allow the Next.js frontend (different origin in dev/prod) to call this API.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000" };
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors();

// GET /api/players -> matches lib/mock-data.ts `onlinePlayers` + `serverInfo` shape
app.MapGet("/api/players", async (OnlinePlayersService svc) =>
{
    var status = await svc.QueryAsync();
    if (status is null)
        return Results.StatusCode(502);

    return Results.Ok(new
    {
        playersOnline = status.Online,
        maxPlayers = status.Max,
        version = status.Version,
        motd = status.Motd,
        // Note: the vanilla Server List Ping "sample" field only gives a
        // partial list of names (usually up to 12) — not ping, world, or
        // per-player playtime. Those extra fields the frontend's
        // OnlinePlayer type expects (ping, playtimeToday, world) need a
        // small in-game plugin if you want them accurate; otherwise this
        // endpoint can only reliably report the total count + name sample.
        players = status.Sample.Select(p => new { username = p.Name })
    });
});

// GET /api/leaderboards -> matches lib/mock-data.ts `leaderboards` shape (per category)
app.MapGet("/api/leaderboards", (LeaderboardCache cache) =>
{
    if (!cache.HasData)
        return Results.StatusCode(503);

    return Results.Ok(cache.GetAll());
});

// GET /api/leaderboards/{category} -> single category, e.g. /api/leaderboards/kills
app.MapGet("/api/leaderboards/{category}", (string category, LeaderboardCache cache) =>
{
    var entries = cache.GetCategory(category);
    return entries is null ? Results.NotFound() : Results.Ok(entries);
});

app.Run();
