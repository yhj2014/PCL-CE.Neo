using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.UI.ViewModels;

public partial class LaunchViewModel : ViewModelBase
{
    private readonly IMinecraftAdapter _minecraftAdapter;
    private readonly IInstanceAdapter _instanceAdapter;

    [ObservableProperty]
    private string? _selectedInstance;

    [ObservableProperty]
    private string? _selectedVersion;

    [ObservableProperty]
    private string? _javaPath;

    [ObservableProperty]
    private int _memoryAllocation = 4096;

    [ObservableProperty]
    private bool _isFullscreen;

    [ObservableProperty]
    private bool _showAdvancedSettings;

    [ObservableProperty]
    private bool _isLaunching;

    public List<string> Instances { get; } = new();
    public List<string> Versions { get; } = new();

    public IAsyncRelayCommand LaunchCommand { get; }

    public LaunchViewModel(
        ILogger<LaunchViewModel> logger,
        IMinecraftAdapter minecraftAdapter,
        IInstanceAdapter instanceAdapter)
        : base(logger)
    {
        _minecraftAdapter = minecraftAdapter;
        _instanceAdapter = instanceAdapter;
        LaunchCommand = new AsyncRelayCommand(LaunchGameAsync);
    }

    public async Task LoadDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            var instances = await _instanceAdapter.GetAllInstancesAsync();
            Instances.Clear();
            Instances.AddRange(instances.Select(i => i.Name));

            var versionList = await _minecraftAdapter.GetMinecraftVersionListAsync();
            Versions.Clear();
            Versions.Add("1.20.1");
            Versions.Add("1.19.4");
            Versions.Add("1.18.2");

            if (Instances.Any())
            {
                SelectedInstance = Instances.First();
            }
            if (Versions.Any())
            {
                SelectedVersion = Versions.First();
            }
        }, "加载启动数据");
    }

    private async Task LaunchGameAsync()
    {
        if (string.IsNullOrEmpty(SelectedInstance) || string.IsNullOrEmpty(SelectedVersion))
        {
            ErrorMessage = "请选择实例和版本";
            return;
        }

        await ExecuteAsync(async () =>
        {
            IsLaunching = true;
            Logger.LogInformation("正在启动 Minecraft: {Instance} - {Version}", SelectedInstance, SelectedVersion);

            var instance = await _instanceAdapter.GetInstanceAsync(SelectedInstance);
            var gameDir = instance?.Folder ?? string.Empty;

            var java = await _minecraftAdapter.DetectJavaAsync();
            if (java == null)
            {
                ErrorMessage = "未检测到 Java 安装";
                return;
            }

            var options = new GameLaunchOptions
            {
                InstanceId = SelectedInstance,
                MinecraftVersion = SelectedVersion,
                Java = java,
                GameDirectory = gameDir,
                MemoryMB = MemoryAllocation,
                Fullscreen = IsFullscreen,
                Username = "Player",
                Uuid = Guid.NewGuid().ToString(),
                AccessToken = string.Empty
            };

            var result = await _minecraftAdapter.LaunchGameAsync(options);

            if (result.Success)
            {
                Logger.LogInformation("Minecraft 启动成功，进程ID: {ProcessId}", result.ProcessId);
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "启动失败";
                Logger.LogError("Minecraft 启动失败: {Message}", result.ErrorMessage);
            }
        }, "启动游戏");

        IsLaunching = false;
    }
}