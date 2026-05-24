using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace PCL_CE.Neo.UI.Navigation;

public partial class NavigationService : ObservableObject
{
    private static NavigationService? _instance;
    private Frame? _frame;

    public static NavigationService Instance => _instance ??= new NavigationService();

    [ObservableProperty]
    private string? _currentPageTitle;

    [ObservableProperty]
    private bool _canGoBack;

    public ObservableCollection<NavigationItem> NavigationItems { get; } = new();

    private NavigationService()
    {
    }

    public void Initialize(Frame frame)
    {
        _frame = frame;
        _frame.Navigated += (s, e) =>
        {
            CanGoBack = _frame.CanGoBack;
            UpdateCurrentPageTitle();
        };
    }

    public void Navigate(Type pageType, object? parameter = null)
    {
        _frame?.Navigate(pageType, parameter);
    }

    public void GoBack()
    {
        if (_frame?.CanGoBack == true)
        {
            _frame.GoBack();
        }
    }

    private void UpdateCurrentPageTitle()
    {
        if (_frame?.Content is Page page)
        {
            CurrentPageTitle = page.GetType().Name.Replace("Page", "");
        }
    }
}

public record NavigationItem(string Title, Type PageType, string? Icon = null);
