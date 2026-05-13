using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Core;
using PCL_CE.Neo.Platform.Windows;
using PCL_CE.Neo.UI;

namespace PCL_CE.Neo.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        var mainWindow = new MainWindow();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddPlatformServices("Windows");
        services.AddTransient<MainViewModel>();
    }
}
