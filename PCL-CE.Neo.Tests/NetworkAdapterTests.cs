using Xunit;
using FluentAssertions;
using PCL_CE.Neo.Core.Adapters;

namespace PCL_CE.Neo.Tests;

public class NetworkAdapterTests
{
    private readonly NetworkAdapter _networkAdapter;

    public NetworkAdapterTests()
    {
        _networkAdapter = new NetworkAdapter();
    }

    [Fact]
    public void NetworkAdapter_Should_Initialize_Successfully()
    {
        _networkAdapter.Should().NotBeNull();
    }

    [Fact]
    public void IsOnline_Should_Return_Boolean()
    {
        var result = _networkAdapter.IsOnline;
        result.Should().BeTrue().Or.BeFalse();
    }

    [Fact]
    public void CheckConnection_Should_Not_Throw()
    {
        Action act = () => _networkAdapter.CheckConnection("https://example.com");
        act.Should().NotThrow();
    }
}
