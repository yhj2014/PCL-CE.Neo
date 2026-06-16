using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.Link.Broadcast;

public class BroadcastLocal(string description, int localPort) : IDisposable
{
    private const string ModuleName = "BroadcastLocal";
    private Socket? _broadcastSocket;
    private CancellationTokenSource? _cts;
    private bool _isRunning;

    public void Start()
    {
        if (_isRunning) return;

        _cts = new CancellationTokenSource();
        _isRunning = true;

        _ = Task.Run(() => _RunUdpBroadcastAsync(_cts.Token), _cts.Token);

        LogWrapper.Info(ModuleName, $"开始向本地 Minecraft 客户端广播，端口: {localPort}");
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _cts?.Cancel();
        _isRunning = false;

        try
        {
            _broadcastSocket?.Close();
        }
        catch
        {
        }

        LogWrapper.Info(ModuleName, "停止向本地 Minecraft 客户端广播");
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
                    LogWrapper.Error(ex, ModuleName, "UDP 本地广播错误");
                    await Task.Delay(5000, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "UDP 本地广播任务发生错误");
        }
    }

    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
        _broadcastSocket?.Close();
        GC.SuppressFinalize(this);
    }
}