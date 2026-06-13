using Xunit;
using FluentAssertions;
using PCL_CE.Neo.Core.Adapters;

namespace PCL_CE.Neo.Tests;

public class StateAdapterTests
{
    private readonly StateAdapter _stateAdapter;

    public StateAdapterTests()
    {
        _stateAdapter = new StateAdapter();
    }

    [Fact]
    public void StateAdapter_Should_Initialize_Successfully()
    {
        _stateAdapter.Should().NotBeNull();
    }

    [Fact]
    public void GetState_Should_Not_Throw()
    {
        Action act = () => _stateAdapter.GetState<string>("test_key");
        act.Should().NotThrow();
    }

    [Fact]
    public void SetState_Should_Not_Throw()
    {
        Action act = () => _stateAdapter.SetState("test_key", "test_value");
        act.Should().NotThrow();
    }

    [Fact]
    public void ClearState_Should_Not_Throw()
    {
        _stateAdapter.SetState("test_key", "test_value");
        Action act = () => _stateAdapter.ClearState("test_key");
        act.Should().NotThrow();
    }
}
