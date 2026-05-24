using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace PCL_CE.Neo.UI.ViewModels;

public partial class InstanceViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private bool _showAllVersions = true;

    public ObservableCollection<GameInstance> Instances { get; } = new();

    public ObservableCollection<GameInstance> FilteredInstances { get; } = new();

    public InstanceViewModel()
    {
        LoadSampleInstances();
    }

    private void LoadSampleInstances()
    {
        Instances.Add(new GameInstance
        {
            Name = "整合包 1.20.4",
            Version = "1.20.4",
            ModCount = 45,
            LastPlayed = DateTime.Now.AddDays(-2),
            TotalPlayTime = TimeSpan.FromHours(50),
            Icon = "🎮"
        });

        Instances.Add(new GameInstance
        {
            Name = "纯净生存",
            Version = "1.19.4",
            ModCount = 0,
            LastPlayed = DateTime.Now.AddDays(-5),
            TotalPlayTime = TimeSpan.FromHours(120),
            Icon = "🏠"
        });

        Instances.Add(new GameInstance
        {
            Name = "空岛生存",
            Version = "1.18.2",
            ModCount = 12,
            LastPlayed = DateTime.Now.AddMonths(-1),
            TotalPlayTime = TimeSpan.FromHours(30),
            Icon = "🏝️"
        });

        Instances.Add(new GameInstance
        {
            Name = "服务器专用",
            Version = "1.12.2",
            ModCount = 8,
            LastPlayed = DateTime.Now.AddMonths(-3),
            TotalPlayTime = TimeSpan.Zero,
            Icon = "🖥️"
        });

        RefreshFilteredInstances();
    }

    partial void OnFilterTextChanged(string value)
    {
        RefreshFilteredInstances();
    }

    partial void OnShowAllVersionsChanged(bool value)
    {
        RefreshFilteredInstances();
    }

    private void RefreshFilteredInstances()
    {
        FilteredInstances.Clear();

        var filtered = Instances.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(FilterText))
        {
            filtered = filtered.Where(i =>
                i.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                i.Version.Contains(FilterText, StringComparison.OrdinalIgnoreCase));
        }

        if (!ShowAllVersions)
        {
            filtered = filtered.Where(i => i.ModCount > 0);
        }

        foreach (var instance in filtered.OrderByDescending(i => i.LastPlayed))
        {
            FilteredInstances.Add(instance);
        }
    }

    [RelayCommand]
    private void CreateInstance()
    {
        var newInstance = new GameInstance
        {
            Name = "新实例",
            Version = "1.20.4",
            ModCount = 0,
            LastPlayed = DateTime.Now,
            Icon = "📦"
        };

        Instances.Add(newInstance);
        RefreshFilteredInstances();
        StatusMessage = "已创建新实例";
    }

    [RelayCommand]
    private void DeleteInstance(GameInstance? instance)
    {
        if (instance != null)
        {
            Instances.Remove(instance);
            RefreshFilteredInstances();
            StatusMessage = "已删除实例";
        }
    }

    [RelayCommand]
    private void LaunchInstance(GameInstance? instance)
    {
        if (instance != null)
        {
            StatusMessage = $"正在启动 {instance.Name}...";
        }
    }
}

public partial class GameInstance : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _version = string.Empty;

    [ObservableProperty]
    private int _modCount;

    [ObservableProperty]
    private DateTime _lastPlayed;

    [ObservableProperty]
    private TimeSpan _totalPlayTime;

    [ObservableProperty]
    private string _icon = "🎮";

    public string LastPlayedText => LastPlayed.ToString("yyyy-MM-dd HH:mm");

    public string PlayTimeText => TotalPlayTime.TotalHours >= 1
        ? $"{(int)TotalPlayTime.TotalHours}h {TotalPlayTime.Minutes}m"
        : $"{TotalPlayTime.Minutes}m";
}
