using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PCL_CE.Neo.UI;

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
}
