using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Utils;

namespace PCL_CE.Neo.Core.Link;

public class BroadcastListener(bool receiveLocalOnly, ILogger<BroadcastListener> logger) : IDisposable
{
    private UdpClient? _client;
    private UdpClient? _clientV6;
    private CancellationTokenSource? _cts;
    private static readonly IPAddress _MulticastAddress = IPAddress.Parse("224.0.2.60");
    private static readonly IPAddress _MulticastAddressV6 = IPAddress.Parse("ff75:230::60");
    private Task? _listenTask;
    private Task? _listenTaskV6;

    public event Action<BroadcastRecord, IPEndPoint>? OnReceive;

    public void Start()
    {
        if (_client is not null || _clientV6 is not null) return;
        _cts = new CancellationTokenSource();

        try
        {
            _client = new UdpClient();
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.Client.Bind(new IPEndPoint(IPAddress.Any, 4445));
            _client.JoinMulticastGroup(_MulticastAddress);

            _clientV6 = new UdpClient(AddressFamily.InterNetworkV6);
            _clientV6.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _clientV6.Client.Bind(new IPEndPoint(IPAddress.IPv6Any, 4445));
            _clientV6.JoinMulticastGroup(_MulticastAddressV6);

            _listenTask = _ListenThreadAsync(_client);
            _listenTaskV6 = _ListenThreadAsync(_clientV6);

            logger.LogInformation("广播监听器已启动，监听端口 4445");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "启动广播监听器失败");
            Stop();
            throw;
        }
    }

    private async Task _ListenThreadAsync(UdpClient? client)
    {
        while (_cts is not null && client is not null && !_cts.IsCancellationRequested)
        {
            try
            {
                var result = await client.ReceiveAsync(_cts.Token);
                var receivedData = result.Buffer;
                var senderEndpoint = result.RemoteEndPoint;

                var message = Encoding.UTF8.GetString(receivedData);

                if (!_TryParseServerInfo(message, out var serverInfo) || serverInfo is null) continue;
                if (receiveLocalOnly && !_IsAddressLocal(senderEndpoint.Address)) continue;
                OnReceive?.Invoke(serverInfo, senderEndpoint);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "处理广播数据包时发生错误");
            }
        }
    }

    private static bool _IsAddressLocal(IPAddress address)
    {
        if (address.IsLoopback) return true;
        
        var bytes = address.GetAddressBytes();
        if (bytes.Length == 4)
        {
            if (bytes[0] == 10) return true;
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;
            if (bytes[0] == 192 && bytes[1] == 168) return true;
            if (bytes[0] == 169 && bytes[1] == 254) return true;
            if (bytes[0] == 127) return true;
        }
        else if (bytes.Length == 16)
        {
            if (address.IsIPv6LinkLocal) return true;
            if (address.IsIPv6SiteLocal) return true;
            if (address.IsIPv6Loopback) return true;
        }
        return false;
    }

    private static bool _TryParseServerInfo(string rawMessage, out BroadcastRecord? serverInfo)
    {
        var motdMatch = RegexPatterns.BroadcastMotd.Match(rawMessage);
        var adMatch = RegexPatterns.BroadcastAd.Match(rawMessage);

        if (!adMatch.Success || !int.TryParse(adMatch.Groups[1].Value.Trim(), out int port))
        {
            serverInfo = null;
            return false;
        }

        serverInfo = new BroadcastRecord(
            motdMatch.Success ? motdMatch.Groups[1].Value.Trim() : "missing no",
            new IPEndPoint(IPAddress.Loopback, port),
            DateTime.Now);
        return true;
    }

    public void Stop()
    {
        if (_client is null) return;

        _cts?.Cancel();
        
        try
        {
            _client?.DropMulticastGroup(_MulticastAddress);
            _client?.Close();
            _client?.Dispose();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "关闭 IPv4 监听器时发生错误");
        }
        _client = null;

        try
        {
            _clientV6?.DropMulticastGroup(_MulticastAddressV6);
            _clientV6?.Close();
            _clientV6?.Dispose();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "关闭 IPv6 监听器时发生错误");
        }
        _clientV6 = null;

        try
        {
            if (_listenTask != null && !_listenTask.IsCompleted)
                _listenTask.Wait(500);
            if (_listenTaskV6 != null && !_listenTaskV6.IsCompleted)
                _listenTaskV6.Wait(500);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "等待监听任务结束时发生错误");
        }

        _cts?.Dispose();
        _cts = null;

        logger.LogInformation("广播监听器已停止");
    }

    private bool _isDisposed;
    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        Stop();
    }
}