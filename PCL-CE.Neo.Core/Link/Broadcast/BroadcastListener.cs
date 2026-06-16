using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.Link.Broadcast;

public class BroadcastListener(bool receiveLocalOnly = true) : IDisposable
{
    private const string ModuleName = "BroadcastListener";
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

            LogWrapper.Info(ModuleName, "广播监听器已启动");
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "启动广播监听器失败");
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
                LogWrapper.Error(ex, ModuleName, "处理广播消息失败");
            }
        }
    }

    private static bool _IsAddressLocal(IPAddress address)
    {
        try
        {
            var hostName = Dns.GetHostName();
            var hostEntry = Dns.GetHostEntry(hostName);
            return hostEntry.AddressList.Contains(address);
        }
        catch
        {
            return address.IsLoopback || 
                   IPAddress.IsLoopback(address) ||
                   address.ToString().StartsWith("127.") ||
                   address.ToString().StartsWith("192.168.") ||
                   address.ToString().StartsWith("10.");
        }
    }

    private static bool _TryParseServerInfo(string rawMessage, out BroadcastRecord? serverInfo)
    {
        var motdMatch = Regex.Match(rawMessage, @"\[MOTD\](.*?)\[/MOTD\]", RegexOptions.Compiled);
        var adMatch = Regex.Match(rawMessage, @"\[AD\](.*?)\[/AD\]", RegexOptions.Compiled);

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
        _cts?.Dispose();
        _cts = null;

        try
        {
            _client?.Close();
            _client?.Dispose();
            _client = null;
            _clientV6?.Close();
            _clientV6?.Dispose();
            _clientV6 = null;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "停止广播监听器失败");
        }

        _listenTask?.Wait(500);
        _listenTaskV6?.Wait(500);

        LogWrapper.Info(ModuleName, "广播监听器已停止");
    }

    private bool _isDisposed;
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        Stop();
    }
}