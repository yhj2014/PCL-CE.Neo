using PCL_CE.Neo.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace PCL_CE.Neo.Platform.Linux;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLinuxPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlatformService, LinuxPlatformService>();
        services.AddSingleton<IWindowService, LinuxWindowService>();
        services.AddSingleton<IJavaScanner, LinuxJavaScanner>();
        services.AddSingleton<IClipboardService, LinuxClipboardService>();
        services.AddSingleton<IDialogService, LinuxDialogService>();
        services.AddSingleton<IThemeService, LinuxThemeService>();
        services.AddSingleton<IAudioService, LinuxAudioService>();
        services.AddSingleton<INotificationService, LinuxNotificationService>();
        services.AddSingleton<IUIAccessProvider, LinuxUIAccessProvider>();
        return services;
    }
}
