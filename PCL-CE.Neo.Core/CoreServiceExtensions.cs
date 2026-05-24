using Microsoft.Extensions.DependencyInjection;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Core.Adapters;

namespace PCL_CE.Neo.Core;

public static class CoreServiceExtensions
{
    public static IServiceCollection AddPCLCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<ILifecycleBridge, LifecycleBridge>();
        services.AddSingleton<IApplicationAdapter, ApplicationAdapter>();
        services.AddSingleton<IConfigAdapter, ConfigAdapter>();
        services.AddSingleton<IPathsAdapter, PathsAdapter>();
        services.AddSingleton<IStateAdapter, StateAdapter>();
        services.AddSingleton<IMinecraftAdapter, MinecraftAdapter>();
        services.AddSingleton<IDownloadAdapter, DownloadAdapter>();
        services.AddSingleton<INetworkAdapter, NetworkAdapter>();
        services.AddSingleton<IDatabaseAdapter, DatabaseAdapter>();
        services.AddSingleton<ILoggerAdapter, LoggerAdapter>();
        services.AddSingleton<ITelemetryAdapter, TelemetryAdapter>();
        services.AddSingleton<IAuthAdapter, AuthAdapter>();
        services.AddSingleton<ITaskAdapter, TaskAdapter>();
        services.AddSingleton<IInstanceAdapter, InstanceAdapter>();
        services.AddSingleton<IModAdapter, ModAdapter>();
        services.AddSingleton<IResourceDownloadAdapter, ResourceDownloadAdapter>();
        services.AddSingleton<ILinkAdapter, LinkAdapter>();
        services.AddSingleton<IEasyTierAdapter, EasyTierAdapter>();
        return services;
    }
}
