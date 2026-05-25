using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PCL_CE.Neo.UI.Navigation;
using PCL_CE.Neo.UI.ViewModels;

namespace PCL_CE.Neo.UI.Pages;

public sealed partial class InstancePage : Page
{
    private readonly InstanceViewModel _viewModel;

    public InstancePage()
    {
        InitializeComponent();
        _viewModel = new InstanceViewModel();
        DataContext = _viewModel;
    }

    private void OnBackClick(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.GoBack();
    }

    private void OnCreateInstanceClick(object sender, RoutedEventArgs e)
    {
        _viewModel.CreateInstanceCommand.Execute(null);
    }

    private void OnLaunchClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is GameInstance instance)
        {
            _viewModel.LaunchInstanceCommand.Execute(instance);
        }
    }

    private void OnEditClick(object sender, RoutedEventArgs e)
    {
        // 导航到编辑页面
    }

    private void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is GameInstance instance)
        {
            _viewModel.DeleteInstanceCommand.Execute(instance);
        }
    }
}
