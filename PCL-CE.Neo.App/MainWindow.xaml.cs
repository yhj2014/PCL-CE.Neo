using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PCL_CE.Neo.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }
}

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "PCL-CE.Neo";

    [ObservableProperty]
    private string _currentPage = "Launch";

    [RelayCommand]
    private void NavigateTo(string page)
    {
        CurrentPage = page;
    }

    [RelayCommand]
    private void Minimize()
    {
        if (System.Windows.Application.Current.MainWindow != null)
        {
            System.Windows.Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
    }

    [RelayCommand]
    private void Maximize()
    {
        if (System.Windows.Application.Current.MainWindow != null)
        {
            var window = System.Windows.Application.Current.MainWindow;
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
    }

    [RelayCommand]
    private void Close()
    {
        System.Windows.Application.Current.Shutdown();
    }
}
