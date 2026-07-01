using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

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

    private readonly ILogger<BroadcastListener> _logger = 
        Microsoft.Extensions.Logging.Abstractions.NullLogger<BroadcastListener>.Instance;

    public event Action<BroadcastRecord, IPEndPoint>? OnReceive;

    public BroadcastListener(ILogger<BroadcastListener> logger, bool receiveLocalOnly = true)
        : this(receiveLocalOnly)
    {
        _logger = logger;
    }

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
            _logger.LogInformation("BroadcastListener: IPv4 listener started on port 4445");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BroadcastListener: Failed to start IPv4 listener");
            _client?.Dispose();
            _client = null;
        }

        try
        {
            _clientV6 = new UdpClient(AddressFamily.InterNetworkV6);
            _clientV6.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _clientV6.Client.Bind(new IPEndPoint(IPAddress.IPv6Any, 4445));
            _clientV6.JoinMulticastGroup(_MulticastAddressV6);
            _logger.LogInformation("BroadcastListener: IPv6 listener started on port 4445");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BroadcastListener: Failed to start IPv6 listener");
            _clientV6?.Dispose();
            _clientV6 = null;
        }

        if (_client != null)
            _listenTask = _ListenThreadAsync(_client);
        if (_clientV6 != null)
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

                _logger.LogDebug("BroadcastListener: Received broadcast from {Endpoint}: {Motd}", senderEndpoint, serverInfo.Motd);
                OnReceive?.Invoke(serverInfo, senderEndpoint);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "BroadcastListener: Error processing packet");
            }
        }
    }

    private static bool _IsAddressLocal(IPAddress address)
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        return host.AddressList.Contains(address);
    }

    private static bool _TryParseServerInfo(string rawMessage, out BroadcastRecord? serverInfo)
    {
        serverInfo = null;

        var motdMatch = System.Text.RegularExpressions.Regex.Match(rawMessage, @"\[MOTD\](.*?)\[/MOTD\]", System.Text.RegularExpressions.RegexOptions.Singleline);
        var adMatch = System.Text.RegularExpressions.Regex.Match(rawMessage, @"\[AD\](.*?)\[/AD\]");

        if (!adMatch.Success || !int.TryParse(adMatch.Groups[1].Value.Trim(), out int port))
        {
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
        }
        catch
        {
        }
        _client = null;

        try
        {
            _clientV6?.Close();
            _clientV6?.Dispose();
        }
        catch
        {
        }
        _clientV6 = null;

        try
        {
            _listenTask?.Wait(500);
        }
        catch
        {
        }
        try
        {
            _listenTaskV6?.Wait(500);
        }
        catch
        {
        }

        _logger.LogInformation("BroadcastListener: Stopped");
    }

    private bool _isDisposed;
    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        Stop();
    }
}