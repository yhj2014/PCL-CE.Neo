using Microsoft.Extensions.DependencyInjection;
using PCL_CE.Neo.Core;
using Xunit;

namespace PCL_CE.Neo.Tests;

public class ServiceExtensionsTests
{
    [Fact]
    public void AddCoreServices_RegistersServices()
    {
        var services = new ServiceCollection();
        services.AddCoreServices();
        
        var provider = services.BuildServiceProvider();
        
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Configuration.IConfigService>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Database.IDatabaseService>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Network.INetworkService>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.IO.IDownloadService>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Lifecycle.ILifecycleManager>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.TaskManager.ITaskManager>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Minecraft.IJavaManager>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Minecraft.IGameLauncher>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Link.ILinkService>());
    }

    [Fact]
    public void AddCoreAdapters_RegistersAdapters()
    {
        var services = new ServiceCollection();
        services.AddCoreAdapters();
        
        var provider = services.BuildServiceProvider();
        
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Abstractions.IApplicationAdapter>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Abstractions.IConfigAdapter>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Abstractions.IPathsAdapter>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Abstractions.IDatabaseAdapter>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Abstractions.INetworkAdapter>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Abstractions.IDownloadAdapter>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Abstractions.ITaskAdapter>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Abstractions.IStateAdapter>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Abstractions.ILoggerAdapter>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Abstractions.IInstanceAdapter>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Abstractions.IMinecraftAdapter>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Abstractions.IModAdapter>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Abstractions.IAuthAdapter>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Abstractions.ILinkAdapter>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Abstractions.ITelemetryAdapter>());
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Abstractions.IResourceDownloadAdapter>());
    }

    [Fact]
    public void BuildCoreServices_CreatesWorkingProvider()
    {
        var services = new ServiceCollection();
        services.AddCoreServices();
        services.AddCoreAdapters();
        
        var provider = services.BuildCoreServices();
        
        Assert.NotNull(provider);
        Assert.NotNull(provider.GetService<PCL_CE.Neo.Core.Abstractions.IConfigAdapter>());
    }
}
