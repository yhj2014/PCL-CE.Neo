using System;
using System.Net;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Link.McPing;

public interface IMcPingServiceFactory
{
    IMcPingService Create(string host, int port = 25565);
    IMcPingService Create(IPEndPoint endpoint);
    bool IsLegacyProtocol(int protocolVersion);
    int GetProtocolVersion(string versionName);
}

public class McPingServiceFactory : IMcPingServiceFactory
{
    private readonly ILogger<McPingServiceFactory> _logger;

    public McPingServiceFactory() : this(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<McPingServiceFactory>.Instance)
    {
    }

    public McPingServiceFactory(ILogger<McPingServiceFactory> logger)
    {
        _logger = logger;
    }

    public IMcPingService Create(string host, int port = 25565)
    {
        var endpoint = ResolveEndpoint(host, port);
        return Create(endpoint);
    }

    public IMcPingService Create(IPEndPoint endpoint)
    {
        _logger.LogDebug("Creating McPingService for {Endpoint}", endpoint);
        return new McPingService(endpoint, _logger as ILogger<McPingService> 
            ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<McPingService>.Instance);
    }

    public bool IsLegacyProtocol(int protocolVersion)
    {
        return protocolVersion < 315;
    }

    public int GetProtocolVersion(string versionName)
    {
        return versionName.ToLowerInvariant() switch
        {
            "1.7.10" => 5,
            "1.8" => 47,
            "1.9" => 107,
            "1.9.1" => 108,
            "1.9.2" => 109,
            "1.9.3" => 110,
            "1.9.4" => 111,
            "1.10" => 210,
            "1.10.1" or "1.10.2" => 212,
            "1.11" => 315,
            "1.11.1" or "1.11.2" => 316,
            "1.12" => 335,
            "1.12.1" => 338,
            "1.12.2" => 340,
            "1.13" => 393,
            "1.13.1" => 401,
            "1.13.2" => 404,
            "1.14" => 477,
            "1.14.1" => 480,
            "1.14.2" => 485,
            "1.14.3" => 490,
            "1.14.4" => 498,
            "1.15" => 573,
            "1.15.1" => 575,
            "1.15.2" => 578,
            "1.16" => 735,
            "1.16.1" => 736,
            "1.16.2" => 751,
            "1.16.3" => 753,
            "1.16.4" => 754,
            "1.16.5" => 755,
            "1.17" => 755,
            "1.17.1" => 756,
            "1.18" or "1.18.1" => 757,
            "1.18.2" => 758,
            "1.19" => 759,
            "1.19.1" => 760,
            "1.19.2" => 761,
            "1.19.3" => 762,
            "1.19.4" => 763,
            "1.20" or "1.20.1" => 763,
            "1.20.2" => 764,
            "1.20.3" => 765,
            "1.20.4" => 766,
            "1.20.5" or "1.20.6" => 767,
            "1.21" => 768,
            "1.21.1" => 769,
            "1.21.2" => 770,
            "1.21.3" => 771,
            _ => 0
        };
    }

    private static IPEndPoint ResolveEndpoint(string host, int port)
    {
        var addresses = Dns.GetHostAddresses(host);
        if (addresses.Length == 0)
            throw new ArgumentException($"Unable to resolve host: {host}");
            
        return new IPEndPoint(addresses[0], port);
    }
}
