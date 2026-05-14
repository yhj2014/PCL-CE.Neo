using PCL_CE.Neo.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace PCL_CE.Neo.Platform.Windows;

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
        services.AddSingleton<IAudioService, WindowsAudioService>();
        services.AddSingleton<INotificationService, WindowsNotificationService>();
        services.AddSingleton<IUIAccessProvider, WindowsUIAccessProvider>();
        services.AddSingleton<IAnimationService, WindowsAnimationService>();
        return services;
    }
}
