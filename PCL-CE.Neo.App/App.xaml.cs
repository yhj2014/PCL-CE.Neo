using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Core;
using PCL_CE.Neo.UI;

namespace PCL_CE.Neo.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    private Window? _window;

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        _window = new MainWindow();
        Window.ActiveWindow = _window;
        _window.Activate();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
#if WINDOWS
        services.AddWindowsPlatformServices();
#elif MACOS
        services.AddMacOSPlatformServices();
#elif LINUX
        services.AddLinuxPlatformServices();
#endif

        services.AddSingleton<MainViewModel>();
    }
}
