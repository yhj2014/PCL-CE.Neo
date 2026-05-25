using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL_CE.Neo.UI.Themes;
using System.Collections.ObjectModel;

namespace PCL_CE.Neo.UI.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _autoDownload = true;

    [ObservableProperty]
    private bool _autoInstall = false;

    [ObservableProperty]
    private bool _checkUpdates = true;

    [ObservableProperty]
    private int _maxConcurrentDownloads = 3;

    [ObservableProperty]
    private string _downloadPath = string.Empty;

    [ObservableProperty]
    private int _selectedThemeIndex = 0;

    public ObservableCollection<string> ThemeOptions { get; } = new()
    {
        "浅色主题",
        "深色主题",
        "跟随系统"
    };

    public ObservableCollection<SettingItem> GeneralSettings { get; } = new();

    public ObservableCollection<SettingItem> GameSettings { get; } = new();

    public ObservableCollection<SettingItem> AdvancedSettings { get; } = new();

    public SettingsViewModel()
    {
        InitializeSettings();
    }

    private void InitializeSettings()
    {
        GeneralSettings.Add(new SettingItem
        {
            Title = "自动下载",
            Description = "自动下载游戏更新",
            IsChecked = AutoDownload
        });

        GeneralSettings.Add(new SettingItem
        {
            Title = "自动安装",
            Description = "下载完成后自动安装",
            IsChecked = AutoInstall
        });

        GeneralSettings.Add(new SettingItem
        {
            Title = "检查更新",
            Description = "启动时检查更新",
            IsChecked = CheckUpdates
        });

        GameSettings.Add(new SettingItem
        {
            Title = "最大并发下载",
            Description = "同时下载的游戏版本数量"
        });

        GameSettings.Add(new SettingItem
        {
            Title = "下载目录",
            Description = "游戏文件存放位置"
        });

        AdvancedSettings.Add(new SettingItem
        {
            Title = "Java 路径",
            Description = "自定义 Java 运行环境"
        });

        AdvancedSettings.Add(new SettingItem
        {
            Title = "内存分配",
            Description = "Minecraft 最大内存使用"
        });
    }

    [RelayCommand]
    private void SaveSettings()
    {
        ShowLoading("正在保存设置...");

        try
        {
            ApplyTheme();
            StatusMessage = "设置已保存";
        }
        finally
        {
            HideLoading();
        }
    }

    [RelayCommand]
    private void ResetSettings()
    {
        AutoDownload = true;
        AutoInstall = false;
        CheckUpdates = true;
        MaxConcurrentDownloads = 3;
        DownloadPath = string.Empty;
        SelectedThemeIndex = 0;
        StatusMessage = "设置已重置";
    }

    private void ApplyTheme()
    {
        var theme = SelectedThemeIndex switch
        {
            0 => AppTheme.Light,
            1 => AppTheme.Dark,
            _ => AppTheme.System
        };

        ThemeManager.Instance.SetTheme(theme);
    }

    partial void OnSelectedThemeIndexChanged(int value)
    {
        ApplyTheme();
    }
}

public class SettingItem : ObservableObject
{
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }

    private string _value = string.Empty;
    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }
}
