using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Link;

public class BroadcastLocal(string description, int localPort, ILogger<BroadcastLocal> logger) : IDisposable
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

        logger.LogInformation("开始向本地 Minecraft 客户端广播，端口: {LocalPort}", localPort);
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _cts?.Cancel();
        _isRunning = false;

        if (_broadcastSocket != null)
        {
            try
            {
                _broadcastSocket.Shutdown(SocketShutdown.Both);
                _broadcastSocket.Close();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "关闭广播套接字时发生错误");
            }
        }

        logger.LogInformation("停止向本地 Minecraft 客户端广播");
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
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "UDP 本地广播错误");
                    await Task.Delay(5000, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UDP 本地广播任务发生错误");
        }
    }

    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
        _broadcastSocket?.Dispose();
        GC.SuppressFinalize(this);
    }
}