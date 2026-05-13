using Microsoft.Extensions.Logging.Abstractions;
using PCL_CE.Neo.Core;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL.CE.Neo.Tests;

public class ServiceLocatorTests
{
    [Fact]
    public void ServiceLocator_Initialize_SetsProvider()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddSingleton<ILoggerAdapter, Adapters.LoggerAdapter>();
        var provider = services.BuildServiceProvider();

        ServiceLocator.Initialize(provider);

        var logger = ServiceLocator.GetService<ILoggerAdapter>();
        Assert.NotNull(logger);
    }

    [Fact]
    public void ServiceLocator_GetService_ReturnsService()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddSingleton<ILoggerAdapter, Adapters.LoggerAdapter>();
        var provider = services.BuildServiceProvider();
        ServiceLocator.Initialize(provider);

        var logger1 = ServiceLocator.GetService<ILoggerAdapter>();
        var logger2 = ServiceLocator.GetService<ILoggerAdapter>();

        Assert.Same(logger1, logger2);
    }

    [Fact]
    public void ServiceLocator_GetServices_ReturnsAll()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddSingleton<ILoggerAdapter, Adapters.LoggerAdapter>();
        var provider = services.BuildServiceProvider();
        ServiceLocator.Initialize(provider);

        var loggers = ServiceLocator.GetServices<ILoggerAdapter>();
        Assert.Single(loggers);
    }
}
