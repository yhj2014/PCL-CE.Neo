using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.UI.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthAdapter _authAdapter;

    [ObservableProperty]
    private string? _username;

    [ObservableProperty]
    private string? _password;

    [ObservableProperty]
    private string? _selectedAuthProvider;

    [ObservableProperty]
    private bool _isRememberMe;

    [ObservableProperty]
    private bool _isLoggingIn;

    public List<string> AuthProviders { get; } = new();

    public IAsyncRelayCommand LoginCommand { get; }
    public IAsyncRelayCommand LogoutCommand { get; }

    public LoginViewModel(
        ILogger<LoginViewModel> logger,
        IAuthAdapter authAdapter)
        : base(logger)
    {
        _authAdapter = authAdapter;
        LoginCommand = new AsyncRelayCommand(LoginAsync);
        LogoutCommand = new AsyncRelayCommand(LogoutAsync);
        InitializeAuthProviders();
    }

    private void InitializeAuthProviders()
    {
        AuthProviders.Add("Microsoft");
        AuthProviders.Add("Offline");
        if (AuthProviders.Any())
        {
            SelectedAuthProvider = AuthProviders.First();
        }
    }

    public async Task CheckLoginStatusAsync()
    {
        await ExecuteAsync(async () =>
        {
            var user = await _authAdapter.GetCurrentUserAsync();
            Logger.LogInformation("当前登录状态: {IsLoggedIn}", user != null);
        }, "检查登录状态");
    }

    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "请输入用户名";
            return;
        }

        if (string.IsNullOrEmpty(SelectedAuthProvider))
        {
            ErrorMessage = "请选择认证方式";
            return;
        }

        await ExecuteAsync(async () =>
        {
            IsLoggingIn = true;
            Logger.LogInformation("正在登录: {Provider} - {Username}", SelectedAuthProvider, Username);

            AuthResult result;

            if (SelectedAuthProvider == "Offline")
            {
                result = await _authAdapter.LoginOfflineAsync(Username);
            }
            else
            {
                result = await _authAdapter.LoginMicrosoftAsync();
            }

            if (result.Success)
            {
                Logger.LogInformation("登录成功: {Username}", result.User?.Username);
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "登录失败";
                Logger.LogError("登录失败: {Message}", result.ErrorMessage);
            }
        }, "登录");

        IsLoggingIn = false;
    }

    private async Task LogoutAsync()
    {
        await ExecuteAsync(async () =>
        {
            await _authAdapter.LogoutAsync();
            Logger.LogInformation("已退出登录");
            Username = string.Empty;
            Password = string.Empty;
        }, "退出登录");
    }
}