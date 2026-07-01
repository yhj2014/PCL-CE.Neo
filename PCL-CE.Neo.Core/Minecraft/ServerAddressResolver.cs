using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft;

public class ServerAddressResolver
{
    private readonly ILogger<ServerAddressResolver> _logger;
    private const string ModuleName = "ServerAddressResolver";

    public ServerAddressResolver()
        : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<ServerAddressResolver>.Instance)
    {
    }

    public ServerAddressResolver(ILogger<ServerAddressResolver> logger)
    {
        _logger = logger;
    }

    public async Task<IPEndPoint> ResolveAsync(string address, int defaultPort = 25565)
    {
        _logger.LogDebug("{ModuleName}: Resolving address: {Address}", ModuleName, address);

        var (host, port) = ParseAddress(address, defaultPort);
        _logger.LogDebug("{ModuleName}: Parsed host: {Host}, port: {Port}", ModuleName, host, port);

        IPAddress ipAddress;
        if (IPAddress.TryParse(host, out ipAddress))
        {
            _logger.LogDebug("{ModuleName}: Address is already an IP: {IpAddress}", ModuleName, ipAddress);
            return new IPEndPoint(ipAddress, port);
        }

        try
        {
            var addresses = await Dns.GetHostAddressesAsync(host);
            if (addresses.Length == 0)
            {
                _logger.LogError("{ModuleName}: No addresses found for host: {Host}", ModuleName, host);
                throw new ArgumentException($"无法解析主机: {host}");
            }

            ipAddress = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork) ?? addresses[0];

            _logger.LogDebug("{ModuleName}: Resolved host {Host} to IP {IpAddress}", ModuleName, host, ipAddress);
            return new IPEndPoint(ipAddress, port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ModuleName}: Failed to resolve host: {Host}", ModuleName, host);
            throw;
        }
    }

    public (string Host, int Port) ParseAddress(string address, int defaultPort = 25565)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("地址不能为空", nameof(address));

        address = address.Trim();

        var bracketMatch = Regex.Match(address, @"^\[(.*?)\]:(\d+)$");
        if (bracketMatch.Success)
        {
            return (bracketMatch.Groups[1].Value, int.Parse(bracketMatch.Groups[2].Value));
        }

        var colonIndex = address.LastIndexOf(':');
        if (colonIndex > 0)
        {
            var portPart = address.Substring(colonIndex + 1);
            if (int.TryParse(portPart, out var port))
            {
                return (address.Substring(0, colonIndex), port);
            }
        }

        return (address, defaultPort);
    }
}