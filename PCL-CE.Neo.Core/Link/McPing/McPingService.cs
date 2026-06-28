using System;
using System.Buffers.Binary;
using System.Collections.Generic;
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
using PCL_CE.Neo.Core.Utils;

namespace PCL_CE.Neo.Core.Link.McPing;

public class McPingService : IMcPingService
{
    private readonly IPEndPoint _endpoint;
    private readonly string _host;
    private readonly int _timeout;
    private bool _disposed;
    private readonly ILogger<McPingService> _logger;
    private const string ModuleName = "McPing";
    private const int DefaultTimeout = 10000;

    public IPEndPoint Endpoint => _endpoint;
    public string Host => _host;
    public int Timeout => _timeout;

    public McPingService(IPEndPoint endpoint, int timeout = DefaultTimeout) : this(
        endpoint,
        endpoint.Address.ToString(),
        timeout,
        Microsoft.Extensions.Logging.Abstractions.NullLogger<McPingService>.Instance)
    {
    }

    public McPingService(IPEndPoint endpoint, int timeout, ILogger<McPingService> logger) : this(
        endpoint,
        endpoint.Address.ToString(),
        timeout,
        logger)
    {
    }

    public McPingService(string ip, int port = 25565, int timeout = DefaultTimeout) : this(
        new IPEndPoint(Dns.GetHostAddresses(ip).First(), port),
        ip,
        timeout,
        Microsoft.Extensions.Logging.Abstractions.NullLogger<McPingService>.Instance)
    {
    }

    public McPingService(string ip, int port, int timeout, ILogger<McPingService> logger)
    {
        _endpoint = Dns.GetHostAddresses(ip).First() is { } ipAddress
            ? new IPEndPoint(ipAddress, port)
            : throw new ArgumentException($"Unable to resolve host: {ip}", nameof(ip));
        _host = ip;
        _timeout = timeout;
        _logger = logger;
    }

    private McPingService(IPEndPoint endpoint, string host, int timeout, ILogger<McPingService> logger)
    {
        _endpoint = endpoint;
        _host = host;
        _timeout = timeout;
        _logger = logger;
    }

    public async Task<McPingResult?> PingAsync(CancellationToken cancellationToken = default)
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
        await using var stream = new NetworkStream(so, false);

        var handshakePacket = BuildHandshakePacket(_host, _endpoint.Port);
        var statusPacket = BuildStatusRequestPacket();

        byte[]? statusPayload;
        long latency = 0;
        try
        {
            await stream.WriteAsync(handshakePacket, linkedCts.Token);
            _logger.LogDebug("Handshake sent, packet length: {Length}", handshakePacket.Length);

            await stream.WriteAsync(statusPacket, linkedCts.Token);
            _logger.LogDebug("Status sent, packet length: {Length}", statusPacket.Length);

            var pingTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var pingPacket = BuildPingRequestPacket(pingTimestamp);
            
            await stream.WriteAsync(pingPacket, linkedCts.Token);
            _logger.LogDebug("Ping sent, packet length: {Length}", pingPacket.Length);

            (statusPayload, latency) = await ReadStatusPayloadAsync(stream, linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Data read/write timeout on {Endpoint}", _endpoint);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to communicate with {Endpoint}", _endpoint);
            return null;
        }
        finally
        {
            if (so.Connected) so.Shutdown(SocketShutdown.Both);
        }

        so.Close();

        if (statusPayload is null || statusPayload.Length == 0)
            throw new InvalidDataException("Server returned no data");
            
        var retCtx = Encoding.UTF8.GetString(statusPayload);
        var retJson = JsonNode.Parse(retCtx);
        
        if (retJson == null)
            throw new NullReferenceException("Server returned invalid data");

        if (retJson["description"] is JsonObject descObj)
        {
            retJson["description"] = ConvertJNodeToMcString(descObj);
        }

        var response = JsonSerializer.Deserialize<McPingResult>(retJson);
        if (response?.Version == null)
            throw new NullReferenceException("Server response missing version field");

        response = response with { Latency = latency };
        return response;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private byte[] BuildHandshakePacket(string serverIp, int serverPort)
    {
        var handshake = new List<byte>();
        handshake.AddRange(VarIntHelper.Encode(0));
        handshake.AddRange(VarIntHelper.Encode(772));
        var binaryIp = Encoding.UTF8.GetBytes(serverIp);
        if (binaryIp.Length > 255) throw new Exception("Server address too long");
        handshake.AddRange(VarIntHelper.Encode((uint)binaryIp.Length));
        handshake.AddRange(binaryIp);
        handshake.AddRange(BitConverter.GetBytes((ushort)serverPort).Reverse());
        handshake.AddRange(VarIntHelper.Encode(1));

        handshake.InsertRange(0, VarIntHelper.Encode((uint)handshake.Count));
        return handshake.ToArray();
    }

