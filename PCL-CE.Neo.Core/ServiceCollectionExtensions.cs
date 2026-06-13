using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Core.Adapters;
using PCL_CE.Neo.Core.Configuration;
using PCL_CE.Neo.Core.Database;
using PCL_CE.Neo.Core.IO;
using PCL_CE.Neo.Core.Lifecycle;
using PCL_CE.Neo.Core.Link;
using PCL_CE.Neo.Core.Minecraft;
using PCL_CE.Neo.Core.Network;
using TaskManagerImpl = PCL_CE.Neo.Core.TaskManager.TaskManager;

namespace PCL_CE.Neo.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // 添加日志服务（简化，不依赖完整 Logging 包）
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<IDatabaseService, DatabaseService>();
        services.AddSingleton<INetworkService, NetworkService>();
        services.AddSingleton<IDownloadService, DownloadService>();
        // 用工厂方法注册 LifecycleManager，避免 IServiceCollection 依赖
        services.AddSingleton<ILifecycleManager>(sp => new LifecycleManager(sp));
        services.AddSingleton<IJavaManager, JavaManager>();
        services.AddSingleton<IGameLauncher, GameLauncher>();
        services.AddSingleton<ILinkService, LinkService>();

        return services;
    }

    public static IServiceCollection AddCoreAdapters(this IServiceCollection services)
    {
        services.AddSingleton<IApplicationAdapter, ApplicationAdapter>();
        services.AddSingleton<IConfigAdapter, ConfigAdapter>();
        services.AddSingleton<IPathsAdapter, PathsAdapter>();
        services.AddSingleton<IDatabaseAdapter, DatabaseAdapter>();
        services.AddSingleton<INetworkAdapter, NetworkAdapter>();
        services.AddSingleton<IDownloadAdapter, DownloadAdapter>();
        services.AddSingleton<ITaskAdapter, TaskAdapter>();
        services.AddSingleton<IStateAdapter, StateAdapter>();
        services.AddSingleton<ILoggerAdapter, LoggerAdapter>();

        services.AddSingleton<IInstanceAdapter, InstanceAdapter>();
        services.AddSingleton<IMinecraftAdapter, MinecraftAdapter>();
        services.AddSingleton<IModAdapter, ModAdapter>();
        services.AddSingleton<IAuthAdapter, AuthAdapter>();
        services.AddSingleton<ILinkAdapter, LinkAdapter>();
        services.AddSingleton<ITelemetryAdapter, TelemetryAdapter>();
        services.AddSingleton<IResourceDownloadAdapter, ResourceDownloadAdapter>();

        return services;
    }

    public static IServiceProvider BuildCoreServices(this IServiceCollection services)
    {
        return services.BuildServiceProvider();
    }
}
