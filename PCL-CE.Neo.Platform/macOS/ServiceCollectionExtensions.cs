using PCL.CE.Neo.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace PCL.CE.Neo.Platform.macOS;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMacOSPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlatformService, MacOSPlatformService>();
        services.AddSingleton<IWindowService, MacOSWindowService>();
        services.AddSingleton<IJavaScanner, MacOSJavaScanner>();
        services.AddSingleton<IClipboardService, MacOSClipboardService>();
        services.AddSingleton<IDialogService, MacOSDialogService>();
        services.AddSingleton<IThemeService, MacOSThemeService>();
        return services;
    }
}
