using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace PCL_CE.Neo.UI.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string _statusMessage = string.Empty;

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public virtual void OnNavigatedTo() { }

    public virtual void OnNavigatedFrom() { }

    protected void ShowLoading(string? message = null)
    {
        IsLoading = true;
        if (!string.IsNullOrEmpty(message))
        {
            StatusMessage = message;
        }
    }

    protected void HideLoading()
    {
        IsLoading = false;
        StatusMessage = string.Empty;
    }
}
