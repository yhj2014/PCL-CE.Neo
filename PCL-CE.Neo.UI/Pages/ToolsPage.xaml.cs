using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PCL_CE.Neo.UI.Navigation;

namespace PCL_CE.Neo.UI.Pages;

public sealed partial class ToolsPage : Page
{
    public ToolsPage()
    {
        InitializeComponent();
    }

    private void OnBackClick(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.GoBack();
    }

    private void OnForgeInstallerClick(object sender, RoutedEventArgs e)
    {
    }

    private void OnFabricInstallerClick(object sender, RoutedEventArgs e)
    {
    }

    private void OnOptiFineInstallerClick(object sender, RoutedEventArgs e)
    {
    }

    private void OnJavaCheckerClick(object sender, RoutedEventArgs e)
    {
    }

    private void OnCacheCleanerClick(object sender, RoutedEventArgs e)
    {
    }

    private void OnBackupManagerClick(object sender, RoutedEventArgs e)
    {
    }

    private void OnNetworkDiagnosticsClick(object sender, RoutedEventArgs e)
    {
    }

    private void OnLogViewerClick(object sender, RoutedEventArgs e)
    {
    }
}
