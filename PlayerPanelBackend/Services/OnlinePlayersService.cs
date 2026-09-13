using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace PlayerPanelBackend.Services;

public record PlayerSample(string Name, string Id);
public record OnlineStatus(int Online, int Max, List<PlayerSample> Sample, string? Motd, string? Version);

/// <summary>
/// Queries a Minecraft server for its live player count using the same
/// "Server List Ping" protocol the vanilla client uses to show servers in
/// the multiplayer list. No plugin or server-side access is required —
/// this just talks to the public game port (25565 by default).
/// </summary>
public class OnlinePlayersService
{
    private readonly IConfiguration _config;
    private readonly ILogger<OnlinePlayersService> _logger;

    public OnlinePlayersService(IConfiguration config, ILogger<OnlinePlayersService> logger)
    {
        _config = config;
        _logger = logger;
    }

        public async Task<OnlineStatus?> QueryAsync(CancellationToken ct = default)
    {
        // The real production server is reached over the public internet, so a
        // single slow/lost packet shouldn't show as "offline" — retry once
        // before giving up.
        const int maxAttempts = 2;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var result = await QueryOnceAsync(ct);
            if (result != null) return result;
            if (attempt < maxAttempts) await Task.Delay(300, ct);
        }
        return null;
    }

    private async Task<OnlineStatus?> QueryOnceAsync(CancellationToken ct)
    {
        var host = _config["Minecraft:Host"]
            ?? throw new InvalidOperationException("Minecraft:Host is not configured.");
        var port = _config.GetValue<int>("Minecraft:Port", 25565);
        var timeoutMs = _config.GetValue<int>("Minecraft:TimeoutMs", 5000);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeoutMs);

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(host, port, cts.Token);
            using var stream = client.GetStream();

            // --- Handshake packet: protocol version, address, port, next state = 1 (status) ---
            using var handshake = new MemoryStream();
            WriteVarInt(handshake, -1); // protocol version: -1 works for a pure status ping
            WriteString(handshake, host);
            WriteUShort(handshake, (ushort)port);
            WriteVarInt(handshake, 1); // next state: status

            await WritePacketAsync(stream, 0x00, handshake.ToArray(), cts.Token);

            // --- Status request: empty payload ---
            await WritePacketAsync(stream, 0x00, Array.Empty<byte>(), cts.Token);

            // --- Read the status response packet ---
            var payload = await ReadPacketPayloadAsync(stream, cts.Token);
            var jsonString = ReadStringFromPayload(payload);

            using var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;
            var players = root.GetProperty("players");
            var online = players.GetProperty("online").GetInt32();
            var max = players.GetProperty("max").GetInt32();

            var sample = new List<PlayerSample>();
            if (players.TryGetProperty("sample", out var sampleArr) && sampleArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var p in sampleArr.EnumerateArray())
                {
                    sample.Add(new PlayerSample(
                        p.GetProperty("name").GetString() ?? "?",
                        p.TryGetProperty("id", out var idEl) ? idEl.GetString() ?? "" : ""));
                }
            }

            string? motd = null;
            if (root.TryGetProperty("description", out var desc))
            {
                motd = desc.ValueKind == JsonValueKind.String
                    ? desc.GetString()
                    : desc.TryGetProperty("text", out var t) ? t.GetString() : null;
            }

            string? version = null;
            if (root.TryGetProperty("version", out var ver) && ver.TryGetProperty("name", out var vname))
            {
                version = vname.GetString();
            }

            return new OnlineStatus(online, max, sample, motd, version);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Server list ping failed for {Host}:{Port}", host, port);
            return null;
        }
    }

    // ---- Minecraft protocol helpers (length-prefixed packets, VarInt-encoded) ----

    private static async Task WritePacketAsync(NetworkStream stream, int packetId, byte[] data, CancellationToken ct)
    {
        using var body = new MemoryStream();
        WriteVarInt(body, packetId);
        body.Write(data, 0, data.Length);

        using var full = new MemoryStream();
        WriteVarInt(full, (int)body.Length);
        body.Position = 0;
        await body.CopyToAsync(full, ct);

        var bytes = full.ToArray();
        await stream.WriteAsync(bytes, ct);
    }

    private static async Task<byte[]> ReadPacketPayloadAsync(NetworkStream stream, CancellationToken ct)
    {
        int length = await ReadVarIntAsync(stream, ct);
        var buffer = new byte[length];
        int read = 0;
        while (read < length)
        {
            int n = await stream.ReadAsync(buffer.AsMemory(read, length - read), ct);
            if (n == 0) throw new IOException("Connection closed while reading packet.");
            read += n;
        }

        using var ms = new MemoryStream(buffer);
        _ = ReadVarInt(ms); // packet id, not needed here
        var payload = new byte[length - (int)ms.Position];
        Array.Copy(buffer, (int)ms.Position, payload, 0, payload.Length);
        return payload;
    }

    private static string ReadStringFromPayload(byte[] payload)
    {
        using var ms = new MemoryStream(payload);
        int strLen = ReadVarInt(ms);
        var strBytes = new byte[strLen];
        var readTotal = 0;
        while (readTotal < strLen)
        {
            int n = ms.Read(strBytes, readTotal, strLen - readTotal);
            if (n == 0) break;
            readTotal += n;
        }
        return Encoding.UTF8.GetString(strBytes);
    }

    private static void WriteVarInt(Stream stream, int value)
    {
        uint uValue = (uint)value;
        do
        {
            byte b = (byte)(uValue & 0x7F);
            uValue >>= 7;
            if (uValue != 0) b |= 0x80;
            stream.WriteByte(b);
        } while (uValue != 0);
    }

    private static int ReadVarInt(Stream stream)
    {
        int numRead = 0, result = 0; byte read;
        do
        {
            int b = stream.ReadByte();
            if (b == -1) throw new IOException("Stream ended while reading VarInt.");
            read = (byte)b;
            result |= (read & 0x7F) << (7 * numRead);
            numRead++;
            if (numRead > 5) throw new InvalidOperationException("VarInt too big.");
        } while ((read & 0x80) != 0);
        return result;
    }

    private static async Task<int> ReadVarIntAsync(NetworkStream stream, CancellationToken ct)
    {
        int numRead = 0, result = 0; byte read;
        var single = new byte[1];
        do
        {
            int n = await stream.ReadAsync(single.AsMemory(0, 1), ct);
            if (n == 0) throw new IOException("Stream ended while reading VarInt.");
            read = single[0];
            result |= (read & 0x7F) << (7 * numRead);
            numRead++;
            if (numRead > 5) throw new InvalidOperationException("VarInt too big.");
        } while ((read & 0x80) != 0);
        return result;
    }

    private static void WriteString(Stream stream, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        WriteVarInt(stream, bytes.Length);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static void WriteUShort(Stream stream, ushort value)
    {
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)(value & 0xFF));
    }
}
