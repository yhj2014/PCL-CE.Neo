using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Core;
using PclLogLevel = PCL_CE.Neo.Core.Abstractions.LogLevel;

namespace PCL_CE.Neo.Tests;

public class ServiceLocatorTests
{
    [Fact]
    public void ServiceLocator_Initialize_SetsProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerAdapter, TestLoggerAdapter>();
        var provider = services.BuildServiceProvider();

        ServiceLocator.Initialize(provider);

        var logger = ServiceLocator.GetService<ILoggerAdapter>();
        Assert.NotNull(logger);
    }

    [Fact]
    public void ServiceLocator_GetService_ReturnsService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerAdapter, TestLoggerAdapter>();
        var provider = services.BuildServiceProvider();
        ServiceLocator.Initialize(provider);

        var logger1 = ServiceLocator.GetService<ILoggerAdapter>();
        var logger2 = ServiceLocator.GetService<ILoggerAdapter>();

        Assert.Same(logger1, logger2);
    }

    [Fact]
    public void ServiceLocator_GetServices_ReturnsAll()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerAdapter, TestLoggerAdapter>();
        var provider = services.BuildServiceProvider();
        ServiceLocator.Initialize(provider);

        var loggers = ServiceLocator.GetServices<ILoggerAdapter>();
        Assert.Single(loggers);
    }

    private class TestLoggerAdapter : ILoggerAdapter
    {
        public void Trace(string message, params object[] args) { }
        public void Debug(string message, params object[] args) { }
        public void Information(string message, params object[] args) { }
        public void Warning(string message, params object[] args) { }
        public void Warning(Exception? ex, string message, params object[] args) { }
        public void Error(string message, params object[] args) { }
        public void Error(Exception? ex, string message, params object[] args) { }
        public void Fatal(string message, params object[] args) { }
        public void Fatal(Exception? ex, string message, params object[] args) { }
        public IDisposable? BeginScope(string scope) => NullLogger.Instance.BeginScope(scope);
        public bool IsEnabled(PclLogLevel level) => true;
        public void SetLevel(PclLogLevel level) { }
    }
}
