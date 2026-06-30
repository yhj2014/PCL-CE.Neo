using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Models;
using PCL_CE.Neo.Core.Link.Scaffolding.Server.Abstractions;
using PCL_CE.Neo.Core.Link.Scaffolding.Server.Handlers;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Server;

public sealed class ScaffoldingServer : IAsyncDisposable
{
    private readonly TcpListener _listener;
    private readonly IServerContext _context;
    private readonly Dictionary<string, IRequestHandler> _handlers;
    private readonly CancellationTokenSource _cts = new();
    private Task? _listenTask;
    private Task? _cleanupTask;
    private readonly ILogger<ScaffoldingServer> _logger;

    private static readonly TimeSpan _PlayerTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan _CleanupInterval = TimeSpan.FromSeconds(5);

    public event Action<IReadOnlyList<PlayerProfile>>? ServerStarted;
    public event Action? ServerStopped;
    public event Action<Exception?>? ServerException;
    public event Action<IReadOnlyList<PlayerProfile>>? PlayerProfilePing;

    private void _OnContextPlayersPing(IReadOnlyList<PlayerProfile> players)
    {
        PlayerProfilePing?.Invoke(players);
    }

    public ScaffoldingServer(int port, IServerContext context, ILogger<ScaffoldingServer> logger)
    {
        _listener = new TcpListener(IPAddress.Loopback, port);
        _context = context;
        _logger = logger;

        _context.PlayerProfilesPing += _OnContextPlayersPing;

        _handlers = new()
        {
            ["c:player_ping"] = new PlayerPingHandler(_logger),
            ["c:server_port"] = new GetServerPortHandler(_logger),
            ["c:player_profiles_list"] = new GetPlayerProfileListHandler(_logger),
            ["c:protocols"] = new GetProtocolsHandler(_logger),
            ["c:ping"] = new PingHandler(_logger)
        };
    }

    public void Start()
    {
        try
        {
            _listener.Start();
            _logger.LogInformation("ScaffoldingServer 成功绑定到 {LocalEndpoint}，开始接受客户端连接", _listener.LocalEndpoint);
        }
        catch (SocketException ex)
        {
            _logger.LogError(ex, "ScaffoldingServer 启动失败，端口 {Port} 可能已被占用或被阻止", ((IPEndPoint)_listener.LocalEndpoint).Port);
            ServerException?.Invoke(ex);
            return;
        }

        _listenTask = _ListenForClientsAsync(_cts.Token);
        _cleanupTask = _MonitorPlayerLivenessAsync(_cts.Token);

        _listenTask.ContinueWith(t =>
        {
            _logger.LogError(t.Exception, "ScaffoldingServer 主监听任务意外失败，服务器不再接受新连接");
            ServerException?.Invoke(t.Exception);
        }, TaskContinuationOptions.OnlyOnFaulted);

        _cleanupTask.ContinueWith(t =>
        {
            _logger.LogError(t.Exception, "ScaffoldingServer 玩家清理任务意外失败");
        }, TaskContinuationOptions.OnlyOnFaulted);

        _logger.LogDebug("ScaffoldingServer 后台任务已成功调度");

        ServerStarted?.Invoke(_context.PlayerProfiles);
    }

    private async Task _MonitorPlayerLivenessAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_CleanupInterval, ct).ConfigureAwait(false);

                var now = DateTime.UtcNow;
                var timedOutPlayerKeys = new List<string>();

                foreach (var (machineId, trackedPlayer) in _context.TrackedPlayers)
                {
                    if (trackedPlayer.Profile.Kind is PlayerKind.HOST)
                    {
                        continue;
                    }

                    if (now - trackedPlayer.LastSeenUtc > _PlayerTimeout)
                    {
                        timedOutPlayerKeys.Add(machineId);
                    }
                }

                if (timedOutPlayerKeys.Count > 0)
                {
                    var listChanged = false;
                    foreach (var key in timedOutPlayerKeys)
                    {
                        if (_context.TrackedPlayers.TryRemove(key, out var removedPlayer))
                        {
                            listChanged = true;
                            _logger.LogInformation("玩家 '{PlayerName}' 超时并被移除", removedPlayer.Profile.PlayerName);
                        }
                    }

                    if (listChanged)
                    {
                        _context.OnPlayerProfilesChanged();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ScaffoldingServer 玩家清理任务发生错误");
            }
        }
    }

    private async Task _ListenForClientsAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var tcpClient = await _listener.AcceptTcpClientAsync(ct).ConfigureAwait(false);
                _logger.LogDebug("客户端连接: {RemoteEndPoint}", tcpClient.Client.RemoteEndPoint);
                _ = _HandleClientAsync(tcpClient, ct);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("ScaffoldingServer 监听任务已取消");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ScaffoldingServer 运行时发生异常");

                try
                {
                    _listener.Stop();
                }
                catch (Exception lisEx)
                {
                    _logger.LogError(lisEx, "ScaffoldingServer 停止监听端口时发生异常");
                }

                ServerStopped?.Invoke();
                break;
            }
        }
    }

    private async Task _HandleClientAsync(TcpClient tcpClient, CancellationToken ct)
    {
        var sessionId = Guid.NewGuid().ToString();
        var clientEndPoint = tcpClient.Client.RemoteEndPoint?.ToString() ?? "unknown";
        _logger.LogDebug("新连接 {SessionId} 来自 {ClientEndPoint}", sessionId, clientEndPoint);

        using (tcpClient)
        {
            var stream = tcpClient.GetStream();
            var reader = PipeReader.Create(stream);
            var writer = PipeWriter.Create(stream);

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    ReadResult readResult;
                    try
                    {
                        readResult = await reader.ReadAsync(ct).ConfigureAwait(false);
                    }
                    catch (IOException ex) when (ex.InnerException is SocketException se &&
                                                 se.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        _logger.LogInformation("连接 {SessionId} 被客户端关闭 (Connection Reset)", sessionId);
                        break;
                    }

                    var buffer = readResult.Buffer;
                    var consumedPosition = buffer.Start;

                    while (_TryParseFrame(in buffer, out var requestFrame, out var frameEndPosition))
                    {
                        _logger.LogDebug("[{SessionId}] 收到帧: {TypeInfo}", sessionId, requestFrame.TypeInfo);
                        if (_handlers.TryGetValue(requestFrame.TypeInfo, out var handler))
                        {
                            var (status, responseBody) = await handler
                                .HandleAsync(requestFrame.Body, _context, sessionId, ct).ConfigureAwait(false);

                            var responseHeader = new byte[5];
                            responseHeader[0] = status;
                            BinaryPrimitives.WriteUInt32BigEndian(responseHeader.AsSpan(1), (uint)responseBody.Length);
                            await writer.WriteAsync(responseHeader, ct).ConfigureAwait(false);
                            if (responseBody.Length > 0)
                            {
                                await writer.WriteAsync(responseBody, ct).ConfigureAwait(false);
                            }

                            await writer.FlushAsync(ct).ConfigureAwait(false);
                        }
                        else
                        {
                            _logger.LogWarning("[{SessionId}] 没有处理程序用于类型: {TypeInfo}", sessionId, requestFrame.TypeInfo);
                        }

                        consumedPosition = frameEndPosition;
                        buffer = buffer.Slice(consumedPosition);
                    }

                    reader.AdvanceTo(consumedPosition, buffer.End);

                    if (readResult.IsCompleted)
                    {
                        break;
                    }
                }
            }
            catch (InvalidDataException ex)
            {
                _logger.LogWarning("来自 {ClientEndPoint} 的连接 {SessionId} 数据格式错误，关闭连接。原因: {Message}", 
                    clientEndPoint, sessionId, ex.Message);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("连接 {SessionId} 已取消", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "连接 {SessionId} 发生意外错误", sessionId);
            }

            _logger.LogDebug("连接 {SessionId} 来自 {ClientEndPoint} 已结束", sessionId, clientEndPoint);
        }
    }

    private static bool _TryParseFrame(
        in ReadOnlySequence<byte> buffer, out (string TypeInfo, byte[] Body) frame, out SequencePosition consumed)
    {
        frame = default;
        consumed = buffer.Start;

        const int maxTypeLength = 128;
        const int maxBodyLength = 65536;

        var reader = new SequenceReader<byte>(buffer);

        if (buffer.Length < 1) return false;
        if (!reader.TryRead(out var typeLength)) return false;

        if (typeLength is 0 or > maxTypeLength)
            throw new InvalidDataException($"无效的帧类型长度: {typeLength}");

        if (reader.Remaining < typeLength + 4) return false;

        Span<byte> typeInfoSpan = stackalloc byte[typeLength];
        if (!reader.TryCopyTo(typeInfoSpan)) return false;
        reader.Advance(typeLength);
        var typeInfo = Encoding.UTF8.GetString(typeInfoSpan);

        if (!reader.TryReadBigEndian(out int bodyLength32)) return false;
        var bodyLength = (uint)bodyLength32;
        if (bodyLength > maxBodyLength)
            throw new InvalidDataException($"帧体长度 {bodyLength} 超过最大值 {maxBodyLength}");

        if (reader.Remaining < bodyLength) return false;

        var bodyBuffer = reader.Sequence.Slice(reader.Position, bodyLength);
        var body = bodyBuffer.ToArray();

        frame = (typeInfo, body);

        reader.Advance(bodyLength);
        consumed = reader.Position;

        return true;
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogDebug("进入 DisposeAsync");
        if (!_cts.IsCancellationRequested)
        {
            await _cts.CancelAsync().ConfigureAwait(false);
        }

        if (_listenTask != null)
        {
            try
            {
                await _listenTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "释放时等待监听任务发生异常");
            }
        }

        if (_cleanupTask != null)
        {
            try
            {
                await _cleanupTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "释放时等待清理任务发生异常");
            }
        }

        _listener.Stop();
        _cts.Dispose();

        _logger.LogDebug("服务器和所有后台任务已优雅停止");

        ServerStopped?.Invoke();
    }
}