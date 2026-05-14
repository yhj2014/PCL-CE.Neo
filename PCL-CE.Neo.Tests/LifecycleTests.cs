using Microsoft.Extensions.DependencyInjection;
using PCL_CE.Neo.Core.Lifecycle;
using Xunit;

namespace PCL_CE.Neo.Tests;

public class LifecycleTests
{
    [Fact]
    public async Task LifecycleManager_StartsServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        var manager = new LifecycleManager(services);

        await manager.StartAsync();

        Assert.True(manager.IsServiceRunning<ITestService>());

        await manager.StopAsync();
    }

    [Fact]
    public async Task LifecycleManager_StopsServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        var manager = new LifecycleManager(services);

        await manager.StartAsync();
        Assert.True(manager.IsServiceRunning<ITestService>());

        await manager.StopAsync();
        Assert.False(manager.IsServiceRunning<ITestService>());
    }

    [Fact]
    public async Task LifecycleManager_FiresEvents()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        var manager = new LifecycleManager(services);

        var startedEventFired = false;
        var stoppedEventFired = false;

        manager.ServiceStarted += _ => startedEventFired = true;
        manager.ServiceStopped += _ => stoppedEventFired = true;

        await manager.StartAsync();
        Assert.True(startedEventFired);

        await manager.StopAsync();
        Assert.True(stoppedEventFired);
    }

    public interface ITestService : IService { }
    public class TestService : ServiceBase, ITestService
    {
        public override string Identifier => "test";
        public override string Name => "Test Service";

        public TestService(IServiceProvider services) : base(services) { }

        public override Task StartAsync()
        {
            return Task.CompletedTask;
        }
    }
}