    private static byte[] BuildStatusRequestPacket()
    {
        var statusRequest = new List<byte>();
        statusRequest.AddRange(VarIntHelper.Encode(1));
        statusRequest.AddRange(VarIntHelper.Encode(0));
        return statusRequest.ToArray();
    }

    private static byte[] BuildPingRequestPacket(long timestamp)
    {
        var pingRequest = new List<byte>();
        pingRequest.AddRange(VarIntHelper.Encode(9));
        pingRequest.AddRange(VarIntHelper.Encode(1));
        pingRequest.AddRange(BitConverter.GetBytes(timestamp).Reverse());
        return pingRequest.ToArray();
    }

    private async Task<(byte[] StatusPayload, long Latency)> ReadStatusPayloadAsync(Stream stream, CancellationToken cancellationToken)
    {
        byte[]? statusPayload = null;
        long? latency = null;

        try
        {
            while (statusPayload is null || latency is null)
            {
                var packetLength = checked((int)await VarIntHelper.ReadFromStreamAsync(stream, cancellationToken));
                _logger.LogDebug("Packet length: {PacketLength}", packetLength);
                if (packetLength <= 0) throw new InvalidDataException("Server returned empty packet");

                var packetData = await ReadExactAsync(stream, packetLength, cancellationToken);
                using var packetStream = new MemoryStream(packetData, writable: false);
                var packetId = checked((int)await VarIntHelper.ReadFromStreamAsync(packetStream, cancellationToken));
                _logger.LogDebug("Packet id: {PacketId}", packetId);

                switch (packetId)
                {
                    case 0:
                        var jsonLength = checked((int)await VarIntHelper.ReadFromStreamAsync(packetStream, cancellationToken));
                        statusPayload = await ReadExactAsync(packetStream, jsonLength, cancellationToken);
                        if (packetStream.Position != packetStream.Length)
                            _logger.LogWarning("Status packet contains {ByteCount} trailing bytes", packetStream.Length - packetStream.Position);
                        break;

                    case 1:
                        var pongData = await ReadExactAsync(packetStream, 8, cancellationToken);
                        if (packetStream.Position != packetStream.Length)
                            _logger.LogWarning("Pong packet contains {ByteCount} trailing bytes", packetStream.Length - packetStream.Position);
                        latency = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - ReadInt64BigEndian(pongData);
                        break;

                    default:
                        _logger.LogWarning("Ignore unexpected packet type: {PacketId}", packetId);
                        break;
                }
            }
        }
        catch (EndOfStreamException ex)
        {
            if (statusPayload is not null && latency is null)
                throw new EndOfStreamException("Server disconnected after status response, no pong data to calculate latency", ex);

            if (statusPayload is null)
                throw new EndOfStreamException("Server disconnected before sending complete status packet", ex);

            throw;
        }

        return (statusPayload, latency.Value);
    }

    private static long ReadInt64BigEndian(byte[] data)
    {
        if (data.Length != 8)
            throw new ArgumentException("Pong data must be 8 bytes", nameof(data));

        return BinaryPrimitives.ReadInt64BigEndian(data);
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
                            for (var i = extraArray.Count - 1; i >= 0; i--)
                                if (extraArray[i] != null)
                                    stack.Push(extraArray[i]!);
                                    
                        if (obj.TryGetPropertyValue("text", out _))
                        {
                            var formatCode = GetTextStyleString(
                                obj["color"]?.ToString() ?? string.Empty,
                                Convert.ToBoolean(obj["bold"]?.ToString() ?? "false"),
                                Convert.ToBoolean(obj["obfuscated"]?.ToString() ?? "false"),
                                Convert.ToBoolean(obj["strikethrough"]?.ToString() ?? "false"),
                                Convert.ToBoolean(obj["underline"]?.ToString() ?? "false"),
                                Convert.ToBoolean(obj["italic"]?.ToString() ?? "false"));
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

    private static readonly Dictionary<string, string> ColorMap = new()
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

    private static string GetTextStyleString(
        string color,
        bool bold = false,
        bool obfuscated = false,
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
        if (color.StartsWith('#')) sb.Append(color);
        return sb.ToString();
    }
}
