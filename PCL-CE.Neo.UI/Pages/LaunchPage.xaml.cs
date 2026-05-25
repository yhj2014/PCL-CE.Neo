using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PCL_CE.Neo.UI.Navigation;
using PCL_CE.Neo.UI.ViewModels;

namespace PCL_CE.Neo.UI.Pages;

public sealed partial class LaunchPage : Page
{
    private readonly LaunchViewModel _viewModel;

    public LaunchPage()
    {
        InitializeComponent();
        _viewModel = new LaunchViewModel();
        DataContext = _viewModel;
    }

    private void OnBackClick(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.GoBack();
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        _viewModel.OpenSettingsCommand.Execute(null);
    }

    private void OnInstanceTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is Border border && border.Tag is GameInstance instance)
        {
            _viewModel.SelectInstanceCommand.Execute(instance);
        }
    }

    private void OnQuickLaunchClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is GameInstance instance)
        {
            _viewModel.SelectInstanceCommand.Execute(instance);
            _viewModel.LaunchGameCommand.Execute(null);
        }
    }
}
