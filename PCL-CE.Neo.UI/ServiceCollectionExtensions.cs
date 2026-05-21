using PCL_CE.Neo.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace PCL_CE.Neo.UI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUIServices(this IServiceCollection services)
    {
        services.AddSingleton<IAnimationService, Services.AnimationService>();
        services.AddSingleton<IAudioService, Services.AudioService>();
        services.AddSingleton<IClipboardService, Services.ClipboardService>();
        services.AddSingleton<IDialogService, Services.DialogService>();
        services.AddSingleton<INotificationService, Services.NotificationService>();
        services.AddSingleton<IThemeService, Services.ThemeService>();
        services.AddSingleton<IUIAccessProvider, Services.UIAccessProvider>();
        services.AddSingleton<IWindowService, Services.WindowService>();
        return services;
    }
}
