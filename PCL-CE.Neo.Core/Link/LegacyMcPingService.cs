using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Link;

public class LegacyMcPingService : IMcPingService
{
    private readonly ILogger<LegacyMcPingService> _logger;
    private readonly IPEndPoint _endpoint;
    private readonly string _host;
    private readonly int _timeout;
    private bool _disposed;
    private const string ModuleName = "LegacyMcPing";

    public IPEndPoint Endpoint => _endpoint;
    public string Host => _host;
    public int Timeout => _timeout;

    public LegacyMcPingService(string host, int port = 25565, int timeout = 5000)
        : this(host, new IPEndPoint(Dns.GetHostAddresses(host).First(), port), timeout)
    {
    }

    public LegacyMcPingService(string host, IPEndPoint endpoint, int timeout = 5000)
    {
        _logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<LegacyMcPingService>();
        _endpoint = endpoint;
        _host = host;
        _timeout = timeout;
    }

    public LegacyMcPingService(ILogger<LegacyMcPingService> logger, string host, int port = 25565, int timeout = 5000)
        : this(logger, host, new IPEndPoint(Dns.GetHostAddresses(host).First(), port), timeout)
    {
    }

    public LegacyMcPingService(ILogger<LegacyMcPingService> logger, string host, IPEndPoint endpoint, int timeout = 5000)
    {
        _logger = logger;
        _endpoint = endpoint;
        _host = host;
        _timeout = timeout;
    }

    public async Task<PingResult?> PingAsync(CancellationToken cancellationToken = default)
    {
        using var client = new TcpClient();
        using var timeoutCts = new CancellationTokenSource(_timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            _logger.LogDebug("Connecting to {Endpoint} (legacy protocol)", _endpoint);
            await client.ConnectAsync(_endpoint.Address, _endpoint.Port, linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Connection timeout to {Endpoint}", _endpoint);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to {Endpoint}", _endpoint);
            return null;
        }

        _logger.LogDebug("Connection established: {Endpoint}", _endpoint);

        try
        {
            await using var stream = client.GetStream();
            stream.ReadTimeout = _timeout;

            var startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var pingBytes = new byte[1];
            pingBytes[0] = 0xFE;
            await stream.WriteAsync(pingBytes, linkedCts.Token);

            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, linkedCts.Token);

            var latency = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime;
            client.Close();

            if (bytesRead < 2)
            {
                _logger.LogError("Server returned insufficient data for legacy ping");
                return null;
            }

            if (buffer[0] != 0xFF)
            {
                _logger.LogError("Invalid legacy ping response header: {Header}", buffer[0]);
                return null;
            }

            var responseData = Encoding.BigEndianUnicode.GetString(buffer, 1, bytesRead - 1);
            
            var parts = responseData.Split('\u00A7');
            
            var result = new PingResult
            {
                Success = true,
                Protocol = PingProtocol.Legacy,
                Latency = latency,
                Description = "Legacy Server"
            };

            if (parts.Length >= 1)
            {
                result = result with { Description = parts[0].Trim() };
            }

            if (parts.Length >= 2 && int.TryParse(parts[1], out var playerCount))
            {
                result = result with { PlayerCount = playerCount };
            }

            if (parts.Length >= 3 && int.TryParse(parts[2], out var maxPlayers))
            {
                result = result with { MaxPlayers = maxPlayers };
            }

            _logger.LogInformation("Legacy ping successful to {Host}:{Port}, latency: {Latency}ms",
                _host, _endpoint.Port, latency);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to legacy ping server {Endpoint}", _endpoint);
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
