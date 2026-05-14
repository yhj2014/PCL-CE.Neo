using Microsoft.Extensions.DependencyInjection;

namespace PCL_CE.Neo.Core.Lifecycle;

public interface IService
{
    string Identifier { get; }
    string Name { get; }
    bool SupportAsync { get; }
    Task StartAsync();
    Task StopAsync();
}

public abstract class ServiceBase : IService
{
    public abstract string Identifier { get; }
    public abstract string Name { get; }
    public virtual bool SupportAsync => true;
    protected IServiceProvider Services { get; }

    protected ServiceBase(IServiceProvider services)
    {
        Services = services;
    }

    public virtual Task StartAsync() => Task.CompletedTask;
    public virtual Task StopAsync() => Task.CompletedTask;
}

public static class ServiceExtensions
{
    public static IServiceCollection AddLifecycleServices(this IServiceCollection services)
    {
        services.AddSingleton<ILifecycleManager, LifecycleManager>();
        return services;
    }
}

public interface ILifecycleManager
{
    IServiceProvider Services { get; }
    Task StartAsync();
    Task StopAsync();
    T GetService<T>() where T : IService;
    bool IsServiceRunning<T>() where T : IService;
    event Action<string>? ServiceStarted;
    event Action<string>? ServiceStopped;
    event Action<string, Exception>? ServiceFailed;
}

public class LifecycleManager : ILifecycleManager, IDisposable
{
    private readonly IServiceCollection _services;
    private readonly Dictionary<string, IService> _runningServices = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IServiceProvider? _serviceProvider;

    public IServiceProvider Services => _serviceProvider ?? throw new InvalidOperationException("Not started");

    public LifecycleManager(IServiceCollection services)
    {
        _services = services;
    }

    public event Action<string>? ServiceStarted;
    public event Action<string>? ServiceStopped;
    public event Action<string, Exception>? ServiceFailed;

    public async Task StartAsync()
    {
        _serviceProvider = _services.BuildServiceProvider();
        
        var services = _serviceProvider.GetServices<IService>();
        foreach (var service in services)
        {
            try
            {
                await _lock.WaitAsync();
                try
                {
                    await service.StartAsync();
                    _runningServices[service.Identifier] = service;
                    ServiceStarted?.Invoke(service.Identifier);
                }
                finally
                {
                    _lock.Release();
                }
            }
            catch (Exception ex)
            {
                ServiceFailed?.Invoke(service.Identifier, ex);
                throw;
            }
        }
    }

    public async Task StopAsync()
    {
        var services = _runningServices.Values.ToList();
        services.Reverse();
        
        foreach (var service in services)
        {
            try
            {
                await _lock.WaitAsync();
                try
                {
                    await service.StopAsync();
                    ServiceStopped?.Invoke(service.Identifier);
                }
                finally
                {
                    _lock.Release();
                }
            }
            catch (Exception ex)
            {
                ServiceFailed?.Invoke(service.Identifier, ex);
            }
        }
        
        _runningServices.Clear();
    }

    public T GetService<T>() where T : IService
    {
        if (_runningServices.TryGetValue(typeof(T).Name, out var service))
        {
            return (T)service;
        }
        throw new InvalidOperationException($"Service {typeof(T).Name} is not running");
    }

    public bool IsServiceRunning<T>() where T : IService
    {
        return _runningServices.ContainsKey(typeof(T).Name);
    }

    private bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _lock.Dispose();
    }
}
