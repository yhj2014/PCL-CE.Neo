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
    private readonly IServiceCollection? _services;
    private readonly Dictionary<string, IService> _runningServices = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IServiceProvider? _serviceProvider;

    public IServiceProvider Services => _serviceProvider ?? throw new InvalidOperationException("Not started");

    public LifecycleManager() : this(new ServiceCollection())
    {
    }

    public LifecycleManager(IServiceCollection services)
    {
        _services = services;
    }

    public LifecycleManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public event Action<string>? ServiceStarted;
    public event Action<string>? ServiceStopped;
    public event Action<string, Exception>? ServiceFailed;

    public async Task StartAsync()
    {
        if (_services != null && _serviceProvider == null)
        {
            _serviceProvider = _services.BuildServiceProvider();
        }

        var servicesToStart = new List<IService>();
        if (_services != null)
        {
            foreach (var descriptor in _services)
            {
                if (typeof(IService).IsAssignableFrom(descriptor.ServiceType))
                {
                    var svc = _serviceProvider!.GetService(descriptor.ServiceType) as IService;
                    if (svc != null) servicesToStart.Add(svc);
                }
                else if (descriptor.ImplementationType != null && typeof(IService).IsAssignableFrom(descriptor.ImplementationType))
                {
                    var svc = _serviceProvider!.GetService(descriptor.ImplementationType) as IService;
                    if (svc != null) servicesToStart.Add(svc);
                }
            }
        }

        // 回退：如果上面没找到，直接尝试获取所有 IService
        if (servicesToStart.Count == 0 && _serviceProvider != null)
        {
            servicesToStart.AddRange(_serviceProvider.GetServices<IService>());
        }

        foreach (var service in servicesToStart)
        {
            try
            {
                await _lock.WaitAsync();
                try
                {
                    await service.StartAsync();
                    _runningServices[service.Identifier] = service;
                    // 也按类型名称注册，便于 IsServiceRunning<T> 查询
                    var serviceType = service.GetType();
                    _runningServices[serviceType.Name] = service;
                    // 以及接口类型名称
                    foreach (var iface in serviceType.GetInterfaces())
                    {
                        if (typeof(IService).IsAssignableFrom(iface))
                        {
                            _runningServices[iface.Name] = service;
                        }
                    }
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
