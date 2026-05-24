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

    private void OnForgeInstallerClick(object sender, TappedRoutedEventArgs e)
    {
    }

    private void OnFabricInstallerClick(object sender, TappedRoutedEventArgs e)
    {
    }

    private void OnOptiFineInstallerClick(object sender, TappedRoutedEventArgs e)
    {
    }

    private void OnJavaCheckerClick(object sender, TappedRoutedEventArgs e)
    {
    }

    private void OnCacheCleanerClick(object sender, TappedRoutedEventArgs e)
    {
    }

    private void OnBackupManagerClick(object sender, TappedRoutedEventArgs e)
    {
    }

    private void OnNetworkDiagnosticsClick(object sender, TappedRoutedEventArgs e)
    {
    }

    private void OnLogViewerClick(object sender, TappedRoutedEventArgs e)
    {
    }
}
