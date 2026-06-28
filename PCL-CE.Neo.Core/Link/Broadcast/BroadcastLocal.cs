using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Link.Broadcast;

public class BroadcastLocal : IDisposable
{
    private Socket? _broadcastSocket;
    private CancellationTokenSource? _cts;
    private bool _isRunning;
    private readonly string _description;
    private readonly int _localPort;
    private readonly ILogger<BroadcastLocal> _logger;

    public BroadcastLocal(string description, int localPort) : this(
        description,
        localPort,
        Microsoft.Extensions.Logging.Abstractions.NullLogger<BroadcastLocal>.Instance)
    {
    }

    public BroadcastLocal(string description, int localPort, ILogger<BroadcastLocal> logger)
    {
        _description = description;
        _localPort = localPort;
        _logger = logger;
    }

    public void Start()
    {
        if (_isRunning) return;

        _cts = new CancellationTokenSource();
        _isRunning = true;

        _ = Task.Run(() => RunUdpBroadcastAsync(_cts.Token), _cts.Token);

        _logger.LogInformation("Started local Minecraft broadcast on port: {Port}", _localPort);
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _cts?.Cancel();
        _isRunning = false;

        try
        {
            _broadcastSocket?.Close();
            _broadcastSocket?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error closing broadcast socket");
        }
        _broadcastSocket = null;

        _logger.LogInformation("Stopped local Minecraft broadcast");
    }

    private async Task RunUdpBroadcastAsync(CancellationToken cancellationToken)
    {
        try
        {
            _broadcastSocket = new Socket(SocketType.Dgram, ProtocolType.Udp)
            {
                DualMode = true
            };

            var buffer = Encoding.UTF8.GetBytes($"[MOTD]{_description}[/MOTD][AD]{_localPort}[/AD]");
            var localEndpoint = new IPEndPoint(IPAddress.Loopback, 4445);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _broadcastSocket.SendToAsync(new ArraySegment<byte>(buffer), SocketFlags.None, localEndpoint);
                    await Task.Delay(1500, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "UDP local broadcast error, retrying in 5 seconds");
                    await Task.Delay(5000, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UDP local broadcast task error");
        }
    }

    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
