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
using PCL_CE.Neo.Core.Logging;
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

    public IPEndPoint Endpoint => _endpoint;
    public string Host => _host;
    public int Timeout => _timeout;

    public McPingService(IPEndPoint endpoint, int timeout = DefaultTimeout)
    {
        _endpoint = endpoint;
        _host = _endpoint.Address.ToString();
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
        using var so = new Socket(SocketType.Stream, ProtocolType.Tcp);
        using var timeoutCts = new CancellationTokenSource(_timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            LogWrapper.Debug(ModuleName, $"Connecting to {_endpoint}");
            await so.ConnectAsync(_endpoint.Address, _endpoint.Port, linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            LogWrapper.Error(new TimeoutException("Connection timeout"), ModuleName, $"Failed to connect to {_endpoint}");
            return null;
        }
        catch (Exception e)
        {
            LogWrapper.Error(e, ModuleName, $"Failed to connect to {_endpoint}");
            return null;
        }

        LogWrapper.Debug(ModuleName, $"Connection established: {_endpoint}");
        await using var stream = new NetworkStream(so, false);

        var handshakePacket = _BuildHandshakePacket(_host, _endpoint.Port);
        var statusPacket = _BuildStatusRequestPacket();

        byte[]? statusPayload;
        long latency = 0;
        try
        {
            await stream.WriteAsync(handshakePacket, linkedCts.Token);
            LogWrapper.Debug(ModuleName, $"Handshake sent, packet length: {handshakePacket.Length}");

            await stream.WriteAsync(statusPacket, linkedCts.Token);
            LogWrapper.Debug(ModuleName, $"Status sent, packet length: {statusPacket.Length}");

            var pingTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var pingPacket = _BuildPingRequestPacket(pingTimestamp);

            await stream.WriteAsync(pingPacket, linkedCts.Token);
            LogWrapper.Debug(ModuleName, $"Ping sent, packet length: {pingPacket.Length}");

            (statusPayload, latency) = await _ReadStatusPayloadAsync(stream, linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            LogWrapper.Error(new TimeoutException("Data read/write timeout"), "McPing", $"Operation timed out on {_endpoint}");
            return null;
        }
        catch (Exception e)
        {
            LogWrapper.Error(e, ModuleName, $"Failed to communicate with {_endpoint}: {e.Message}");
            return null;
        }
        finally
        {
            if (so.Connected) so.Shutdown(SocketShutdown.Both);
        }

        so.Close();

        if (statusPayload is null || statusPayload.Length == 0)
        {
            LogWrapper.Error(new InvalidDataException("Server returned no status information"), ModuleName, "Empty response from server");
            return null;
        }

        var retCtx = Encoding.UTF8.GetString(statusPayload);

        var retJson = JsonNode.Parse(retCtx);
        if (retJson == null)
        {
            LogWrapper.Error(new NullReferenceException("Server returned invalid data"), ModuleName, "Failed to parse JSON response");
            return null;
        }

#if DEBUG
        var resJsonDebug = retJson.DeepClone();
        if (resJsonDebug is JsonObject jsonObject && jsonObject.ContainsKey("favicon"))
        {
            jsonObject["favicon"] = "...";
        }

        LogWrapper.Debug(ModuleName, resJsonDebug.ToJsonString());
#endif

        if (retJson["description"] is JsonObject descObj)
        {
            retJson["description"] = _ConvertJNodeToMcString(descObj);
        }

        var response = JsonSerializer.Deserialize<McPingResult>(retJson);
        if (response?.Version == null)
        {
            LogWrapper.Error(new NullReferenceException("Server returned missing version field"), ModuleName, "Incomplete server response");
            return null;
        }

        response = response with
        {
            Latency = latency
        };

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
        handshake.AddRange(VarIntHelper.Encode(0));
        handshake.AddRange(VarIntHelper.Encode(772));
        var binaryIp = Encoding.UTF8.GetBytes(serverIp);
        if (binaryIp.Length > 255) throw new ArgumentException("Server address too long", nameof(serverIp));
        handshake.AddRange(VarIntHelper.Encode((uint)binaryIp.Length));
        handshake.AddRange(binaryIp);
        handshake.AddRange(BitConverter.GetBytes((ushort)serverPort).AsEnumerable().Reverse());
        handshake.AddRange(VarIntHelper.Encode(1));

        handshake.InsertRange(0, VarIntHelper.Encode((uint)handshake.Count));
        return handshake.ToArray();
    }

    private byte[] _BuildStatusRequestPacket()
    {
        List<byte> statusRequest = [];
        statusRequest.AddRange(VarIntHelper.Encode(1));
        statusRequest.AddRange(VarIntHelper.Encode(0));
        return statusRequest.ToArray();
    }

    private byte[] _BuildPingRequestPacket(long timestamp)
    {
        List<byte> pingRequest = [];
        pingRequest.AddRange(VarIntHelper.Encode(9));
        pingRequest.AddRange(VarIntHelper.Encode(1));
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
                LogWrapper.Debug(ModuleName, $"Packet length: {packetLength}");
                if (packetLength <= 0) throw new InvalidDataException("Server returned empty packet");

                var packetData = await _ReadExactAsync(stream, packetLength, cancellationToken);
                using var packetStream = new MemoryStream(packetData, writable: false);
                var packetId = checked((int)await VarIntHelper.ReadFromStreamAsync(packetStream, cancellationToken));
                LogWrapper.Debug(ModuleName, $"Packet id: {packetId}");

                switch (packetId)
                {
                    case 0:
                        var jsonLength = checked((int)await VarIntHelper.ReadFromStreamAsync(packetStream, cancellationToken));
                        statusPayload = await _ReadExactAsync(packetStream, jsonLength, cancellationToken);
                        if (packetStream.Position != packetStream.Length)
                            LogWrapper.Warn(ModuleName, $"Status packet contains {packetStream.Length - packetStream.Position} trailing bytes.");
                        break;

                    case 1:
                        var pongData = await _ReadExactAsync(packetStream, 8, cancellationToken);
                        if (packetStream.Position != packetStream.Length)
                            LogWrapper.Warn(ModuleName, $"Pong packet contains {packetStream.Length - packetStream.Position} trailing bytes.");
                        latency = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _ReadInt64BigEndian(pongData);
                        break;

                    default:
                        LogWrapper.Warn(ModuleName, $"Ignoring unexpected packet type: {packetId}");
                        break;
                }
            }
        }
        catch (EndOfStreamException ex)
        {
            if (statusPayload is not null && latency is null)
                throw new EndOfStreamException("Server disconnected after returning status, no pong packet received for latency calculation.", ex);

            if (statusPayload is null)
                throw new EndOfStreamException("Server disconnected before returning complete status packet.", ex);

            throw;
        }

        return (statusPayload, latency.Value);
    }

    private static long _ReadInt64BigEndian(byte[] data)
    {
        if (data.Length != 8)
            throw new ArgumentException("Pong data must be 8 bytes", nameof(data));

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
                        LogWrapper.Warn(ModuleName, $"Encountered unhandled Motd content type ({current.GetValueKind()}): {current}");
                        break;
                    }
            }
        }

        LogWrapper.Debug(ModuleName, $"Motd processing complete, result: {result}");
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