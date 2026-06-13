using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PCL_CE.Neo.UI.Pages;

namespace PCL_CE.Neo.UI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MainFrame.Navigate(typeof(HomePage));
    }

    private void OnMinimizeClick(object sender, RoutedEventArgs e)
    {
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnNavHomeClick(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(typeof(HomePage));
    }

    private void OnNavDownloadClick(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(typeof(DownloadPage));
    }

    private void OnNavLaunchClick(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(typeof(LaunchPage));
    }

    private void OnNavInstanceClick(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(typeof(InstancePage));
    }

    private void OnNavLoginClick(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(typeof(LoginPage));
    }

    private void OnNavToolsClick(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(typeof(ToolsPage));
    }

    private void OnNavSettingsClick(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(typeof(SettingsPage));
    }
}
