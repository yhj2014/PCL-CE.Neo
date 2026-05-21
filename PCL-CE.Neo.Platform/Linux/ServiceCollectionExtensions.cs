using PCL_CE.Neo.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace PCL_CE.Neo.Platform.Linux;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLinuxPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlatformService, LinuxPlatformService>();
        services.AddSingleton<IJavaScanner, LinuxJavaScanner>();
        return services;
    }
}
