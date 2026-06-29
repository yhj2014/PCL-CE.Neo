using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Link;

public interface IBroadcastLocal
{
    Task StartAsync(string serverAddress, int serverPort = 25565);
    Task StopAsync();
    bool IsRunning { get; }
}

public class BroadcastLocal : IBroadcastLocal
{
    private UdpClient? _udpClient;
    private bool _isRunning;
    private string? _serverAddress;
    private int _serverPort;
    private BroadcastOptions _options = new();
    private CancellationTokenSource? _cts;

    public bool IsRunning => _isRunning;

    public BroadcastLocal(BroadcastOptions? options = null)
    {
        _options = options ?? new BroadcastOptions();
    }

    public async Task StartAsync(string serverAddress, int serverPort = 25565)
    {
        try
        {
            _serverAddress = serverAddress;
            _serverPort = serverPort;

            _udpClient = new UdpClient();
            _isRunning = true;
            _cts = new CancellationTokenSource();

            _ = BroadcastLoopAsync(_cts.Token);

            LogWrapper.Info($"Local broadcast started for {serverAddress}:{serverPort}");
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to start local broadcast for {serverAddress}:{serverPort}");
            throw;
        }
    }

    public async Task StopAsync()
    {
        try
        {
            _cts?.Cancel();
            _isRunning = false;

            if (_udpClient != null)
            {
                _udpClient.Close();
                _udpClient = null;
            }

            LogWrapper.Info("Local broadcast stopped");
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to stop local broadcast");
            throw;
        }
    }

    private async Task BroadcastLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                await SendBroadcastAsync();
                await Task.Delay(_options.BroadcastInterval, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Local broadcast loop stopped");
            _isRunning = false;
        }
    }

    private async Task SendBroadcastAsync()
    {
        try
        {
            if (_udpClient == null)
                return;

            var message = $"PCL-CE-NEO::{_serverAddress}::{_serverPort}";
            var data = Encoding.UTF8.GetBytes(message);

            var endPoint = new IPEndPoint(IPAddress.Broadcast, _options.BroadcastPort);
            await _udpClient.SendAsync(data, data.Length, endPoint);
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to send broadcast");
        }
    }
}