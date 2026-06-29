using PCL_CE.Neo.Core.Link.Scaffolding.Client.Abstractions;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Framing;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Models;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Requests;
using PCL_CE.Neo.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Client;

internal enum ClientState
{
    Disconnected,
    Connecting,
    Handshaking,
    Connected,
    Disposing
}

public sealed class ScaffoldingClient(string host, int scfPort, string playerName, string machineId, string vendor)
    : IAsyncDisposable
{
    private readonly SemaphoreSlim _srLock = new(1, 1);
    private TcpClient? _tcpClient;
    private PipeReader? _pipeReader;
    private PipeWriter? _pipeWriter;

    private Task? _heartbeatTask;
    private readonly PlayerPingRequest _playerPingRequest = new(playerName, machineId, vendor);
    private CancellationTokenSource? _heartbeatCts;
    private readonly Stopwatch _heartbeatTimer = new();

    private ClientState _state = ClientState.Disconnected;

    public event Action<IReadOnlyList<PlayerProfile>, long>? Heartbeat;

    public event Action? ServerShuttedDown;

    public IReadOnlyList<PlayerProfile>? PlayerList;

    public bool IsConnected => _state == ClientState.Connected;

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

            LogWrapper.Info("Scaffolding", $"Trying to connect to server: {host}:{scfPort}");

            await _tcpClient.ConnectAsync(host, scfPort, ct).ConfigureAwait(false);

            var stream = _tcpClient.GetStream();
            _pipeReader = PipeReader.Create(stream);
            _pipeWriter = PipeWriter.Create(stream);

            _state = ClientState.Handshaking;
            LogWrapper.Info("Scaffolding", "Connection established. Performing handshake...");

            await SendRequestAsync(_playerPingRequest, ct).ConfigureAwait(false);

            _state = ClientState.Connected;

            _StartHeartbeats();
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "ScaffoldingClient", "Failed to connect to server.");

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
                LogWrapper.Error(ex, "[ScaffoldingClient]",
                    "Failed when sending heartbeat message. Maybe the server has been shut down.");

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
            throw new InvalidOperationException("Client is not connected.");
        }

        if (_pipeWriter is null || _pipeReader is null)
        {
            throw new InvalidOperationException("Client is not connected.");
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