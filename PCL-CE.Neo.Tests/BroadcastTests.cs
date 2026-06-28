using System;
using System.Net;
using PCL_CE.Neo.Core.Link.Broadcast;
using Xunit;

namespace PCL_CE.Neo.Tests;

public class BroadcastTests
{
    [Fact]
    public void BroadcastRecord_CanBeCreated()
    {
        var endpoint = new IPEndPoint(IPAddress.Loopback, 25565);
        var foundAt = DateTime.Now;
        var record = new BroadcastRecord("Test Server", endpoint, foundAt);
        
        Assert.Equal("Test Server", record.Description);
        Assert.Equal(endpoint, record.Address);
        Assert.Equal(foundAt, record.FoundAt);
    }

    [Fact]
    public void BroadcastListener_CanBeCreated()
    {
        var listener = new BroadcastListener();
        
        Assert.NotNull(listener);
    }

    [Fact]
    public void BroadcastListener_CanBeCreatedWithLocalOnly()
    {
        var listener = new BroadcastListener(receiveLocalOnly: false);
        
        Assert.NotNull(listener);
    }

    [Fact]
    public void BroadcastLocal_CanBeCreated()
    {
        using var broadcaster = new BroadcastLocal("Test Server", 25565);
        
        Assert.NotNull(broadcaster);
    }

    [Fact]
    public void BroadcastLocal_CanStartAndStop()
    {
        using var broadcaster = new BroadcastLocal("Test Server", 25565);
        
        broadcaster.Start();
        Assert.True(true);
        
        broadcaster.Stop();
        Assert.True(true);
    }
}
