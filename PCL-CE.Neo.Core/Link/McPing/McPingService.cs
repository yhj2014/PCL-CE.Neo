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

namespace PCL_CE.Neo.Core.Link.McPing;

/// <summary>
/// 现代 Minecraft 协议服务器探测服务实现
/// 支持 1.7+ 版本的服务器信息查询协议
/// </summary>
public class McPingService : IMcPingService
{
    private readonly ILogger<McPingService>? _logger;
    private readonly IPEndPoint _endpoint;
    private readonly string _host;
    private const int DefaultTimeout = 10000;
    private readonly int _timeout;
    private bool _disposed;

    public IPEndPoint Endpoint => _endpoint;
    public string Host => _host;
    public int Timeout => _timeout;

    public McPingService(IPEndPoint endpoint, int timeout = DefaultTimeout, ILogger<McPingService>? logger = null)
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        _host = _endpoint.Address.ToString();
        _timeout = timeout;
        _logger = logger;
    }

    public McPingService(string ip, int port = 25565, int timeout = DefaultTimeout, ILogger<McPingService>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(ip))
        {
            throw new ArgumentNullException(nameof(ip));
        }

        _logger = logger;

        try
        {
            if (IPAddress.TryParse(ip, out var ipAddress))
            {
                _endpoint = new IPEndPoint(ipAddress, port);
            }
            else
            {
                var addresses = Dns.GetHostAddresses(ip);
                var address = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
                              ?? addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetworkV6)
                              ?? addresses.First();
                _endpoint = new IPEndPoint(address, port);
            }
            _host = ip;
            _timeout = timeout;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "解析服务器地址失败: {Address}", ip);
            throw;
        }
    }

    public McPingService(string host, IPEndPoint endpoint, int timeout = DefaultTimeout, ILogger<McPingService>? logger = null)
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _timeout = timeout;
        _logger = logger;
    }

    /// <summary>
    /// 执行现代 Minecraft 协议的服务器探测
    /// </summary>
    public async Task<McPingResult?> PingAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(McPingService));
        }

        using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        using var timeoutCts = new CancellationTokenSource(_timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            _logger?.LogDebug("正在连接到服务器: {Endpoint}", _endpoint);
            await socket.ConnectAsync(_endpoint.Address, _endpoint.Port, linkedCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger?.LogWarning("连接服务器超时: {Endpoint}", _endpoint);
            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "连接服务器失败: {Endpoint}", _endpoint);
            return null;
        }

        _logger?.LogDebug("已连接到服务器: {Endpoint}", _endpoint);

        await using var stream = new NetworkStream(socket, false);

        var handshakePacket = BuildHandshakePacket(_host, _endpoint.Port);
        var statusPacket = BuildStatusRequestPacket();

        byte[]? statusPayload = null;
        long latency = 0;

        try
        {
            await stream.WriteAsync(handshakePacket, linkedCts.Token).ConfigureAwait(false);
            _logger?.LogDebug("已发送握手包，长度: {Length}", handshakePacket.Length);

            await stream.WriteAsync(statusPacket, linkedCts.Token).ConfigureAwait(false);
            _logger?.LogDebug("已发送状态请求包，长度: {Length}", statusPacket.Length);

            var pingTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var pingPacket = BuildPingRequestPacket(pingTimestamp);

            await stream.WriteAsync(pingPacket, linkedCts.Token).ConfigureAwait(false);
            _logger?.LogDebug("已发送 Ping 包，长度: {Length}", pingPacket.Length);

            (statusPayload, latency) = await ReadStatusPayloadAsync(stream, linkedCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger?.LogWarning("服务器通信超时: {Endpoint}", _endpoint);
            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "服务器通信失败: {Endpoint}", _endpoint);
            return null;
        }
        finally
        {
            if (socket.Connected)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                    // 忽略关闭异常
                }
            }
        }

        socket.Close();

        if (statusPayload == null || statusPayload.Length == 0)
        {
            _logger?.LogWarning("服务器返回空响应: {Endpoint}", _endpoint);
            return null;
        }

        var responseText = Encoding.UTF8.GetString(statusPayload);

        try
        {
            var jsonNode = JsonNode.Parse(responseText);
            if (jsonNode == null)
            {
                _logger?.LogWarning("服务器返回无效 JSON: {Endpoint}", _endpoint);
                return null;
            }

            // 处理 Description 字段，将其转换为字符串形式
            if (jsonNode["description"] is JsonObject descObj)
            {
                jsonNode["description"] = ConvertJsonNodeToMcString(descObj);
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var response = JsonSerializer.Deserialize<McPingResult>(jsonNode, options);
            if (response?.Version == null)
            {
                _logger?.LogWarning("服务器返回的版本信息无效: {Endpoint}", _endpoint);
                return null;
            }

            response = response with { Latency = latency };

            _logger?.LogInformation("服务器探测成功: {Host}, 版本: {Version}, 玩家: {Online}/{Max}, 延迟: {Latency}ms",
                _host, response.Version.Name, response.Players.Online, response.Players.Max, latency);

            return response;
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "解析服务器响应失败: {Endpoint}", _endpoint);
            return null;
        }
    }

    /// <summary>
    /// 构建握手包
    /// </summary>
    private static byte[] BuildHandshakePacket(string serverIp, int serverPort)
    {
        var handshake = new List<byte>();
        handshake.AddRange(EncodeVarInt(0)); // 状态头，表明这是一个握手包
        handshake.AddRange(EncodeVarInt(772)); // 协议版本，表明请求客户端的版本
        var binaryIp = Encoding.UTF8.GetBytes(serverIp);
        if (binaryIp.Length > 255)
        {
            throw new ArgumentException("服务器地址过长", nameof(serverIp));
        }
        handshake.AddRange(EncodeVarInt((uint)binaryIp.Length)); // 服务器地址长度
        handshake.AddRange(binaryIp); // 服务器地址
        handshake.AddRange(BitConverter.GetBytes((ushort)serverPort).Reverse()); // 服务器端口（大端序）
        handshake.AddRange(EncodeVarInt(1)); // 1 表明当前状态为 ping

        handshake.InsertRange(0, EncodeVarInt((uint)handshake.Count)); // 包长度
        return handshake.ToArray();
    }

    /// <summary>
    /// 构建状态请求包
    /// </summary>
    private static byte[] BuildStatusRequestPacket()
    {
        var statusRequest = new List<byte>();
        statusRequest.AddRange(EncodeVarInt(1)); // 包长度
        statusRequest.AddRange(EncodeVarInt(0)); // 包 ID
        return statusRequest.ToArray();
    }

    /// <summary>
    /// 构建 Ping 请求包
    /// </summary>
    private static byte[] BuildPingRequestPacket(long timestamp)
    {
        var pingRequest = new List<byte>();
        pingRequest.AddRange(EncodeVarInt(9)); // 包长度（1 + 8）
        pingRequest.AddRange(EncodeVarInt(1)); // 包 ID
        pingRequest.AddRange(BitConverter.GetBytes(timestamp).Reverse()); // 时间戳（大端序）
        return pingRequest.ToArray();
    }

    /// <summary>
    /// 读取服务器响应
    /// </summary>
    private async Task<(byte[] StatusPayload, long Latency)> ReadStatusPayloadAsync(Stream stream, CancellationToken cancellationToken)
    {
        byte[]? statusPayload = null;
        long? latency = null;

        try
        {
            while (statusPayload == null || latency == null)
            {
                var packetLength = checked((int)await ReadVarIntFromStreamAsync(stream, cancellationToken).ConfigureAwait(false));
                _logger?.LogDebug("收到包长度: {Length}", packetLength);

                if (packetLength <= 0)
                {
                    throw new InvalidDataException("收到空包");
                }

                var packetData = await ReadExactAsync(stream, packetLength, cancellationToken).ConfigureAwait(false);
                using var packetStream = new MemoryStream(packetData, writable: false);
                var packetId = checked((int)await ReadVarIntFromStreamAsync(packetStream, cancellationToken).ConfigureAwait(false));
                _logger?.LogDebug("收到包 ID: {Id}", packetId);

                switch (packetId)
                {
                    case 0:
                        var jsonLength = checked((int)await ReadVarIntFromStreamAsync(packetStream, cancellationToken).ConfigureAwait(false));
                        statusPayload = await ReadExactAsync(packetStream, jsonLength, cancellationToken).ConfigureAwait(false);
                        break;

                    case 1:
                        var pongData = await ReadExactAsync(packetStream, 8, cancellationToken).ConfigureAwait(false);
                        latency = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - BinaryPrimitives.ReadInt64BigEndian(pongData);
                        break;

                    default:
                        _logger?.LogWarning("收到未知包类型: {Id}", packetId);
                        break;
                }
            }
        }
        catch (EndOfStreamException ex)
        {
            _logger?.LogError(ex, "服务器连接中断");
            throw;
        }

        return (statusPayload!, latency!.Value);
    }

    /// <summary>
    /// 精确读取指定长度的数据
    /// </summary>
    private static async Task<byte[]> ReadExactAsync(Stream stream, int length, CancellationToken cancellationToken)
    {
        var buffer = new byte[length];
        await stream.ReadExactlyAsync(buffer, cancellationToken).ConfigureAwait(false);
        return buffer;
    }

    /// <summary>
    /// 从流中读取 VarInt
    /// </summary>
    private static async Task<int> ReadVarIntFromStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        int value = 0;
        int shift = 0;
        var buffer = new byte[1];
        while (true)
        {
            var read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                throw new EndOfStreamException();
            }
            var b = buffer[0];
            value |= (b & 0x7F) << shift;
            if ((b & 0x80) == 0) break;
            shift += 7;
            if (shift >= 35)
            {
                throw new InvalidDataException("VarInt 过长");
            }
        }
        return value;
    }

    /// <summary>
    /// 编码 VarInt
    /// </summary>
    private static byte[] EncodeVarInt(uint value)
    {
        var result = new List<byte>();
        while (true)
        {
            if ((value & ~0x7FU) == 0)
            {
                result.Add((byte)value);
                return result.ToArray();
            }
            result.Add((byte)((value & 0x7F) | 0x80));
            value >>= 7;
        }
    }

    /// <summary>
    /// 将 JSON 节点转换为 Minecraft 格式字符串
    /// </summary>
    private static string ConvertJsonNodeToMcString(JsonNode? jsonNode)
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
                        // 检查并处理 extra 数组
                        if (obj.TryGetPropertyValue("extra", out var extraNode) && extraNode is JsonArray extraArray)
                        {
                            // 逆序压栈保证原始顺序
                            for (var i = extraArray.Count - 1; i >= 0; i--)
                            {
                                if (extraArray[i] != null)
                                {
                                    stack.Push(extraArray[i]!);
                                }
                            }
                        }
                        // 检查并处理 text 属性
                        if (obj.TryGetPropertyValue("text", out var textNode))
                        {
                            var formatCode = GetTextStyleString(
                                obj["color"]?.ToString() ?? string.Empty,
                                obj["bold"]?.GetValue<bool>() ?? false,
                                obj["obfuscated"]?.GetValue<bool>() ?? false,
                                obj["strikethrough"]?.GetValue<bool>() ?? false,
                                obj["underline"]?.GetValue<bool>() ?? false,
                                obj["italic"]?.GetValue<bool>() ?? false
                            );
                            result.Append($"{formatCode}{textNode}");
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
                        var array = current.AsArray();
                        // 逆序压栈保证原始顺序
                        for (var i = array.Count - 1; i >= 0; i--)
                        {
                            if (array[i] != null)
                            {
                                stack.Push(array[i]!);
                            }
                        }
                        break;
                    }

                default:
                    {
                        // 忽略其他类型
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
        if (ColorMap.TryGetValue(color, out var colorCode))
        {
            sb.Append($"§{colorCode}");
        }
        if (bold) sb.Append("§l");
        if (italic) sb.Append("§o");
        if (underline) sb.Append("§n");
        if (strikethrough) sb.Append("§m");
        if (color.StartsWith('#'))
        {
            sb.Append(color);
        }
        return sb.ToString();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}