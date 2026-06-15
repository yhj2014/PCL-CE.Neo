using Microsoft.Extensions.DependencyInjection;
using PCL_CE.Neo.UI.Navigation;
using PCL_CE.Neo.UI.Themes;
using PCL_CE.Neo.UI.ViewModels;

namespace PCL_CE.Neo.UI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUIServices(this IServiceCollection services)
    {
        services.AddSingleton<IThemeManager, ThemeManager>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddTransient<LaunchViewModel>();
        services.AddTransient<InstanceViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<ToolsViewModel>();
        services.AddTransient<VersionSelectViewModel>();
        return services;
    }
}