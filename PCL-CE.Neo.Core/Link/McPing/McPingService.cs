using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Link.McPing.Model;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.Link.McPing;

public class McPingService : IMcPingService
{
    private readonly IPEndPoint _endpoint;
    private readonly string _host;
    private const int DefaultTimeout = 10000;
    private readonly int _timeout;

    public IPEndPoint Endpoint => _endpoint;
    public string Host => _host;
    public int Timeout => _timeout;

    public McPingService(IPEndPoint endpoint) : this(endpoint, DefaultTimeout)
    {
    }

    public McPingService(IPEndPoint endpoint, int timeout)
    {
        _endpoint = endpoint;
        _host = endpoint.Address.ToString();
        _timeout = timeout;
    }

    public McPingService(string ip, int port = 25565, int timeout = DefaultTimeout)
    {
        _endpoint = IPAddress.TryParse(ip, out var ipAddress)
            ? new IPEndPoint(ipAddress, port)
            : new IPEndPoint(Dns.GetHostAddresses(ip).First(), port);
        _host = ip;
        _timeout = timeout;
    }

    public McPingService(string host, IPEndPoint endpoint, int timeout = DefaultTimeout)
    {
        _endpoint = endpoint;
        _host = host;
        _timeout = timeout;
    }

    public async Task<McPingResult?> PingAsync(CancellationToken cancellationToken = default)
    {
        var startTime = DateTimeOffset.UtcNow;
        
        try
        {
            using var timeoutCts = new CancellationTokenSource(_timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            
            using var client = new TcpClient();
            await client.ConnectAsync(_endpoint.Address, _endpoint.Port, linkedCts.Token);

            using var stream = client.GetStream();
            stream.ReadTimeout = _timeout;

            var handshake = CreateHandshake(_host, _endpoint.Port);
            await stream.WriteAsync(handshake, linkedCts.Token);

            var statusRequest = CreateStatusRequest();
            await stream.WriteAsync(statusRequest, linkedCts.Token);

            var length = await ReadVarIntAsync(stream, linkedCts.Token);
            if (length <= 0) return null;

            var response = new byte[length];
            await ReadExactAsync(stream, response, linkedCts.Token);

            var latency = (DateTimeOffset.UtcNow - startTime).Milliseconds;
            return ParseResponse(response, latency);
        }
        catch (Exception ex)
        {
            LogWrapper.Debug("McPing", $"Ping failed for {_endpoint}: {ex.Message}");
            return null;
        }
    }

    private static byte[] CreateHandshake(string address, int port)
    {
        var packet = new List<byte>();

        packet.AddRange(WriteVarInt(0));
        packet.AddRange(WriteVarInt(47));
        packet.AddRange(WriteVarInt(address.Length));
        packet.AddRange(Encoding.UTF8.GetBytes(address));
        packet.AddRange(WriteShort((short)port));
        packet.AddRange(WriteVarInt(1));

        var data = packet.ToArray();
        var result = new List<byte>();
        result.AddRange(WriteVarInt(data.Length));
        result.AddRange(data);

        return result.ToArray();
    }

    private static byte[] CreateStatusRequest()
    {
        var result = new List<byte>();
        result.AddRange(WriteVarInt(1));
        result.AddRange(WriteVarInt(0));
        return result.ToArray();
    }

    private static async Task<int> ReadVarIntAsync(Stream stream, CancellationToken cancellationToken)
    {
        int value = 0;
        int shift = 0;
        while (true)
        {
            var b = (byte)stream.ReadByte();
            if (b == 0 && stream.Position == 0) return -1;
            value |= (b & 0x7F) << shift;
            if ((b & 0x80) == 0) break;
            shift += 7;
            if (shift >= 32) throw new InvalidOperationException("VarInt too long");
            cancellationToken.ThrowIfCancellationRequested();
        }
        return value;
    }

    private static async Task ReadExactAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        int offset = 0;
        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer, offset, buffer.Length - offset, cancellationToken);
            if (read == 0) break;
            offset += read;
        }
    }

    private static byte[] WriteVarInt(int value)
    {
        var result = new List<byte>();
        while (true)
        {
            if ((value & ~0x7F) == 0)
            {
                result.Add((byte)value);
                return result.ToArray();
            }
            result.Add((byte)((value & 0x7F) | 0x80));
            value >>= 7;
        }
    }

    private static byte[] WriteShort(short value)
    {
        return new byte[] { (byte)(value >> 8), (byte)(value & 0xFF) };
    }

    private static McPingResult? ParseResponse(byte[] response, long latency)
    {
        try
        {
            var json = Encoding.UTF8.GetString(response);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var description = root.GetProperty("description").GetString() ?? string.Empty;
            
            var versionObj = root.GetProperty("version");
            var version = new McPingVersionResult(
                Name: versionObj.GetProperty("name").GetString() ?? string.Empty,
                Protocol: versionObj.GetProperty("protocol").GetInt32()
            );

            var playersObj = root.GetProperty("players");
            var players = new McPingPlayerResult(
                Max: playersObj.GetProperty("max").GetInt32(),
                Online: playersObj.GetProperty("online").GetInt32(),
                Samples: playersObj.TryGetProperty("sample", out var sample) && sample.ValueKind == JsonValueKind.Array
                    ? sample.EnumerateArray().Select(p => new McPingPlayerSampleResult(
                        Name: p.GetProperty("name").GetString() ?? string.Empty,
                        Id: p.GetProperty("id").GetString() ?? string.Empty
                    )).ToList()
                    : null
            );

            var favicon = root.TryGetProperty("favicon", out var faviconProp) ? faviconProp.GetString() : null;
            
            McPingModInfoResult? modInfo = null;
            if (root.TryGetProperty("modinfo", out var modInfoProp))
            {
                modInfo = new McPingModInfoResult(
                    Type: modInfoProp.GetProperty("type").GetString() ?? string.Empty,
                    ModList: modInfoProp.GetProperty("modList").EnumerateArray()
                        .Select(m => new McPingModInfoModResult(
                            Id: m.GetProperty("modid").GetString() ?? string.Empty,
                            Version: m.GetProperty("version").GetString() ?? string.Empty
                        )).ToList()
                );
            }

            var preventsChatReports = root.TryGetProperty("preventsChatReports", out var pcrProp) && pcrProp.ValueKind == JsonValueKind.True;

            return new McPingResult(
                Version: version,
                Players: players,
                Description: description,
                Favicon: favicon,
                Latency: latency,
                ModInfo: modInfo,
                PreventsChatReports: preventsChatReports
            );
        }
        catch
        {
            return null;
        }
    }
}