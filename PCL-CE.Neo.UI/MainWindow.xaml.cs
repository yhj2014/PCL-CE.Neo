using Microsoft.UI.Xaml;
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
        // Reset all navigation buttons
        NavHome.Background = new UI.Xaml.Media.SolidColorBrush(UI.Colors.Transparent);
        NavLaunch.Background = new UI.Xaml.Media.SolidColorBrush(UI.Colors.Transparent);
        NavVersions.Background = new UI.Xaml.Media.SolidColorBrush(UI.Colors.Transparent);
        NavInstances.Background = new UI.Xaml.Media.SolidColorBrush(UI.Colors.Transparent);
        NavLogin.Background = new UI.Xaml.Media.SolidColorBrush(UI.Colors.Transparent);
        NavTools.Background = new UI.Xaml.Media.SolidColorBrush(UI.Colors.Transparent);
        NavSettings.Background = new UI.Xaml.Media.SolidColorBrush(UI.Colors.Transparent);

        // Highlight the current page
        var activeColor = new UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(40, 52, 152, 219));
        if (currentPage == typeof(HomePage)) NavHome.Background = activeColor;
        else if (currentPage == typeof(LaunchPage)) NavLaunch.Background = activeColor;
        else if (currentPage == typeof(VersionSelectPage)) NavVersions.Background = activeColor;
        else if (currentPage == typeof(InstancePage)) NavInstances.Background = activeColor;
        else if (currentPage == typeof(LoginPage)) NavLogin.Background = activeColor;
        else if (currentPage == typeof(ToolsPage)) NavTools.Background = activeColor;
        else if (currentPage == typeof(SettingsPage)) NavSettings.Background = activeColor;
    }
}
