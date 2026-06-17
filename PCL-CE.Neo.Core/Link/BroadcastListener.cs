using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Link;

public class BroadcastListener(bool receiveLocalOnly = true) : IDisposable
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
            catch
            {
            }
        }
    }

    private static bool _IsAddressLocal(IPAddress address)
    {
        var ips = NetworkInterface.GetAllNetworkInterfaces()
            .SelectMany(iface => iface.GetIPProperties().UnicastAddresses)
            .Select(addr => addr.Address);
        return ips.Contains(address);
    }

    private static bool _TryParseServerInfo(string rawMessage, out BroadcastRecord? serverInfo)
    {
        var motdMatch = Regex.Match(rawMessage, @"\[MOTD\](.*?)\[/MOTD\]");
        var adMatch = Regex.Match(rawMessage, @"\[AD\](.*?)\[/AD\]");

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
        _client?.Close();
        _client?.Dispose();
        _client = null;
        _clientV6?.Close();
        _clientV6?.Dispose();
        _clientV6 = null;

        _listenTask?.Wait(500);
        _listenTaskV6?.Wait(500);
    }

    private bool _isDisposed;
    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        Stop();
    }
}