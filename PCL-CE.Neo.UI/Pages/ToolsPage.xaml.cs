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
        // TODO: 导航到 Forge 安装器
    }

    private void OnFabricInstallerClick(object sender, TappedRoutedEventArgs e)
    {
        // TODO: 导航到 Fabric 安装器
    }

    private void OnOptiFineInstallerClick(object sender, TappedRoutedEventArgs e)
    {
        // TODO: 导航到 OptiFine 安装器
    }

    private void OnJavaCheckerClick(object sender, TappedRoutedEventArgs e)
    {
        // TODO: 导航到 Java 检测器
    }

    private void OnCacheCleanerClick(object sender, TappedRoutedEventArgs e)
    {
        // TODO: 导航到缓存清理器
    }

    private void OnBackupManagerClick(object sender, TappedRoutedEventArgs e)
    {
        // TODO: 导航到备份管理器
    }

    private void OnNetworkDiagnosticsClick(object sender, TappedRoutedEventArgs e)
    {
        // TODO: 导航到网络诊断
    }

    private void OnLogViewerClick(object sender, TappedRoutedEventArgs e)
    {
        // TODO: 导航到日志查看器
    }
}
