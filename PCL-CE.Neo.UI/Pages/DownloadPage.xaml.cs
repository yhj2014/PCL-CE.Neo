using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PCL_CE.Neo.UI.Navigation;
using System.Collections.ObjectModel;

namespace PCL_CE.Neo.UI.Pages;

public sealed partial class DownloadPage : Page
{
    public ObservableCollection<DownloadItem> Downloads { get; } = new();

    public DownloadPage()
    {
        InitializeComponent();
        DownloadQueue.ItemsSource = Downloads;
        LoadSampleDownloads();
    }

    private void LoadSampleDownloads()
    {
        Downloads.Add(new DownloadItem
        {
            Name = "Minecraft 1.20.4",
            Status = "下载中... 45MB / 100MB",
            Progress = 45,
            IsPaused = false
        });

        Downloads.Add(new DownloadItem
        {
            Name = "OptiFine HD U9",
            Status = "等待中",
            Progress = 0,
            IsPaused = false
        });

        Downloads.Add(new DownloadItem
        {
            Name = "Fabric API",
            Status = "已完成",
            Progress = 100,
            IsCompleted = true
        });
    }

    private void OnBackClick(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.GoBack();
    }

    private void OnOpenFolderClick(object sender, RoutedEventArgs e)
    {
        // 打开下载文件夹
    }

    private void OnPauseResumeClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is DownloadItem item)
        {
            item.IsPaused = !item.IsPaused;
            item.Status = item.IsPaused ? "已暂停" : "下载中...";
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is DownloadItem item)
        {
            Downloads.Remove(item);
        }
    }

    private void OnClearCompletedClick(object sender, RoutedEventArgs e)
    {
        var completedItems = Downloads.Where(d => d.IsCompleted).ToList();
        foreach (var item in completedItems)
        {
            Downloads.Remove(item);
        }
    }

    private void OnStartAllClick(object sender, RoutedEventArgs e)
    {
        foreach (var item in Downloads.Where(d => !d.IsCompleted && !d.IsPaused))
        {
            item.Status = "下载中...";
        }
    }
}

public class DownloadItem
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public bool IsPaused { get; set; }
    public bool IsCompleted { get; set; }
}
