using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.UI.ViewModels;

public partial class ToolsViewModel : ViewModelBase
{
    private readonly IModAdapter _modAdapter;
    private readonly IJavaScanner _javaScanner;

    [ObservableProperty]
    private string? _javaVersion;

    [ObservableProperty]
    private string? _javaPath;

    [ObservableProperty]
    private bool _isScanningJava;

    public List<ModInfo> InstalledMods { get; } = new();

    public IAsyncRelayCommand ScanJavaCommand { get; }
    public IAsyncRelayCommand RefreshModsCommand { get; }
    public IAsyncRelayCommand InstallModCommand { get; }

    public ToolsViewModel(
        ILogger<ToolsViewModel> logger,
        IModAdapter modAdapter,
        IJavaScanner javaScanner)
        : base(logger)
    {
        _modAdapter = modAdapter;
        _javaScanner = javaScanner;
        ScanJavaCommand = new AsyncRelayCommand(ScanJavaAsync);
        RefreshModsCommand = new AsyncRelayCommand(RefreshModsAsync);
        InstallModCommand = new AsyncRelayCommand(InstallModAsync);
    }

    public async Task InitializeAsync()
    {
        await ScanJavaAsync();
        await RefreshModsAsync();
    }

    private async Task ScanJavaAsync()
    {
        await ExecuteAsync(async () =>
        {
            IsScanningJava = true;
            Logger.LogInformation("正在扫描 Java 安装");

            var javaPaths = await Task.Run(() => _javaScanner.ScanJavaPaths());
            
            if (javaPaths.Any())
            {
                var javaPath = javaPaths.First();
                JavaPath = javaPath;
                JavaVersion = "未知版本";
                Logger.LogInformation("找到 Java: {Path}", javaPath);
            }
            else
            {
                JavaVersion = "未找到";
                JavaPath = string.Empty;
                Logger.LogWarning("未找到 Java 安装");
            }
        }, "扫描 Java");

        IsScanningJava = false;
    }

    private async Task RefreshModsAsync()
    {
        await ExecuteAsync(async () =>
        {
            var mods = await _modAdapter.GetModsAsync(string.Empty);
            InstalledMods.Clear();
            InstalledMods.AddRange(mods);
            Logger.LogInformation("已加载 {Count} 个模组", InstalledMods.Count);
        }, "刷新模组列表");
    }

    private async Task InstallModAsync()
    {
        await ExecuteAsync(async () =>
        {
            Logger.LogInformation("打开模组安装对话框");
        }, "安装模组");
    }
}