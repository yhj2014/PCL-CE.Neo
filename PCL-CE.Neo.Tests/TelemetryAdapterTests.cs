using Xunit;
using FluentAssertions;
using PCL_CE.Neo.Core.Adapters;

namespace PCL_CE.Neo.Tests;

public class TelemetryAdapterTests
{
    private readonly TelemetryAdapter _telemetryAdapter;

    public TelemetryAdapterTests()
    {
        _telemetryAdapter = new TelemetryAdapter();
    }

    [Fact]
    public void TelemetryAdapter_Should_Initialize_Successfully()
    {
        _telemetryAdapter.Should().NotBeNull();
    }

    [Fact]
    public void TrackEvent_Should_Not_Throw()
    {
        Action act = () => _telemetryAdapter.TrackEvent("test_event");
        act.Should().NotThrow();
    }

    [Fact]
    public void TrackException_Should_Not_Throw()
    {
        var exception = new Exception("Test exception");
        Action act = () => _telemetryAdapter.TrackException(exception);
        act.Should().NotThrow();
    }

    [Fact]
    public void TrackMetric_Should_Not_Throw()
    {
        Action act = () => _telemetryAdapter.TrackMetric("test_metric", 42);
        act.Should().NotThrow();
    }
}
