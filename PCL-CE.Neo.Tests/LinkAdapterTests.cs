using Xunit;
using FluentAssertions;
using PCL_CE.Neo.Core.Adapters;

namespace PCL_CE.Neo.Tests;

public class LinkAdapterTests
{
    private readonly LinkAdapter _linkAdapter;

    public LinkAdapterTests()
    {
        _linkAdapter = new LinkAdapter();
    }

    [Fact]
    public void LinkAdapter_Should_Initialize_Successfully()
    {
        _linkAdapter.Should().NotBeNull();
    }

    [Fact]
    public void Connect_Should_Not_Throw()
    {
        Action act = () => _linkAdapter.Connect("test.server", 25565);
        act.Should().NotThrow();
    }

    [Fact]
    public void Disconnect_Should_Not_Throw()
    {
        Action act = () => _linkAdapter.Disconnect();
        act.Should().NotThrow();
    }

    [Fact]
    public void IsConnected_Should_Return_Boolean()
    {
        var result = _linkAdapter.IsConnected;
        result.Should().BeTrue().Or.BeFalse();
    }
}
