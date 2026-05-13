using PCL_CE.Neo.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace PCL_CE.Neo.Platform.macOS;

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
        services.AddSingleton<IAudioService, MacOSAudioService>();
        services.AddSingleton<INotificationService, MacOSNotificationService>();
        services.AddSingleton<IUIAccessProvider, MacOSUIAccessProvider>();
        return services;
    }
}
