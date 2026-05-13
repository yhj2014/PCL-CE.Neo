using Microsoft.Extensions.DependencyInjection;
using PCL_CE.Neo.Core.Abstractions;
using System.Reflection;

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
        var serviceCollection = Services.AddPCLCoreServices();
        platform = platform.ToLowerInvariant();
        
        return platform switch
        {
            "windows" => AddWindowsPlatformServices(serviceCollection),
            "macos" or "darwin" => AddMacOSPlatformServices(serviceCollection),
            "linux" => AddLinuxPlatformServices(serviceCollection),
            _ => throw new NotSupportedException($"不支持的平台: {platform}")
        };
    }

    private static IServiceCollection AddWindowsPlatformServices(IServiceCollection services)
    {
        try
        {
            var assembly = Assembly.Load("PCL-CE.Neo.Platform.Windows");
            var extensionType = assembly.GetType("PCL_CE.Neo.Platform.Windows.ServiceCollectionExtensions");
            extensionType?.GetMethod("AddWindowsPlatformServices", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { services });
        }
        catch
        {
        }
        return services;
    }

    private static IServiceCollection AddMacOSPlatformServices(IServiceCollection services)
    {
        try
        {
            var assembly = Assembly.Load("PCL-CE.Neo.Platform.macOS");
            var extensionType = assembly.GetType("PCL_CE.Neo.Platform.macOS.ServiceCollectionExtensions");
            extensionType.GetMethod("AddMacOSPlatformServices", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { services });
        }
        catch
        {
        }
        return services;
    }

    private static IServiceCollection AddLinuxPlatformServices(IServiceCollection services)
    {
        try
        {
            var assembly = Assembly.Load("PCL-CE.Neo.Platform.Linux");
            var extensionType = assembly.GetType("PCL_CE.Neo.Platform.Linux.ServiceCollectionExtensions");
            extensionType.GetMethod("AddLinuxPlatformServices", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { services });
        }
        catch
        {
        }
        return services;
    }

    public static IServiceCollection AddLogging(Action<ILoggerAdapter> configure)
    {
        var logger = Services.BuildServiceProvider().GetRequiredService<ILoggerAdapter>();
        configure(logger);
        return Services;
    }
}
