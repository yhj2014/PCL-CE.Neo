using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PCL_CE.Neo.UI.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _rememberMe;

    [ObservableProperty]
    private bool _isOfflineMode;

    [ObservableProperty]
    private string _statusMessage = "准备就绪";

    public bool CanLogin => !string.IsNullOrWhiteSpace(Username) && !IsLoading;

    public LoginViewModel()
    {
    }

    partial void OnUsernameChanged(string value)
    {
        OnPropertyChanged(nameof(CanLogin));
    }

    [RelayCommand]
    private async Task Login()
    {
        if (!CanLogin)
            return;

        ShowLoading(IsOfflineMode ? "正在离线登录..." : "正在登录...");

        try
        {
            // 模拟登录过程
            await Task.Delay(2000);

            if (IsOfflineMode)
            {
                StatusMessage = "离线登录成功！";
            }
            else
            {
                StatusMessage = "登录成功！";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"登录失败: {ex.Message}";
        }
        finally
        {
            HideLoading();
        }
    }

    [RelayCommand]
    private void OfflineMode()
    {
        IsOfflineMode = !IsOfflineMode;
        StatusMessage = IsOfflineMode ? "已切换到离线模式" : "已切换到在线模式";
    }
}
