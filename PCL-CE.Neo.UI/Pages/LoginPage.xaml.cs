using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PCL_CE.Neo.UI.Navigation;
using PCL_CE.Neo.UI.ViewModels;

namespace PCL_CE.Neo.UI.Pages;

public sealed partial class LoginPage : Page
{
    private readonly LoginViewModel _viewModel;

    public LoginPage()
    {
        InitializeComponent();
        _viewModel = new LoginViewModel();
        DataContext = _viewModel;
    }

    private void OnBackClick(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.GoBack();
    }

    private void OnOfflineModeClick(object sender, RoutedEventArgs e)
    {
        _viewModel.OfflineModeCommand.Execute(null);
    }
}
