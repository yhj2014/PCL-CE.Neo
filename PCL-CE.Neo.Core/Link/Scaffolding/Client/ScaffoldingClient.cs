using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Abstractions;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Framing;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Models;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Requests;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Client;

internal enum ClientState
{
    Disconnected,
    Connecting,
    Handshaking,
    Connected,
    Disposing
}

public sealed class ScaffoldingClient : IAsyncDisposable
{
    private readonly SemaphoreSlim _srLock = new(1, 1);
    private TcpClient? _tcpClient;
    private PipeReader? _pipeReader;
    private PipeWriter? _pipeWriter;
    private readonly string _host;
    private readonly int _scfPort;
    private readonly ILogger<ScaffoldingClient> _logger;

    private Task? _heartbeatTask;
    private readonly PlayerPingRequest _playerPingRequest;
    private CancellationTokenSource? _heartbeatCts;
    private readonly Stopwatch _heartbeatTimer = new();

    private ClientState _state = ClientState.Disconnected;

    public event Action<IReadOnlyList<PlayerProfile>, long>? Heartbeat;
    public event Action? ServerShuttedDown;

    public IReadOnlyList<PlayerProfile>? PlayerList;
    public bool IsConnected => _state == ClientState.Connected;

    public ScaffoldingClient(string host, int scfPort, string playerName, string machineId, ILogger<ScaffoldingClient> logger)
    {
        _host = host;
        _scfPort = scfPort;
        _logger = logger;
        _playerPingRequest = new PlayerPingRequest(playerName, machineId, $"PCL CE Neo");
    }

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        if (_state is not ClientState.Disconnected)
        {
            return;
        }

        _tcpClient = new TcpClient();
        try
        {
            _state = ClientState.Connecting;

            _logger.LogInformation("尝试连接到服务器: {Host}:{Port}", _host, _scfPort);

            await _tcpClient.ConnectAsync(_host, _scfPort, ct).ConfigureAwait(false);

            var stream = _tcpClient.GetStream();
            _pipeReader = PipeReader.Create(stream);
            _pipeWriter = PipeWriter.Create(stream);

            _state = ClientState.Handshaking;
            _logger.LogInformation("连接已建立，正在执行握手...");

            await SendRequestAsync(_playerPingRequest, ct).ConfigureAwait(false);

            _state = ClientState.Connected;
            _logger.LogInformation("连接成功");

            _StartHeartbeats();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "连接服务器失败");

            await DisposeAsync().ConfigureAwait(false);
            ServerShuttedDown?.Invoke();

            throw;
        }
    }

    private void _StartHeartbeats()
    {
        _heartbeatCts = new CancellationTokenSource();
        _heartbeatTask = _HeartbeatLoopAsync(_heartbeatCts.Token);
    }

    private async Task _HeartbeatLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), ct).ConfigureAwait(false);

                _heartbeatTimer.Start();
                await SendRequestAsync(_playerPingRequest, ct).ConfigureAwait(false);
                _heartbeatTimer.Stop();

                var latency = _heartbeatTimer.ElapsedMilliseconds;
                _heartbeatTimer.Reset();

                PlayerList = await SendRequestAsync(new GetPlayerProfileListRequest(), ct).ConfigureAwait(false);

                Heartbeat?.Invoke(PlayerList, latency);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送心跳消息失败，可能服务器已关闭");

                ServerShuttedDown?.Invoke();
                break;
            }
        }
    }

    public async Task<TResponse> SendRequestAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken ct = default)
    {
        if (_state < ClientState.Handshaking)
        {
            throw new InvalidOperationException("客户端未连接");
        }

        if (_pipeWriter is null || _pipeReader is null)
        {
            throw new InvalidOperationException("客户端未连接");
        }

        await _srLock.WaitAsync(ct).ConfigureAwait(false);

        try
        {
            await ProtocolWriter.WriteRequestAsync(_pipeWriter, request, ct).ConfigureAwait(false);
            var response = await ProtocolReader.ReadResponseAsync(_pipeReader, ct).ConfigureAwait(false);

            return request.ParseResponseBody(response.Body);
        }
        finally
        {
            _srLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_state is ClientState.Disposing)
        {
            return;
        }

        _state = ClientState.Disposing;

        await CastAndDispose(_srLock).ConfigureAwait(false);
        if (_tcpClient != null) await CastAndDispose(_tcpClient).ConfigureAwait(false);
        if (_heartbeatCts != null)
        {
            await _heartbeatCts.CancelAsync().ConfigureAwait(false);
            await CastAndDispose(_heartbeatCts).ConfigureAwait(false);
        }

        if (_heartbeatTask != null)
        {
            await _heartbeatTask.ConfigureAwait(false);
            await CastAndDispose(_heartbeatTask).ConfigureAwait(false);
        }

        return;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
                await resourceAsyncDisposable.DisposeAsync().ConfigureAwait(false);
            else
                resource.Dispose();
        }
    }
}