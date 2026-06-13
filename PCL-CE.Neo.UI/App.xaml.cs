using Microsoft.UI.Xaml;
using PCL_CE.Neo.UI.Pages;

namespace PCL_CE.Neo.UI;

public sealed partial class App : Application
{
    public App()
    {
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var window = new MainWindow();
        window.Activate();
    }
}
