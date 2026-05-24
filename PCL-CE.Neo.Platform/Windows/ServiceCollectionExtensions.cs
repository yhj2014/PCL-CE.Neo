using PCL_CE.Neo.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace PCL_CE.Neo.Platform.Windows;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlatformService, WindowsPlatformService>();
        services.AddSingleton<IJavaScanner, WindowsJavaScanner>();
        return services;
    }
}
