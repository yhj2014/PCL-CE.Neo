using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Link;

public class McPingService : IMcPingService
{
    private readonly ILogger<McPingService> _logger;
    private readonly IPEndPoint _endpoint;
    private readonly string _host;
    private readonly int _timeout;
    private bool _disposed;
    private const string ModuleName = "McPing";

    public IPEndPoint Endpoint => _endpoint;
    public string Host => _host;
    public int Timeout => _timeout;

    public McPingService(string host, int port = 25565, int timeout = 5000)
        : this(host, new IPEndPoint(Dns.GetHostAddresses(host).First(), port), timeout)
    {
    }

    public McPingService(string host, IPEndPoint endpoint, int timeout = 5000)
    {
        _logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<McPingService>();
        _endpoint = endpoint;
        _host = host;
        _timeout = timeout;
    }

    public McPingService(ILogger<McPingService> logger, string host, int port = 25565, int timeout = 5000)
        : this(logger, host, new IPEndPoint(Dns.GetHostAddresses(host).First(), port), timeout)
    {
    }

    public McPingService(ILogger<McPingService> logger, string host, IPEndPoint endpoint, int timeout = 5000)
    {
        _logger = logger;
        _endpoint = endpoint;
        _host = host;
        _timeout = timeout;
    }

    public async Task<PingResult?> PingAsync(CancellationToken cancellationToken = default)
    {
        using var so = new Socket(SocketType.Stream, ProtocolType.Tcp);
        using var timeoutCts = new CancellationTokenSource(_timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            _logger.LogDebug("Connecting to {Endpoint}", _endpoint);
            await so.ConnectAsync(_endpoint.Address, _endpoint.Port, linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Connection timeout to {Endpoint}", _endpoint);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to {Endpoint}", _endpoint);
            return null;
        }

        _logger.LogDebug("Connection established: {Endpoint}", _endpoint);
        
        try
        {
            await using var stream = new NetworkStream(so, false);

            var handshakePacket = BuildHandshakePacket(_host, _endpoint.Port, 762);
            var statusPacket = BuildStatusRequestPacket();

            await stream.WriteAsync(handshakePacket, linkedCts.Token);
            _logger.LogDebug("Handshake sent, packet length: {Length}", handshakePacket.Length);

            await stream.WriteAsync(statusPacket, linkedCts.Token);
            _logger.LogDebug("Status request sent, packet length: {Length}", statusPacket.Length);

            var pingTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var pingPacket = BuildPingRequestPacket(pingTimestamp);
            
            await stream.WriteAsync(pingPacket, linkedCts.Token);
            _logger.LogDebug("Ping sent");

            var (statusPayload, latency) = await ReadStatusPayloadAsync(stream, linkedCts.Token);
            
            so.Close();

            if (statusPayload == null || statusPayload.Length == 0)
            {
                _logger.LogError("Server returned empty status payload");
                return null;
            }

            var responseJson = Encoding.UTF8.GetString(statusPayload);
            var retJson = JsonNode.Parse(responseJson);
            
            if (retJson == null)
            {
                _logger.LogError("Failed to parse server response as JSON");
                return null;
            }

            if (retJson["description"] is JsonObject descObj)
            {
                retJson["description"] = ConvertJNodeToMcString(descObj);
            }

            var version = retJson["version"];
            var players = retJson["players"];
            var description = retJson["description"]?.ToString();
            
            var result = new PingResult
            {
                Success = true,
                Protocol = PingProtocol.Modern,
                Latency = latency,
                VersionName = version?["name"]?.ToString(),
                ProtocolVersion = version?["protocol"]?.GetValue<int>() ?? 0,
                Description = description,
                DescriptionRaw = retJson["description"]?.ToString(),
                PlayerCount = players?["online"]?.GetValue<int>() ?? 0,
                MaxPlayers = players?["max"]?.GetValue<int>() ?? 0,
                Favicon = retJson["favicon"]?.ToString()
            };

            if (retJson["modinfo"] != null)
            {
                result = result with 
                { 
                    IsModded = true,
                    ModInfo = retJson["modinfo"]?.ToString()
                };
            }

            _logger.LogInformation("Ping successful to {Host}:{Port}, latency: {Latency}ms, players: {Online}/{Max}",
                _host, _endpoint.Port, latency, result.PlayerCount, result.MaxPlayers);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ping server {Endpoint}", _endpoint);
            return null;
        }
    }

    private byte[] BuildHandshakePacket(string serverIp, int serverPort, int protocolVersion)
    {
        var handshake = new List<byte>();
        
        handshake.AddRange(WriteVarInt(0));
        handshake.AddRange(WriteVarInt(protocolVersion));
        
        var binaryIp = Encoding.UTF8.GetBytes(serverIp);
        if (binaryIp.Length > 255) 
            throw new ArgumentException("Server address too long");
            
        handshake.AddRange(WriteVarInt(binaryIp.Length));
        handshake.AddRange(binaryIp);
        handshake.AddRange(BitConverter.GetBytes((ushort)serverPort).Reverse());
        handshake.AddRange(WriteVarInt(1));

        handshake.InsertRange(0, WriteVarInt(handshake.Count));
        return handshake.ToArray();
    }

    private static byte[] BuildStatusRequestPacket()
    {
        var statusRequest = new List<byte>();
        statusRequest.AddRange(WriteVarInt(1));
        statusRequest.AddRange(WriteVarInt(0));
        return statusRequest.ToArray();
    }

