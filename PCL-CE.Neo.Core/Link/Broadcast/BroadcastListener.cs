using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Utils;

namespace PCL_CE.Neo.Core.Link.Broadcast;

public class BroadcastListener : IDisposable
{
    private UdpClient? _client;
    private UdpClient? _clientV6;
    private CancellationTokenSource? _cts;
    private Task? _listenTask;
    private Task? _listenTaskV6;
    private readonly ILogger<BroadcastListener> _logger;
    private readonly bool _receiveLocalOnly;
    private bool _disposed;

    private static readonly IPAddress MulticastAddress = IPAddress.Parse("224.0.2.60");
    private static readonly IPAddress MulticastAddressV6 = IPAddress.Parse("ff75:230::60");

    public event Action<BroadcastRecord, IPEndPoint>? OnReceive;

    public BroadcastListener(bool receiveLocalOnly = true) : this(
        receiveLocalOnly,
        Microsoft.Extensions.Logging.Abstractions.NullLogger<BroadcastListener>.Instance)
    {
    }

    public BroadcastListener(bool receiveLocalOnly, ILogger<BroadcastListener> logger)
    {
        _receiveLocalOnly = receiveLocalOnly;
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
            _client.JoinMulticastGroup(MulticastAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to setup IPv4 broadcast listener");
            _client?.Dispose();
            _client = null;
        }

        try
        {
            _clientV6 = new UdpClient(AddressFamily.InterNetworkV6);
            _clientV6.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _clientV6.Client.Bind(new IPEndPoint(IPAddress.IPv6Any, 4445));
            _clientV6.JoinMulticastGroup(MulticastAddressV6);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to setup IPv6 broadcast listener");
            _clientV6?.Dispose();
            _clientV6 = null;
        }

        if (_client != null)
            _listenTask = ListenThreadAsync(_client);
        if (_clientV6 != null)
            _listenTaskV6 = ListenThreadAsync(_clientV6);
            
        _logger.LogInformation("Broadcast listener started");
    }

    private async Task ListenThreadAsync(UdpClient? client)
    {
        if (client == null) return;
        
        while (_cts is not null && !_cts.IsCancellationRequested)
        {
            try
            {
                var result = await client.ReceiveAsync(_cts.Token);
                var receivedData = result.Buffer;
                var senderEndpoint = result.RemoteEndPoint;

                var message = Encoding.UTF8.GetString(receivedData);

                if (!TryParseServerInfo(message, out var serverInfo) || serverInfo is null) continue;
                if (_receiveLocalOnly && !IsAddressLocal(senderEndpoint.Address)) continue;
                
                OnReceive?.Invoke(serverInfo, senderEndpoint);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error processing broadcast packet");
            }
        }
    }

    private static bool IsAddressLocal(IPAddress address)
    {
        var ips = NetworkUtils.GetAllLocalAddress();
        return ips.Contains(address);
    }

    private static bool TryParseServerInfo(string rawMessage, out BroadcastRecord? serverInfo)
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
        if (_client == null && _clientV6 == null) return;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        
        try
        {
            _client?.Close();
            _client?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error closing IPv4 client");
        }
        _client = null;
        
        try
        {
            _clientV6?.Close();
            _clientV6?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error closing IPv6 client");
        }
        _clientV6 = null;

        _listenTask?.Wait(500);
        _listenTaskV6?.Wait(500);
        
        _logger.LogInformation("Broadcast listener stopped");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
        GC.SuppressFinalize(this);
    }
}
