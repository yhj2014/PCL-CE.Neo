using PCL.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace PCL.Platform.Windows;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlatformService, WindowsPlatformService>();
        services.AddSingleton<IWindowService, WindowsWindowService>();
        services.AddSingleton<IJavaScanner, WindowsJavaScanner>();
        services.AddSingleton<IClipboardService, WindowsClipboardService>();
        services.AddSingleton<IDialogService, WindowsDialogService>();
        services.AddSingleton<IThemeService, WindowsThemeService>();
        return services;
    }
}
