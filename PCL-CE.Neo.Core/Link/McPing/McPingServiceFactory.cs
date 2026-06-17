using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;

namespace PCL_CE.Neo.Core.Link.McPing;

public static class McPingServiceFactory
{
    public static IMcPingService CreateService(IPEndPoint endpoint, ILogger<McPingService>? logger = null, int timeout = 10000)
    {
        return new McPingService(endpoint, logger, timeout);
    }

    public static IMcPingService CreateService(string ip, ILogger<McPingService>? logger = null, int port = 25565, int timeout = 10000)
    {
        return new McPingService(ip, logger, port, timeout);
    }

    public static IMcPingService CreateService(string host, string? ip, int port = 25565, ILogger<McPingService>? logger = null)
    {
        return CreateService(host, ip, port, 10000, logger);
    }

    public static IMcPingService CreateService(string host, string? ip, int port, int timeout, ILogger<McPingService>? logger = null)
    {
        return !string.IsNullOrWhiteSpace(ip) && IPAddress.TryParse(ip, out var ipAddress)
            ? new McPingService(host, new IPEndPoint(ipAddress, port), logger, timeout)
            : new McPingService(host, logger, port, timeout);
    }

    public static IMcPingService CreateLegacyService(IPEndPoint endpoint, ILogger<LegacyMcPingService>? logger = null, int timeout = 10000)
    {
        return new LegacyMcPingService(endpoint, logger, timeout);
    }

    public static IMcPingService CreateLegacyService(string ip, ILogger<LegacyMcPingService>? logger = null, int port = 25565, int timeout = 10000)
    {
        return new LegacyMcPingService(ip, logger, port, timeout);
    }
}

public static class McPingServiceExtensions
{
    public static IServiceCollection AddMcPingServices(this IServiceCollection services)
    {
        return services;
    }
}