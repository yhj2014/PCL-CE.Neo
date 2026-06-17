using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Link;

public class BroadcastLocal(string description, int localPort) : IDisposable
{
    private Socket? _broadcastSocket;
    private CancellationTokenSource? _cts;
    private bool _isRunning;

    public void Start()
    {
        if (_isRunning) return;

        _cts = new CancellationTokenSource();
        _isRunning = true;

        _ = Task.Run(() => _RunUdpBroadcastAsync(_cts.Token), _cts.Token);
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _cts?.Cancel();
        _isRunning = false;

        _broadcastSocket?.SafeClose();
    }

    private async Task _RunUdpBroadcastAsync(CancellationToken cancellationToken)
    {
        try
        {
            _broadcastSocket = new Socket(SocketType.Dgram, ProtocolType.Udp)
            {
                DualMode = true
            };

            var buffer = Encoding.UTF8.GetBytes($"[MOTD]{description}[/MOTD][AD]{localPort}[/AD]");
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
                catch (Exception)
                {
                    await Task.Delay(5000, cancellationToken);
                }
            }
        }
        catch
        {
        }
    }

    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
        _broadcastSocket?.SafeClose();
        GC.SuppressFinalize(this);
    }
}