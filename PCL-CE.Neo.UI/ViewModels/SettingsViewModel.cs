using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.UI.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IConfigAdapter _configAdapter;
    private readonly IPlatformService _platformService;

    [ObservableProperty]
    private string? _gameDirectory;

    [ObservableProperty]
    private int _maxMemory = 4096;

    [ObservableProperty]
    private int _minMemory = 2048;

    [ObservableProperty]
    private bool _enableAutoUpdate;

    [ObservableProperty]
    private bool _enableTelemetry;

    [ObservableProperty]
    private string? _selectedLanguage = "zh-CN";

    [ObservableProperty]
    private bool _closeToTray;

    public List<string> Languages { get; } = new();

    public IAsyncRelayCommand LoadSettingsCommand { get; }
    public IAsyncRelayCommand SaveSettingsCommand { get; }
    public IAsyncRelayCommand ResetSettingsCommand { get; }

    public SettingsViewModel(
        ILogger<SettingsViewModel> logger,
        IConfigAdapter configAdapter,
        IPlatformService platformService)
        : base(logger)
    {
        _configAdapter = configAdapter;
        _platformService = platformService;
        LoadSettingsCommand = new AsyncRelayCommand(LoadSettingsAsync);
        SaveSettingsCommand = new AsyncRelayCommand(SaveSettingsAsync);
        ResetSettingsCommand = new AsyncRelayCommand(ResetSettingsAsync);
        InitializeLanguages();
    }

    private void InitializeLanguages()
    {
        Languages.Add("zh-CN");
        Languages.Add("zh-TW");
        Languages.Add("en-US");
        Languages.Add("ja-JP");
        Languages.Add("ko-KR");
        Languages.Add("fr-FR");
        Languages.Add("de-DE");
    }

    public async Task LoadSettingsAsync()
    {
        await ExecuteAsync(async () =>
        {
            _configAdapter.LoadConfig();
            GameDirectory = _configAdapter.GetConfig<string>("GameDirectory", 
                _platformService.GetGameDataPath());
            MaxMemory = _configAdapter.GetConfig<int>("MaxMemory", 4096);
            MinMemory = _configAdapter.GetConfig<int>("MinMemory", 2048);
            EnableAutoUpdate = _configAdapter.GetConfig<bool>("EnableAutoUpdate", true);
            EnableTelemetry = _configAdapter.GetConfig<bool>("EnableTelemetry", true);
            SelectedLanguage = _configAdapter.GetConfig<string>("Language", "zh-CN");
            CloseToTray = _configAdapter.GetConfig<bool>("CloseToTray", false);
            Logger.LogInformation("设置已加载");
        }, "加载设置");
    }

    private async Task SaveSettingsAsync()
    {
        await ExecuteAsync(async () =>
        {
            _configAdapter.SetConfig("GameDirectory", GameDirectory);
            _configAdapter.SetConfig("MaxMemory", MaxMemory);
            _configAdapter.SetConfig("MinMemory", MinMemory);
            _configAdapter.SetConfig("EnableAutoUpdate", EnableAutoUpdate);
            _configAdapter.SetConfig("EnableTelemetry", EnableTelemetry);
            _configAdapter.SetConfig("Language", SelectedLanguage);
            _configAdapter.SetConfig("CloseToTray", CloseToTray);
            _configAdapter.SaveConfig();
            Logger.LogInformation("设置已保存");
            await Task.CompletedTask;
        }, "保存设置");
    }

    private async Task ResetSettingsAsync()
    {
        await ExecuteAsync(async () =>
        {
            GameDirectory = _platformService.GetGameDataPath();
            MaxMemory = 4096;
            MinMemory = 2048;
            EnableAutoUpdate = true;
            EnableTelemetry = true;
            SelectedLanguage = "zh-CN";
            CloseToTray = false;
            await SaveSettingsAsync();
            Logger.LogInformation("设置已重置为默认值");
        }, "重置设置");
    }
}