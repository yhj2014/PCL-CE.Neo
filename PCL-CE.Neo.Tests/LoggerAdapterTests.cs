using Xunit;
using FluentAssertions;
using PCL_CE.Neo.Core.Adapters;

namespace PCL_CE.Neo.Tests;

public class LoggerAdapterTests
{
    private readonly LoggerAdapter _logger;

    public LoggerAdapterTests()
    {
        _logger = new LoggerAdapter(new TestLogger<LoggerAdapter>());
    }

    [Fact]
    public void LoggerAdapter_Should_Initialize_Successfully()
    {
        _logger.Should().NotBeNull();
    }

    [Fact]
    public void LoggerAdapter_Should_Handle_InfoLogs()
    {
        Action act = () => _logger.Info("Test message");
        act.Should().NotThrow();
    }

    [Fact]
    public void LoggerAdapter_Should_Handle_WarningLogs()
    {
        Action act = () => _logger.Warn("Test warning");
        act.Should().NotThrow();
    }

    [Fact]
    public void LoggerAdapter_Should_Handle_ErrorLogs()
    {
        Action act = () => _logger.Error("Test error");
        act.Should().NotThrow();
    }

    [Fact]
    public void LoggerAdapter_Should_Handle_ExceptionLogs()
    {
        var exception = new Exception("Test exception");
        Action act = () => _logger.Error(exception, "Error message");
        act.Should().NotThrow();
    }

    [Fact]
    public void LoggerAdapter_Should_Handle_DebugLogs()
    {
        Action act = () => _logger.Debug("Test debug");
        act.Should().NotThrow();
    }
}
