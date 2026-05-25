using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PCL_CE.Neo.UI.Navigation;
using PCL_CE.Neo.UI.ViewModels;

namespace PCL_CE.Neo.UI.Pages;

public sealed partial class SettingsPage : Page
{
    private readonly SettingsViewModel _viewModel;

    public SettingsPage()
    {
        InitializeComponent();
        _viewModel = new SettingsViewModel();
        DataContext = _viewModel;
    }

    private void OnBackClick(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.GoBack();
    }

    private void OnAutoDownloadChanged(object sender, bool e)
    {
        _viewModel.AutoDownload = e;
    }

    private void OnAutoInstallChanged(object sender, bool e)
    {
        _viewModel.AutoInstall = e;
    }

    private void OnCheckUpdatesChanged(object sender, bool e)
    {
        _viewModel.CheckUpdates = e;
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        _viewModel.SaveSettingsCommand.Execute(null);
    }

    private void OnResetClick(object sender, RoutedEventArgs e)
    {
        _viewModel.ResetSettingsCommand.Execute(null);
    }
}
