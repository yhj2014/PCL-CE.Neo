using Microsoft.Extensions.DependencyInjection;

namespace PCL_CE.Neo.Core;

public static class ServiceLocator
{
    private static IServiceProvider? _serviceProvider;
    private static readonly object _lock = new();

    public static IServiceProvider Services
    {
        get
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("ServiceLocator 尚未初始化，请先调用 Initialize()");
            }
            return _serviceProvider;
        }
    }

    public static void Initialize(IServiceProvider serviceProvider)
    {
        lock (_lock)
        {
            _serviceProvider = serviceProvider;
        }
    }

    public static T GetService<T>() where T : class
    {
        return Services.GetRequiredService<T>();
    }

    public static T? GetServiceOrNull<T>() where T : class
    {
        return Services.GetService<T>();
    }

    public static IEnumerable<T> GetServices<T>() where T : class
    {
        return Services.GetServices<T>();
    }

    public static object GetService(Type serviceType)
    {
        return Services.GetRequiredService(serviceType);
    }

    public static T GetRequiredService<T>(Type serviceType) where T : class
    {
        return (T)Services.GetRequiredService(serviceType);
    }
}
