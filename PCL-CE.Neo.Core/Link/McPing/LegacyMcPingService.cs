using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Link.McPing;

public class LegacyMcPingService : IMcPingService
{
    private readonly IPEndPoint _endpoint;
    private readonly string _host;
    private readonly int _timeout;
    private bool _disposed;
    private readonly ILogger<LegacyMcPingService> _logger;
    private const int DefaultTimeout = 10000;

    public IPEndPoint Endpoint => _endpoint;
    public string Host => _host;
    public int Timeout => _timeout;

    public LegacyMcPingService(IPEndPoint endpoint, int timeout = DefaultTimeout) : this(
        endpoint,
        endpoint.Address.ToString(),
        timeout,
        Microsoft.Extensions.Logging.Abstractions.NullLogger<LegacyMcPingService>.Instance)
    {
    }

    public LegacyMcPingService(IPEndPoint endpoint, int timeout, ILogger<LegacyMcPingService> logger) : this(
        endpoint,
        endpoint.Address.ToString(),
        timeout,
        logger)
    {
    }

    public LegacyMcPingService(string ip, int port = 25565, int timeout = DefaultTimeout) : this(
        new IPEndPoint(Dns.GetHostAddresses(ip).First(), port),
        ip,
        timeout,
        Microsoft.Extensions.Logging.Abstractions.NullLogger<LegacyMcPingService>.Instance)
    {
    }

    public LegacyMcPingService(string ip, int port, int timeout, ILogger<LegacyMcPingService> logger)
    {
        _endpoint = Dns.GetHostAddresses(ip).First() is { } ipAddress
            ? new IPEndPoint(ipAddress, port)
            : throw new ArgumentException($"Unable to resolve host: {ip}", nameof(ip));
        _host = ip;
        _timeout = timeout;
        _logger = logger;
    }

    private LegacyMcPingService(IPEndPoint endpoint, string host, int timeout, ILogger<LegacyMcPingService> logger)
    {
        _endpoint = endpoint;
        _host = host;
        _timeout = timeout;
        _logger = logger;
    }

    public async Task<McPingResult?> PingAsync(CancellationToken cancellationToken = default)
    {
        using var so = new Socket(SocketType.Stream, ProtocolType.Tcp);
        using var timeoutCts = new CancellationTokenSource(_timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        linkedCts.Token.Register(() =>
        {
            try
            {
                if (so.Connected) so.Close();
            }
            catch (ObjectDisposedException)
            {
            }
        });

        await so.ConnectAsync(_endpoint, linkedCts.Token);
        _logger.LogDebug("Connected to {Endpoint}", _endpoint);
        await using var stream = new NetworkStream(so, false);

        var queryPack = new byte[] { 0xfe, 0x01 };
        await stream.WriteAsync(queryPack.AsMemory(0, queryPack.Length), linkedCts.Token);
        var ms = new MemoryStream();
        await stream.CopyToAsync(ms, linkedCts.Token);
        so.Close();
        var retData = ms.ToArray();
        
        if (retData.Length < 21 || (retData.Length >= 21 && retData[0] != 0xff))
        {
            _logger.LogInformation("Unknown response from {Endpoint}, ignoring", _endpoint);
            return null;
        }

        var retRep = Encoding.UTF8.GetString(retData);
        try
        {
            var retPart = retRep.Split(["\0\0\0"], StringSplitOptions.None);
            retPart = retPart
                .Select(s => new string(s.Where((_, index) => index % 2 == 0).ToArray()))
                .ToArray();
                
            if (retPart.Length < 6)
                return null;
                
            return new McPingResult(
                new McPingVersionResult(retPart[2], int.Parse(retPart[1])),
                new McPingPlayerResult(int.Parse(retPart[5]), int.Parse(retPart[4]), []),
                retPart[3],
                string.Empty,
                0,
                new McPingModInfoResult(string.Empty, []),
                null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to parse response from {Endpoint}", _endpoint);
            return null;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
