using PCL_CE.Neo.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Platform.Windows;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlatformService>(sp => new WindowsPlatformService(
            sp.GetRequiredService<ILogger<WindowsPlatformService>>()));

        services.AddSingleton<IWindowService>(sp => new WindowsWindowService(
            sp.GetRequiredService<ILogger<WindowsWindowService>>()));

        services.AddSingleton<IJavaScanner>(sp => new WindowsJavaScanner(
            sp.GetRequiredService<ILogger<WindowsJavaScanner>>()));

        services.AddSingleton<IThemeService>(sp => new WindowsThemeService(
            sp.GetRequiredService<ILogger<WindowsThemeService>>()));

        services.AddSingleton<IAudioService>(sp => new WindowsAudioService(
            sp.GetRequiredService<ILogger<WindowsAudioService>>()));

        services.AddSingleton<IClipboardService>(sp => new WindowsClipboardService(
            sp.GetRequiredService<ILogger<WindowsClipboardService>>()));

        services.AddSingleton<IDialogService>(sp => new WindowsDialogService(
            sp.GetRequiredService<ILogger<WindowsDialogService>>()));

        services.AddSingleton<INotificationService>(sp => new WindowsNotificationService(
            sp.GetRequiredService<ILogger<WindowsNotificationService>>()));

        services.AddSingleton<IUIAccessProvider>(sp => new WindowsUIAccessProvider(
            sp.GetRequiredService<ILogger<WindowsUIAccessProvider>>()));

        services.AddSingleton<IAnimationService>(sp => new WindowsAnimationService(
            sp.GetRequiredService<ILogger<WindowsAnimationService>>()));

        return services;
    }
}
