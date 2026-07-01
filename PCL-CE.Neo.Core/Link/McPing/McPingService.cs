using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Link.McPing.Model;
using PCL_CE.Neo.Core.Utils;

namespace PCL_CE.Neo.Core.Link.McPing;

public class McPingService : IMcPingService
{
    private readonly IPEndPoint _endpoint;
    private readonly string _host;
    private const int DefaultTimeout = 10000;
    private readonly int _timeout;
    private bool _disposed;
    private const string ModuleName = "McPing";

    private readonly ILogger<McPingService> _logger;

    public IPEndPoint Endpoint => _endpoint;
    public string Host => _host;
    public int Timeout => _timeout;

    public McPingService(IPEndPoint endpoint, int timeout = DefaultTimeout)
        : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<McPingService>.Instance, endpoint, timeout)
    {
    }

    public McPingService(ILogger<McPingService> logger, IPEndPoint endpoint, int timeout = DefaultTimeout)
    {
        _logger = logger;
        _endpoint = endpoint;
        _host = _endpoint.Address.ToString();
        _timeout = timeout;
    }

    public McPingService(string ip, int port = 25565, int timeout = DefaultTimeout)
        : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<McPingService>.Instance, ip, port, timeout)
    {
    }

    public McPingService(ILogger<McPingService> logger, string ip, int port = 25565, int timeout = DefaultTimeout)
    {
        _logger = logger;
        _endpoint = IPAddress.TryParse(ip, out var ipAddress)
            ? new IPEndPoint(ipAddress, port)
            : new IPEndPoint(Dns.GetHostAddresses(ip).First(), port);
        _host = ip;
        _timeout = timeout;
    }

    public McPingService(string host, IPEndPoint endpoint, int timeout = DefaultTimeout)
        : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<McPingService>.Instance, host, endpoint, timeout)
    {
    }

    public McPingService(ILogger<McPingService> logger, string host, IPEndPoint endpoint, int timeout = DefaultTimeout)
    {
        _logger = logger;
        _endpoint = endpoint;
        _host = host;
        _timeout = timeout;
    }

    public async Task<McPingResult?> PingAsync(CancellationToken cancellationToken = default)
    {
        using var so = new Socket(SocketType.Stream, ProtocolType.Tcp);
        using var timeoutCts = new CancellationTokenSource(_timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            _logger.LogDebug("{ModuleName}: Connecting to {Endpoint}", ModuleName, _endpoint);
            await so.ConnectAsync(_endpoint.Address, _endpoint.Port, linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogError(new TimeoutException("连接超时"), "{ModuleName}: Failed to connect to the {Endpoint}", ModuleName, _endpoint);
            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{ModuleName}: Failed to connect to the {Endpoint}", ModuleName, _endpoint);
            return null;
        }

        _logger.LogDebug("{ModuleName}: Connection established: {Endpoint}", ModuleName, _endpoint);
        await using var stream = new NetworkStream(so, false);

        var handshakePacket = _BuildHandshakePacket(_host, _endpoint.Port);
        var statusPacket = _BuildStatusRequestPacket();

        byte[]? statusPayload;
        long latency = 0;
        try
        {
            await stream.WriteAsync(handshakePacket, linkedCts.Token);
            _logger.LogDebug("{ModuleName}: Handshake sent, packet length: {Length}", ModuleName, handshakePacket.Length);

            await stream.WriteAsync(statusPacket, linkedCts.Token);
            _logger.LogDebug("{ModuleName}: Status sent, packet length: {Length}", ModuleName, statusPacket.Length);

            var pingTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var pingPacket = _BuildPingRequestPacket(pingTimestamp);

            await stream.WriteAsync(pingPacket, linkedCts.Token);
            _logger.LogDebug("{ModuleName}: Ping sent, packet length: {Length}", ModuleName, pingPacket.Length);

            (statusPayload, latency) = await _ReadStatusPayloadAsync(stream, linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogError(new TimeoutException("数据读写超时"), "{ModuleName}: Operation timed out on {Endpoint}", ModuleName, _endpoint);
            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{ModuleName}: Failed to communicate with {Endpoint}: {Message}", ModuleName, _endpoint, e.Message);
            return null;
        }
        finally
        {
            if (so.Connected) so.Shutdown(SocketShutdown.Both);
        }

        so.Close();

        if (statusPayload is null || statusPayload.Length == 0)
        {
            _logger.LogError("{ModuleName}: No server info returned", ModuleName);
            return null;
        }

        var retCtx = Encoding.UTF8.GetString(statusPayload);
        _logger.LogDebug("{ModuleName}: Raw response: {Response}", ModuleName, retCtx.Length > 100 ? retCtx.Substring(0, 100) + "..." : retCtx);

        var retJson = JsonNode.Parse(retCtx);
        if (retJson is null)
        {
            _logger.LogError("{ModuleName}: Server returned invalid information", ModuleName);
            return null;
        }

#if DEBUG
        var resJsonDebug = retJson.DeepClone();
        if (resJsonDebug is JsonObject jsonObject && jsonObject.ContainsKey("favicon"))
        {
            jsonObject["favicon"] = "...";
        }
        _logger.LogDebug("{ModuleName}: {Json}", ModuleName, resJsonDebug.ToJsonString());
#endif

        if (retJson["description"] is JsonObject descObj)
        {
            retJson["description"] = _ConvertJNodeToMcString(descObj);
        }

        var response = JsonSerializer.Deserialize<McPingResult>(retJson);
        if (response?.Version == null)
        {
            _logger.LogError("{ModuleName}: Server returned invalid fields, missing: version", ModuleName);
            return null;
        }

        response = response with
        {
            Latency = latency
        };

        _logger.LogInformation("{ModuleName}: Ping successful - {Endpoint}, Latency: {Latency}ms", ModuleName, _endpoint, latency);
        return response;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private byte[] _BuildHandshakePacket(string serverIp, int serverPort)
    {
        List<byte> handshake = [];
        handshake.AddRange(VarIntHelper.Encode(0u));
        handshake.AddRange(VarIntHelper.Encode(772u));
        var binaryIp = Encoding.UTF8.GetBytes(serverIp);
        if (binaryIp.Length > 255) throw new ArgumentException("服务器地址过长", nameof(serverIp));
        handshake.AddRange(VarIntHelper.Encode((uint)binaryIp.Length));
        handshake.AddRange(binaryIp);
        handshake.AddRange(BitConverter.GetBytes((ushort)serverPort).AsEnumerable().Reverse());
        handshake.AddRange(VarIntHelper.Encode(1u));

        handshake.InsertRange(0, VarIntHelper.Encode((uint)handshake.Count));
        return handshake.ToArray();
    }

    private byte[] _BuildStatusRequestPacket()
    {
        List<byte> statusRequest = [];
        statusRequest.AddRange(VarIntHelper.Encode(1u));
        statusRequest.AddRange(VarIntHelper.Encode(0u));
        return statusRequest.ToArray();
    }

    private byte[] _BuildPingRequestPacket(long timestamp)
    {
        List<byte> pingRequest = [];
        pingRequest.AddRange(VarIntHelper.Encode(9u));
        pingRequest.AddRange(VarIntHelper.Encode(1u));
        pingRequest.AddRange(BitConverter.GetBytes(timestamp).AsEnumerable().Reverse());
        return pingRequest.ToArray();
    }

    private async Task<(byte[] StatusPayload, long Latency)> _ReadStatusPayloadAsync(Stream stream, CancellationToken cancellationToken)
    {
        byte[]? statusPayload = null;
        long? latency = null;

        try
        {
            while (statusPayload is null || latency is null)
            {
                var packetLength = checked((int)await VarIntHelper.ReadFromStreamAsync(stream, cancellationToken));
                _logger.LogDebug("{ModuleName}: Packet length: {Length}", ModuleName, packetLength);
                if (packetLength <= 0) throw new InvalidDataException("服务器返回了空数据包");

                var packetData = await _ReadExactAsync(stream, packetLength, cancellationToken);
                using var packetStream = new MemoryStream(packetData, writable: false);
                var packetId = checked((int)await VarIntHelper.ReadFromStreamAsync(packetStream, cancellationToken));
                _logger.LogDebug("{ModuleName}: Packet id: {Id}", ModuleName, packetId);

                switch (packetId)
                {
                    case 0:
                        var jsonLength = checked((int)await VarIntHelper.ReadFromStreamAsync(packetStream, cancellationToken));
                        statusPayload = await _ReadExactAsync(packetStream, jsonLength, cancellationToken);
                        if (packetStream.Position != packetStream.Length)
                            _logger.LogWarning("{ModuleName}: Status packet contains {ExtraBytes} trailing bytes.", ModuleName, packetStream.Length - packetStream.Position);
                        break;

                    case 1:
                        var pongData = await _ReadExactAsync(packetStream, 8, cancellationToken);
                        if (packetStream.Position != packetStream.Length)
                            _logger.LogWarning("{ModuleName}: Pong packet contains {ExtraBytes} trailing bytes.", ModuleName, packetStream.Length - packetStream.Position);
                        latency = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _ReadInt64BigEndian(pongData);
                        break;

                    default:
                        _logger.LogWarning("{ModuleName}: Ignore unexpected packet type: {Id}", ModuleName, packetId);
                        break;
                }
            }
        }
        catch (EndOfStreamException ex)
        {
            if (statusPayload is not null && latency is null)
                throw new EndOfStreamException("服务器在返回状态后提前断开连接，未返回 pong 数据包，无法计算延迟。", ex);

            if (statusPayload is null)
                throw new EndOfStreamException("服务器在返回完整状态数据包前提前断开连接。", ex);

            throw;
        }

        return (statusPayload, latency.Value);
    }

    private static long _ReadInt64BigEndian(byte[] data)
    {
        if (data.Length != 8)
            throw new ArgumentException("Pong 数据长度必须为 8 字节", nameof(data));

        return BinaryPrimitives.ReadInt64BigEndian(data);
    }

    private static async Task<byte[]> _ReadExactAsync(Stream stream, int length, CancellationToken cancellationToken)
    {
        var buffer = new byte[length];
        await stream.ReadExactlyAsync(buffer, cancellationToken);
        return buffer;
    }

    private static string _ConvertJNodeToMcString(JsonNode? jsonNode)
    {
        if (jsonNode == null) return string.Empty;
        StringBuilder result = new();
        Stack<JsonNode> stack = new();
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
                            for (var i = extraArray.Count - 1; i >= 0; i--)
                                if (extraArray[i] != null)
                                    stack.Push(extraArray[i]!);
                        if (obj.TryGetPropertyValue("text", out _))
                        {
                            var formatCode = _GetTextStyleString(
                                obj["color"]?.ToString() ?? string.Empty,
                                Convert.ToBoolean(obj["bold"]?.ToString() ?? "false"),
                                Convert.ToBoolean(obj["obfuscated"]?.ToString() ?? "false"),
                                Convert.ToBoolean(obj["strikethrough"]?.ToString() ?? "false"),
                                Convert.ToBoolean(obj["underline"]?.ToString() ?? "false"),
                                Convert.ToBoolean(obj["italic"]?.ToString() ?? "false")
                            );
                            result.Append($"{formatCode}{obj["text"] ?? string.Empty}");
                        }
                        break;
                    }
                case JsonValueKind.String:
                    {
                        result.Append(current);
                        break;
                    }
                case JsonValueKind.Array:
                    {
                        var jArr = current.AsArray();
                        for (var i = jArr.Count - 1; i >= 0; i--)
                            if (jArr[i] != null)
                                stack.Push(jArr[i]!);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        return result.ToString();
    }

    private static readonly Dictionary<string, string> _ColorMap = new()
    {
        ["black"] = "0",
        ["dark_blue"] = "1",
        ["dark_green"] = "2",
        ["dark_aqua"] = "3",
        ["dark_red"] = "4",
        ["dark_purple"] = "5",
        ["gold"] = "6",
        ["gray"] = "7",
        ["dark_gray"] = "8",
        ["blue"] = "9",
        ["green"] = "a",
        ["aqua"] = "b",
        ["red"] = "c",
        ["light_purple"] = "d",
        ["yellow"] = "e",
        ["white"] = "f"
    };

    private static string _GetTextStyleString(
        string color,
        bool bold = false,
        bool obfuscated = false,
        bool strikethrough = false,
        bool underline = false,
        bool italic = false)
    {
        var sb = new StringBuilder();
        if (_ColorMap.TryGetValue(color, out var colorCode)) sb.Append($"§{colorCode}");
        if (bold) sb.Append("§l");
        if (italic) sb.Append("§o");
        if (underline) sb.Append("§n");
        if (strikethrough) sb.Append("§m");
        if (color.StartsWith('#')) sb.Append(color);
        return sb.ToString();
    }
}