using System.Net;

namespace PCL_CE.Neo.Core.Link.McPing;

public static class McPingServiceFactory
{
    public static IMcPingService CreateService(IPEndPoint endpoint, int timeout = 10000)
    {
        return new McPingService(endpoint, timeout);
    }

    public static IMcPingService CreateService(string ip, int port = 25565, int timeout = 10000)
    {
        return new McPingService(ip, port, timeout);
    }

    public static IMcPingService CreateService(string host, string? ip, int port = 25565)
    {
        return CreateService(host, ip, port, 10000);
    }

    public static IMcPingService CreateService(string host, string? ip, int port, int timeout)
    {
        if (IPAddress.TryParse(ip, out var ipAddress))
        {
            var endpoint = new IPEndPoint(ipAddress, port);
            return new McPingService(host, endpoint, timeout);
        }
        return new McPingService(host, port, timeout);
    }

    public static IMcPingService CreateLegacyService(IPEndPoint endpoint, int timeout = 10000)
    {
        return new LegacyMcPingService(endpoint, timeout);
    }

    public static IMcPingService CreateLegacyService(string ip, int port = 25565, int timeout = 10000)
    {
        return new LegacyMcPingService(ip, port, timeout);
    }
}