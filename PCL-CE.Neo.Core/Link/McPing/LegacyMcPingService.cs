using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Link.McPing.Model;

namespace PCL_CE.Neo.Core.Link.McPing;

public class LegacyMcPingService : IMcPingService
{
    private readonly IPEndPoint _endpoint;
    private readonly string _host;
    private const int DefaultTimeout = 10000;
    private readonly int _timeout;
    private bool _disposed;
    private const string ModuleName = "LegacyMcPing";

    private readonly ILogger<LegacyMcPingService> _logger;

    public IPEndPoint Endpoint => _endpoint;
    public string Host => _host;
    public int Timeout => _timeout;

    public LegacyMcPingService(IPEndPoint endpoint, int timeout = DefaultTimeout)
        : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<LegacyMcPingService>.Instance, endpoint, timeout)
    {
    }

    public LegacyMcPingService(ILogger<LegacyMcPingService> logger, IPEndPoint endpoint, int timeout = DefaultTimeout)
    {
        _logger = logger;
        _endpoint = endpoint;
        _host = _endpoint.Address.ToString();
        _timeout = timeout;
    }

    public LegacyMcPingService(string ip, int port = 25565, int timeout = DefaultTimeout)
        : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<LegacyMcPingService>.Instance, ip, port, timeout)
    {
    }

    public LegacyMcPingService(ILogger<LegacyMcPingService> logger, string ip, int port = 25565, int timeout = DefaultTimeout)
    {
        _logger = logger;
        _endpoint = IPAddress.TryParse(ip, out var ipAddress)
            ? new IPEndPoint(ipAddress, port)
            : new IPEndPoint(Dns.GetHostAddresses(ip).First(), port);
        _host = ip;
        _timeout = timeout;
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

        try
        {
            await so.ConnectAsync(_endpoint, linkedCts.Token);
            _logger.LogDebug("{ModuleName}: Connected to {Endpoint}", ModuleName, _endpoint);
        }
        catch (OperationCanceledException)
        {
            _logger.LogError(new TimeoutException("连接超时"), "{ModuleName}: Failed to connect to {Endpoint}", ModuleName, _endpoint);
            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{ModuleName}: Failed to connect to {Endpoint}", ModuleName, _endpoint);
            return null;
        }

        await using var stream = new NetworkStream(so, false);

        var queryPack = new byte[] { 0xfe, 0x01 };
        await stream.WriteAsync(queryPack.AsMemory(0, queryPack.Length), linkedCts.Token);
        
        var ms = new MemoryStream();
        try
        {
            await stream.CopyToAsync(ms, linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogError(new TimeoutException("读取超时"), "{ModuleName}: Read timed out from {Endpoint}", ModuleName, _endpoint);
            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{ModuleName}: Failed to read from {Endpoint}", ModuleName, _endpoint);
            return null;
        }
        finally
        {
            so.Close();
        }

        var retData = ms.ToArray();
        if (retData.Length < 21 || (retData.Length >= 21 && retData[0] != 0xff))
        {
            _logger.LogInformation("{ModuleName}: Unknown response from {Endpoint}, ignore", ModuleName, _endpoint);
            return null;
        }

        var retRep = Encoding.UTF8.GetString(retData);
        try
        {
            var retPart = retRep.Split(["\0\0\0"], StringSplitOptions.None);
            retPart = retPart
                .Select(s => new string([.. s.Where((_, index) => index % 2 == 0)]))
                .ToArray();
            if (retPart.Length < 6)
            {
                _logger.LogWarning("{ModuleName}: Invalid response format from {Endpoint}", ModuleName, _endpoint);
                return null;
            }

            _logger.LogInformation("{ModuleName}: Legacy ping successful - {Endpoint}", ModuleName, _endpoint);
            return new McPingResult(
                new McPingVersionResult(retPart[2], int.Parse(retPart[1])),
                new McPingPlayerResult(int.Parse(retPart[5]), int.Parse(retPart[4]), []),
                retPart[3],
                string.Empty,
                0,
                new McPingModInfoResult(string.Empty, []),
                null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{ModuleName}: Unable to parse response from {Endpoint}", ModuleName, _endpoint);
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