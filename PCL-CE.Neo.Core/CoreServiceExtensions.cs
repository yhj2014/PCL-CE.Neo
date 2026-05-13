using Microsoft.Extensions.DependencyInjection;
using PCL_CE.Neo.Core.Abstractions;

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
        return services;
    }
}
