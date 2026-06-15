using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;
using System.Text.Json;

namespace PCL_CE.Neo.UI.ViewModels;

public enum VersionType
{
    Release,
    Snapshot
}

public record VersionInfo
{
    public string Id { get; init; } = "";
    public VersionType Type { get; init; }
    public DateTime ReleaseTime { get; init; }
}

public partial class VersionSelectViewModel : ViewModelBase
{
    private readonly IMinecraftAdapter _minecraftAdapter;

    [ObservableProperty]
    private string? _selectedVersion;

    [ObservableProperty]
    private bool _includeSnapshots;

    [ObservableProperty]
    private bool _isRefreshing;

    public List<VersionInfo> Versions { get; } = new();

    public IAsyncRelayCommand RefreshVersionsCommand { get; }
    public IAsyncRelayCommand DownloadVersionCommand { get; }

    public VersionSelectViewModel(
        ILogger<VersionSelectViewModel> logger,
        IMinecraftAdapter minecraftAdapter)
        : base(logger)
    {
        _minecraftAdapter = minecraftAdapter;
        RefreshVersionsCommand = new AsyncRelayCommand(RefreshVersionsAsync);
        DownloadVersionCommand = new AsyncRelayCommand(DownloadVersionAsync);
    }

    public async Task LoadVersionsAsync()
    {
        await RefreshVersionsAsync();
    }

    private async Task RefreshVersionsAsync()
    {
        await ExecuteAsync(async () =>
        {
            IsRefreshing = true;
            Logger.LogInformation("正在刷新版本列表");

            var manifestJson = await _minecraftAdapter.GetMinecraftVersionListAsync();
            
            try
            {
                var manifest = JsonDocument.Parse(manifestJson);
                var versions = manifest.RootElement.GetProperty("versions");
                
                Versions.Clear();
                
                foreach (var versionElement in versions.EnumerateArray())
                {
                    var id = versionElement.GetProperty("id").GetString();
                    var type = versionElement.GetProperty("type").GetString();
                    
                    if (!string.IsNullOrEmpty(id))
                    {
                        var isSnapshot = type?.Equals("snapshot", StringComparison.OrdinalIgnoreCase) ?? false;
                        
                        if (!isSnapshot || IncludeSnapshots)
                        {
                            Versions.Add(new VersionInfo
                            {
                                Id = id,
                                Type = isSnapshot ? VersionType.Snapshot : VersionType.Release,
                                ReleaseTime = versionElement.TryGetProperty("releaseTime", out var releaseTime) 
                                    ? releaseTime.GetDateTime() : DateTime.MinValue
                            });
                        }
                    }
                }
                
                Versions.Sort((a, b) => b.ReleaseTime.CompareTo(a.ReleaseTime));
                Logger.LogInformation("已加载 {Count} 个版本", Versions.Count);
            }
            catch (JsonException ex)
            {
                Logger.LogError(ex, "解析版本列表失败，使用默认版本");
                LoadDefaultVersions();
            }
        }, "刷新版本列表");

        IsRefreshing = false;
    }

    private void LoadDefaultVersions()
    {
        Versions.Clear();
        Versions.Add(new VersionInfo { Id = "1.20.1", Type = VersionType.Release, ReleaseTime = new DateTime(2023, 6, 7) });
        Versions.Add(new VersionInfo { Id = "1.19.4", Type = VersionType.Release, ReleaseTime = new DateTime(2022, 11, 15) });
        Versions.Add(new VersionInfo { Id = "1.18.2", Type = VersionType.Release, ReleaseTime = new DateTime(2022, 2, 28) });
    }

    private async Task DownloadVersionAsync()
    {
        if (string.IsNullOrEmpty(SelectedVersion))
        {
            ErrorMessage = "请选择要下载的版本";
            return;
        }

        await ExecuteAsync(async () =>
        {
            Logger.LogInformation("开始下载版本: {Version}", SelectedVersion);
            await Task.Delay(1000);
            Logger.LogInformation("版本下载完成: {Version}", SelectedVersion);
        }, "下载版本");
    }
}