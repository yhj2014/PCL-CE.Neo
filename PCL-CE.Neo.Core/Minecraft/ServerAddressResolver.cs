using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft;

public class ServerAddressResolver
{
    private readonly ILogger<ServerAddressResolver> _logger;

    public ServerAddressResolver(ILogger<ServerAddressResolver> logger)
    {
        _logger = logger;
    }

    public async Task<(string Host, int Port)> ResolveAsync(string address, int defaultPort = 25565)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentNullException(nameof(address));

        try
        {
            var parts = address.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1], out var port))
            {
                var host = (await ResolveHostAsync(parts[0])).ToString();
                _logger.LogDebug("Resolved address '{Address}' to {Host}:{Port}", address, host, port);
                return (host, port);
            }
            else
            {
                var host = (await ResolveHostAsync(address)).ToString();
                _logger.LogDebug("Resolved address '{Address}' to {Host}:{DefaultPort}", address, host, defaultPort);
                return (host, defaultPort);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve address: {Address}", address);
            throw;
        }
    }

    public async Task<IPAddress> ResolveHostAsync(string hostname)
    {
        if (string.IsNullOrWhiteSpace(hostname))
            throw new ArgumentNullException(nameof(hostname));

        try
        {
            if (IPAddress.TryParse(hostname, out var ipAddress))
                return ipAddress;

            var hostEntry = await Dns.GetHostEntryAsync(hostname);
            if (hostEntry.AddressList.Length == 0)
                throw new SocketException((int)SocketError.HostNotFound);

            foreach (var addr in hostEntry.AddressList)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                    return addr;
            }

            return hostEntry.AddressList[0];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve hostname: {Hostname}", hostname);
            throw;
        }
    }

    public async Task<bool> CanReachAsync(string host, int port, int timeoutMs = 3000)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentNullException(nameof(host));
        if (port < 1 || port > 65535)
            throw new ArgumentOutOfRangeException(nameof(port));

        try
        {
            using var tcpClient = new TcpClient();
            var connectTask = tcpClient.ConnectAsync(host, port);
            var timeoutTask = Task.Delay(timeoutMs);

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                _logger.LogDebug("Connection to {Host}:{Port} timed out", host, port);
                return false;
            }

            await connectTask;
            var result = tcpClient.Connected;
            _logger.LogDebug("Connection to {Host}:{Port} {Result}", host, port, result ? "succeeded" : "failed");
            return result;
        }
        catch (SocketException ex)
        {
            _logger.LogDebug("Connection to {Host}:{Port} failed: {Message}", host, port, ex.Message);
            return false;
        }
    }

    public bool IsLocalAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return false;

        try
        {
            if (IPAddress.TryParse(address, out var ipAddress))
            {
                return IPAddress.IsLoopback(ipAddress) ||
                       ipAddress.AddressFamily == AddressFamily.InterNetwork &&
                       (ipAddress.GetAddressBytes()[0] == 10 ||
                        (ipAddress.GetAddressBytes()[0] == 172 && ipAddress.GetAddressBytes()[1] >= 16 && ipAddress.GetAddressBytes()[1] <= 31) ||
                        (ipAddress.GetAddressBytes()[0] == 192 && ipAddress.GetAddressBytes()[1] == 168));
            }

            return address.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                   address.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                   address.Equals("::1", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public bool IsValidServerAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return false;

        var parts = address.Split(':');
        if (parts.Length > 2)
            return false;

        if (parts.Length == 2)
        {
            if (!int.TryParse(parts[1], out var port))
                return false;
            if (port < 1 || port > 65535)
                return false;
        }

        var hostname = parts[0];
        if (string.IsNullOrWhiteSpace(hostname))
            return false;

        try
        {
            return IPAddress.TryParse(hostname, out _) ||
                   hostname.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '-');
        }
        catch
        {
            return false;
        }
    }
}