using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using PCL_CE.Neo.UI.Themes;
using PCL_CE.Neo.UI.Navigation;
using PCL_CE.Neo.UI.Pages;

namespace PCL_CE.Neo.UI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ThemeManager.Instance.Initialize();
        NavigationService.Instance.Initialize(MainFrame);
        NavigationService.Instance.Navigate(typeof(HomePage));
        UpdateNavigationState(typeof(HomePage));
    }

    private void OnLightThemeClick(object sender, RoutedEventArgs e)
    {
        ThemeManager.Instance.SetTheme(AppTheme.Light);
    }

    private void OnDarkThemeClick(object sender, RoutedEventArgs e)
    {
        ThemeManager.Instance.SetTheme(AppTheme.Dark);
    }

    private void OnNavHomeClick(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.Navigate(typeof(HomePage));
        UpdateNavigationState(typeof(HomePage));
    }

    private void OnNavLaunchClick(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.Navigate(typeof(LaunchPage));
        UpdateNavigationState(typeof(LaunchPage));
    }

    private void OnNavVersionsClick(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.Navigate(typeof(VersionSelectPage));
        UpdateNavigationState(typeof(VersionSelectPage));
    }

    private void OnNavInstancesClick(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.Navigate(typeof(InstancePage));
        UpdateNavigationState(typeof(InstancePage));
    }

    private void OnNavLoginClick(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.Navigate(typeof(LoginPage));
        UpdateNavigationState(typeof(LoginPage));
    }

    private void OnNavToolsClick(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.Navigate(typeof(ToolsPage));
        UpdateNavigationState(typeof(ToolsPage));
    }

    private void OnNavSettingsClick(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.Navigate(typeof(SettingsPage));
        UpdateNavigationState(typeof(SettingsPage));
    }

    private void UpdateNavigationState(Type currentPage)
    {
        // 暂时简化，不处理导航状态
    }
}
