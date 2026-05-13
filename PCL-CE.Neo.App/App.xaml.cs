using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PCL.CE.Neo.Core.Abstractions;
using PCL.CE.Neo.Platform.Windows;
using PCL.CE.Neo.UI;

namespace PCL.CE.Neo.App;

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
        services.AddWindowsPlatformServices();
        services.AddTransient<MainViewModel>();
    }
}
