using Microsoft.Extensions.DependencyInjection;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core;

public static class ServiceBuilder
{
    private static IServiceCollection? _services;
    private static IServiceProvider? _provider;

    public static IServiceCollection Services
    {
        get
        {
            _services ??= new ServiceCollection();
            return _services;
        }
    }

    public static IServiceProvider Build()
    {
        _provider = Services.BuildServiceProvider();
        ServiceLocator.Initialize(_provider);
        return _provider;
    }

    public static IServiceCollection AddPlatformServices(string platform)
    {
        return platform.ToLowerInvariant() switch
        {
            "windows" => AddWindowsServices(),
            "macos" or "darwin" => AddMacOSServices(),
            "linux" => AddLinuxServices(),
            _ => throw new NotSupportedException($"不支持的平台: {platform}")
        };
    }

    private static IServiceCollection AddWindowsServices()
    {
        Services.AddPCLCoreServices();
        return Services;
    }

    private static IServiceCollection AddMacOSServices()
    {
        Services.AddPCLCoreServices();
        return Services;
    }

    private static IServiceCollection AddLinuxServices()
    {
        Services.AddPCLCoreServices();
        return Services;
    }

    public static IServiceCollection AddLogging(Action<ILoggerAdapter> configure)
    {
        var logger = Services.BuildServiceProvider().GetRequiredService<ILoggerAdapter>();
        configure(logger);
        return Services;
    }
}
