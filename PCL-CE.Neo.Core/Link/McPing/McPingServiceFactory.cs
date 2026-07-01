using System.Net;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Link.McPing;

public static class McPingServiceFactory
{
    public static IMcPingService Create(string ip, int port = 25565, int timeout = 10000)
    {
        return new McPingService(ip, port, timeout);
    }

    public static IMcPingService Create(IPEndPoint endpoint, int timeout = 10000)
    {
        return new McPingService(endpoint, timeout);
    }

    public static IMcPingService Create(ILogger<McPingService> logger, string ip, int port = 25565, int timeout = 10000)
    {
        return new McPingService(logger, ip, port, timeout);
    }

    public static IMcPingService Create(ILogger<McPingService> logger, IPEndPoint endpoint, int timeout = 10000)
    {
        return new McPingService(logger, endpoint, timeout);
    }

    public static IMcPingService CreateLegacy(string ip, int port = 25565, int timeout = 10000)
    {
        return new LegacyMcPingService(ip, port, timeout);
    }

    public static IMcPingService CreateLegacy(IPEndPoint endpoint, int timeout = 10000)
    {
        return new LegacyMcPingService(endpoint, timeout);
    }

    public static IMcPingService CreateLegacy(ILogger<LegacyMcPingService> logger, string ip, int port = 25565, int timeout = 10000)
    {
        return new LegacyMcPingService(logger, ip, port, timeout);
    }

    public static IMcPingService CreateLegacy(ILogger<LegacyMcPingService> logger, IPEndPoint endpoint, int timeout = 10000)
    {
        return new LegacyMcPingService(logger, endpoint, timeout);
    }
}