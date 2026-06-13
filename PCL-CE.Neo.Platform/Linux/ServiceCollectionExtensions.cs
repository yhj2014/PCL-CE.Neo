using PCL_CE.Neo.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Platform.Linux;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLinuxPlatformServices(this IServiceCollection services)
    {
        // 添加日志服务（简化，不依赖完整 Logging 包）
        services.AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(Microsoft.Extensions.Logging.Abstractions.NullLogger<>));

        services.AddSingleton<IPlatformService>(sp => new LinuxPlatformService(
            sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LinuxPlatformService>>()));

        services.AddSingleton<IWindowService>(sp => new LinuxWindowService(
            sp.GetRequiredService<ILogger<LinuxWindowService>>()));

        services.AddSingleton<IJavaScanner>(sp => new LinuxJavaScanner(
            sp.GetRequiredService<ILogger<LinuxJavaScanner>>()));

        services.AddSingleton<IThemeService>(sp => new LinuxThemeService(
            sp.GetRequiredService<ILogger<LinuxThemeService>>()));

        services.AddSingleton<IAudioService>(sp => new LinuxAudioService(
            sp.GetRequiredService<ILogger<LinuxAudioService>>()));

        services.AddSingleton<IClipboardService>(sp => new LinuxClipboardService(
            sp.GetRequiredService<ILogger<LinuxClipboardService>>()));

        services.AddSingleton<IDialogService>(sp => new LinuxDialogService(
            sp.GetRequiredService<ILogger<LinuxDialogService>>()));

        services.AddSingleton<INotificationService>(sp => new LinuxNotificationService(
            sp.GetRequiredService<ILogger<LinuxNotificationService>>()));

        services.AddSingleton<IUIAccessProvider>(sp => new LinuxUIAccessProvider(
            sp.GetRequiredService<ILogger<LinuxUIAccessProvider>>()));

        services.AddSingleton<IAnimationService>(sp => new LinuxAnimationService(
            sp.GetRequiredService<ILogger<LinuxAnimationService>>()));

        return services;
    }
}
