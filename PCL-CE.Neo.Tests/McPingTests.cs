using System.Net;
using PCL_CE.Neo.Core.Link.McPing;
using Xunit;

namespace PCL_CE.Neo.Tests;

public class McPingTests
{
    [Fact]
    public void McPingService_CanBeCreated_WithIPEndPoint()
    {
        var endpoint = new IPEndPoint(IPAddress.Loopback, 25565);
        var service = new McPingService(endpoint);
        
        Assert.Equal(endpoint, service.Endpoint);
        Assert.Equal("127.0.0.1", service.Host);
        Assert.Equal(10000, service.Timeout);
    }

    [Fact]
    public void McPingService_CanBeCreated_WithHostAndPort()
    {
        var service = new McPingService("localhost", 25565);
        
        Assert.Equal(25565, service.Endpoint.Port);
        Assert.Equal("localhost", service.Host);
    }

    [Fact]
    public void LegacyMcPingService_CanBeCreated()
    {
        var endpoint = new IPEndPoint(IPAddress.Loopback, 25565);
        var service = new LegacyMcPingService(endpoint);
        
        Assert.Equal(endpoint, service.Endpoint);
        Assert.Equal("127.0.0.1", service.Host);
    }

    [Fact]
    public void McPingModels_CanSerializeAndDeserialize()
    {
        var version = new McPingVersionResult("1.20.1", 763);
        var players = new McPingPlayerResult(20, 5, null);
        var result = new McPingResult(version, players, "A Minecraft Server", null, 50, null, null);
        
        Assert.Equal("1.20.1", result.Version.Name);
        Assert.Equal(763, result.Version.Protocol);
        Assert.Equal(20, result.Players.Max);
        Assert.Equal(5, result.Players.Online);
        Assert.Equal("A Minecraft Server", result.Description);
        Assert.Equal(50, result.Latency);
    }

    [Fact]
    public void McPingPlayerSample_CanBeCreated()
    {
        var sample = new McPingPlayerSampleResult("Player1", "uuid-123");
        
        Assert.Equal("Player1", sample.Name);
        Assert.Equal("uuid-123", sample.Id);
    }

    [Fact]
    public void McPingModInfo_CanBeCreated()
    {
        var mod = new McPingModInfoModResult("modid", "1.0.0");
        var modInfo = new McPingModInfoResult("FML", new List<McPingModInfoModResult> { mod });
        
        Assert.Equal("FML", modInfo.Type);
        Assert.Single(modInfo.ModList);
        Assert.Equal("modid", modInfo.ModList[0].Id);
    }
}
