using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PCL_CE.Neo.UI;

namespace PCL_CE.Neo.App;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        this.Minimize();
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.WindowState == WindowState.Maximized)
            this.WindowState = WindowState.Normal;
        else
            this.WindowState = WindowState.Maximized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void ContentFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
    }
}
