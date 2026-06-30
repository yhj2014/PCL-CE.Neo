using PCL_CE.Neo.Core.Link.McPing.Model;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Link.McPing;

public static class McPingServiceFactory
{
    public static async Task<McPingResult?> PingAsync(string host, int port = 25565, int timeout = 10000, CancellationToken cancellationToken = default)
    {
        try
        {
            using var modernService = new McPingService(host, port, timeout);
            var result = await modernService.PingAsync(cancellationToken);
            if (result != null)
                return result;
        }
        catch (Exception)
        {
        }

        try
        {
            using var legacyService = new LegacyMcPingService(host, port, timeout);
            return await legacyService.PingAsync(cancellationToken);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static IMcPingService CreateModern(string host, int port = 25565, int timeout = 10000)
    {
        return new McPingService(host, port, timeout);
    }

    public static IMcPingService CreateModern(IPEndPoint endpoint, int timeout = 10000)
    {
        return new McPingService(endpoint, timeout);
    }

    public static IMcPingService CreateLegacy(string host, int port = 25565, int timeout = 10000)
    {
        return new LegacyMcPingService(host, port, timeout);
    }

    public static IMcPingService CreateLegacy(IPEndPoint endpoint, int timeout = 10000)
    {
        return new LegacyMcPingService(endpoint, timeout);
    }
}