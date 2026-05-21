using PCL_CE.Neo.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace PCL_CE.Neo.Platform.macOS;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMacOSPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlatformService, MacOSPlatformService>();
        services.AddSingleton<IJavaScanner, MacOSJavaScanner>();
        return services;
    }
}