    private static byte[] BuildPingRequestPacket(long timestamp)
    {
        var pingRequest = new List<byte>();
        pingRequest.AddRange(WriteVarInt(9));
        pingRequest.AddRange(WriteVarInt(1));
        pingRequest.AddRange(BitConverter.GetBytes(timestamp).Reverse());
        return pingRequest.ToArray();
    }

    private async Task<(byte[]? StatusPayload, long Latency)> ReadStatusPayloadAsync(
        Stream stream, CancellationToken cancellationToken)
    {
        byte[]? statusPayload = null;
        long? latency = null;

        try
        {
            while (statusPayload == null || latency == null)
            {
                var packetLength = await ReadVarIntAsync(stream, cancellationToken);
                if (packetLength <= 0) 
                    throw new InvalidDataException("Server returned empty packet");

                var packetData = await ReadExactAsync(stream, packetLength, cancellationToken);
                using var packetStream = new MemoryStream(packetData, writable: false);
                var packetId = await ReadVarIntAsync(packetStream, cancellationToken);

                switch (packetId)
                {
                    case 0:
                        var jsonLength = await ReadVarIntAsync(packetStream, cancellationToken);
                        statusPayload = await ReadExactAsync(packetStream, jsonLength, cancellationToken);
                        break;

                    case 1:
                        var pongData = await ReadExactAsync(packetStream, 8, cancellationToken);
                        latency = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - ReadInt64BigEndian(pongData);
                        break;
                }
            }
        }
        catch (EndOfStreamException ex)
        {
            if (statusPayload != null && latency == null)
                throw new EndOfStreamException("Server disconnected after status response, no pong received", ex);
            throw;
        }

        return (statusPayload, latency!.Value);
    }

    private static long ReadInt64BigEndian(byte[] data)
    {
        if (data.Length != 8)
            throw new ArgumentException("Pong data must be 8 bytes");
        return BinaryPrimitives.ReadInt64BigEndian(data);
    }

    private static async Task<int> ReadVarIntAsync(Stream stream, CancellationToken cancellationToken)
    {
        int value = 0;
        int shift = 0;
        while (true)
        {
            var b = (byte)stream.ReadByte();
            if (b == 0xFF && stream.ReadByte() == -1) break;
            value |= (b & 0x7F) << shift;
            if ((b & 0x80) == 0) break;
            shift += 7;
        }
        return value;
    }

    private static async Task<byte[]> ReadExactAsync(Stream stream, int length, CancellationToken cancellationToken)
    {
        var buffer = new byte[length];
        await stream.ReadExactlyAsync(buffer, cancellationToken);
        return buffer;
    }

    private static string ConvertJNodeToMcString(JsonNode? jsonNode)
    {
        if (jsonNode == null) return string.Empty;
        
        var result = new StringBuilder();
        var stack = new Stack<JsonNode>();
        stack.Push(jsonNode);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            switch (current.GetValueKind())
            {
                case JsonValueKind.Object:
                {
                    var obj = current.AsObject();
                    if (obj.TryGetPropertyValue("extra", out var extraNode) && extraNode is JsonArray extraArray)
                    {
                        for (var i = extraArray.Count - 1; i >= 0; i--)
                            if (extraArray[i] != null)
                                stack.Push(extraArray[i]!);
                    }
                    if (obj.TryGetPropertyValue("text", out _))
                    {
                        var formatCode = GetTextStyleString(
                            obj["color"]?.ToString() ?? string.Empty,
                            Convert.ToBoolean(obj["bold"]?.ToString() ?? "false"),
                            Convert.ToBoolean(obj["strikethrough"]?.ToString() ?? "false"),
                            Convert.ToBoolean(obj["underline"]?.ToString() ?? "false"),
                            Convert.ToBoolean(obj["italic"]?.ToString() ?? "false")
                        );
                        result.Append($"{formatCode}{obj["text"] ?? string.Empty}");
                    }
                    break;
                }
                case JsonValueKind.String:
                    result.Append(current.ToString());
                    break;
                case JsonValueKind.Array:
                {
                    var jArr = current.AsArray();
                    for (var i = jArr.Count - 1; i >= 0; i--)
                        if (jArr[i] != null)
                            stack.Push(jArr[i]!);
                    break;
                }
            }
        }

        return result.ToString();
    }

    private static readonly Dictionary<string, string> ColorMap = new()
    {
        ["black"] = "0", ["dark_blue"] = "1", ["dark_green"] = "2", ["dark_aqua"] = "3",
        ["dark_red"] = "4", ["dark_purple"] = "5", ["gold"] = "6", ["gray"] = "7",
        ["dark_gray"] = "8", ["blue"] = "9", ["green"] = "a", ["aqua"] = "b",
        ["red"] = "c", ["light_purple"] = "d", ["yellow"] = "e", ["white"] = "f"
    };

    private static string GetTextStyleString(
        string color,
        bool bold = false,
        bool strikethrough = false,
        bool underline = false,
        bool italic = false)
    {
        var sb = new StringBuilder();
        if (ColorMap.TryGetValue(color, out var colorCode)) sb.Append($"§{colorCode}");
        if (bold) sb.Append("§l");
        if (italic) sb.Append("§o");
        if (underline) sb.Append("§n");
        if (strikethrough) sb.Append("§m");
        return sb.ToString();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
