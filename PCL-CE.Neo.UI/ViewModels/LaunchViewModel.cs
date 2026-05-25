using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace PCL_CE.Neo.UI.ViewModels;

public partial class LaunchViewModel : ViewModelBase
{
    [ObservableProperty]
    private GameInstance? _selectedInstance;

    [ObservableProperty]
    private int _memoryAllocation = 2048;

    [ObservableProperty]
    private string _additionalArguments = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "准备就绪";

    public ObservableCollection<GameInstance> RecentInstances { get; } = new();

    public LaunchViewModel()
    {
        LoadRecentInstances();
    }

    private void LoadRecentInstances()
    {
        RecentInstances.Clear();
        RecentInstances.Add(new GameInstance
        {
            Name = "整合包 1.20.4",
            Version = "1.20.4",
            ModCount = 45,
            LastPlayed = DateTime.Now.AddDays(-2),
            TotalPlayTime = TimeSpan.FromHours(50),
            Icon = "🎮"
        });
        RecentInstances.Add(new GameInstance
        {
            Name = "纯净生存",
            Version = "1.19.4",
            ModCount = 0,
            LastPlayed = DateTime.Now.AddDays(-5),
            TotalPlayTime = TimeSpan.FromHours(120),
            Icon = "🏠"
        });
        RecentInstances.Add(new GameInstance
        {
            Name = "空岛生存",
            Version = "1.18.2",
            ModCount = 12,
            LastPlayed = DateTime.Now.AddMonths(-1),
            TotalPlayTime = TimeSpan.FromHours(30),
            Icon = "🏝️"
        });

        if (RecentInstances.Count > 0)
        {
            SelectedInstance = RecentInstances[0];
        }
    }

    [RelayCommand]
    private async Task LaunchGame()
    {
        if (SelectedInstance == null)
        {
            StatusMessage = "请先选择游戏实例";
            return;
        }

        ShowLoading($"正在启动 {SelectedInstance.Name}...");

        try
        {
            // 模拟启动过程
            StatusMessage = "正在检查游戏文件...";
            await Task.Delay(1000);

            StatusMessage = "正在验证资源...";
            await Task.Delay(1000);

            StatusMessage = "正在准备启动...";
            await Task.Delay(1000);

            StatusMessage = "游戏启动成功！";
        }
        catch (Exception ex)
        {
            StatusMessage = $"启动失败: {ex.Message}";
        }
        finally
        {
            HideLoading();
        }
    }

    [RelayCommand]
    private void SelectInstance(GameInstance? instance)
    {
        SelectedInstance = instance;
        if (instance != null)
        {
            StatusMessage = $"已选择: {instance.Name}";
        }
    }

    [RelayCommand]
    private void OpenSettings()
    {
        StatusMessage = "设置功能开发中...";
    }
}
