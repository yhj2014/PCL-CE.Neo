using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.UI.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    protected readonly ILogger Logger;

    private bool _isLoading;
    private string? _errorMessage;

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    protected ViewModelBase(ILogger logger)
    {
        Logger = logger;
    }

    protected virtual async Task ExecuteAsync(Func<Task> operation, string operationName)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            await operation();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"操作失败: {ex.Message}";
            Logger.LogError(ex, "{OperationName} 失败", operationName);
        }
        finally
        {
            IsLoading = false;
        }
    }
}