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
    }

    private void OnLightThemeClick(object sender, RoutedEventArgs e)
    {
        ThemeManager.Instance.SetTheme(AppTheme.Light);
    }

    private void OnDarkThemeClick(object sender, RoutedEventArgs e)
    {
        ThemeManager.Instance.SetTheme(AppTheme.Dark);
    }
}
