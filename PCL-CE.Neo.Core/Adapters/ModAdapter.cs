using System.Text.Json;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class ModAdapter : IModAdapter
{
    private readonly ILogger<ModAdapter> _logger;
    private readonly IPathsAdapter _paths;
    private readonly IDatabaseAdapter _database;
    private readonly IDownloadAdapter _download;
    private readonly INetworkAdapter _network;

    public event Action<ModInfo>? ModAdded;
    public event Action<ModInfo>? ModRemoved;
    public event Action<ModInfo>? ModUpdated;

    public ModAdapter(
        ILogger<ModAdapter> logger,
        IPathsAdapter paths,
        IDatabaseAdapter database,
        IDownloadAdapter download,
        INetworkAdapter network)
    {
        _logger = logger;
        _paths = paths;
        _database = database;
        _download = download;
        _network = network;
    }

    public async Task<IReadOnlyList<ModInfo>> GetModsAsync(string instanceId)
    {
        var mods = await _database.GetAllAsync<ModInfo>($"instance_{instanceId}_mods");
        return mods.ToList();
    }

    public async Task<ModInfo?> GetModAsync(string instanceId, string modId)
    {
        return await _database.GetAsync<ModInfo>($"instance_{instanceId}_mods", modId);
    }

    public async Task<bool> AddModAsync(string instanceId, string filePath)
    {
        try
        {
            var fileName = Path.GetFileName(filePath);
            var modsFolder = Path.Combine(_paths.Data, "instances", instanceId, "mods");
            Directory.CreateDirectory(modsFolder);

            var targetPath = Path.Combine(modsFolder, fileName);
            File.Copy(filePath, targetPath, overwrite: true);

            var modInfo = new ModInfo
            {
                Id = Guid.NewGuid().ToString(),
                Name = Path.GetFileNameWithoutExtension(fileName),
                FilePath = targetPath,
                Source = ModSource.Local,
                IsEnabled = true,
                FileSize = new FileInfo(targetPath).Length
            };

            await _database.InsertAsync($"instance_{instanceId}_mods", modInfo.Id, modInfo);
            ModAdded?.Invoke(modInfo);

            _logger.LogInformation("添加 Mod: {Name}", modInfo.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加 Mod 失败: {Path}", filePath);
            return false;
        }
    }

    public async Task<bool> RemoveModAsync(string instanceId, string modId)
    {
        var mod = await GetModAsync(instanceId, modId);
        if (mod == null) return false;

        if (File.Exists(mod.FilePath))
        {
            File.Delete(mod.FilePath);
        }

        var deleted = await _database.DeleteAsync($"instance_{instanceId}_mods", modId);
        if (deleted)
        {
            ModRemoved?.Invoke(mod);
            _logger.LogInformation("移除 Mod: {Name}", mod.Name);
        }
        return deleted;
    }

    public async Task<bool> UpdateModAsync(string instanceId, ModInfo mod)
    {
        var updated = await _database.UpdateAsync($"instance_{instanceId}_mods", mod.Id, mod);
        if (updated)
        {
            ModUpdated?.Invoke(mod);
        }
        return updated;
    }

    public async Task<bool> ToggleModAsync(string instanceId, string modId, bool enabled)
    {
        var mod = await GetModAsync(instanceId, modId);
        if (mod == null) return false;

        var updatedMod = mod with { IsEnabled = enabled };
        return await UpdateModAsync(instanceId, updatedMod);
    }

    public async Task<IReadOnlyList<ModInfo>> SearchModsAsync(ModSearchQuery query)
    {
        try
        {
            if (query.Source == ModSource.Modrinth || query.Source == null)
            {
                return await SearchModrinthAsync(query);
            }
            else if (query.Source == ModSource.CurseForge)
            {
                return await SearchCurseForgeAsync(query);
            }

            return new List<ModInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "搜索 Mod 失败");
            return new List<ModInfo>();
        }
    }

    public async Task<DownloadResult> DownloadModAsync(ModDownloadRequest request, CancellationToken cancellationToken = default)
    {
        var modsFolder = Path.Combine(_paths.Data, "instances", request.InstanceId, "mods");
        Directory.CreateDirectory(modsFolder);

        var fileName = Path.GetFileName(new Uri(request.Url).LocalPath);
        if (string.IsNullOrEmpty(fileName))
        {
            fileName = $"{request.ModId}_{Guid.NewGuid():N}.jar";
        }

        var targetPath = Path.Combine(modsFolder, fileName);

        var downloadRequest = new Abstractions.DownloadRequest
        {
            Url = request.Url,
            DestinationPath = targetPath,
            ExpectedHash = request.ExpectedHash
        };

        var result = await _download.DownloadFileAsync(downloadRequest, cancellationToken);

        if (result.Success)
        {
            var modInfo = new ModInfo
            {
                Id = request.ModId,
                Name = Path.GetFileNameWithoutExtension(fileName),
                FilePath = targetPath,
                Source = request.Source,
                RemoteId = request.ModId,
                FileSize = result.BytesDownloaded,
                FileHash = request.ExpectedHash
            };

            await _database.InsertAsync($"instance_{request.InstanceId}_mods", modInfo.Id, modInfo);
            ModAdded?.Invoke(modInfo);
        }

        return result;
    }

    public async Task<Dictionary<string, ModUpdateInfo>> CheckUpdatesAsync(string instanceId)
    {
        var updates = new Dictionary<string, ModUpdateInfo>();
        var mods = await GetModsAsync(instanceId);

        foreach (var mod in mods.Where(m => m.Source != ModSource.Local))
        {
            updates[mod.Id] = new ModUpdateInfo
            {
                ModId = mod.Id,
                CurrentVersion = mod.Version,
                IsUpdateAvailable = false
            };
        }

        return updates;
    }

    private async Task<IReadOnlyList<ModInfo>> SearchModrinthAsync(ModSearchQuery query)
    {
        var url = $"https://api.modrinth.com/v2/search?query={Uri.EscapeDataString(query.Query ?? "")}&limit={query.PageSize}";

        if (!string.IsNullOrEmpty(query.MinecraftVersion))
        {
            url += $"&game_version={Uri.EscapeDataString(query.MinecraftVersion)}";
        }

        if (!string.IsNullOrEmpty(query.LoaderType))
        {
            url += $"&facets=[[\"categories:{query.LoaderType.ToLower()}\"]]";
        }

        var response = await _network.GetAsync(url);
        var result = JsonSerializer.Deserialize<ModrinthSearchResult>(response);

        return result?.Hits.Select(h => new ModInfo
        {
            Id = h.ModId,
            Name = h.Title,
            FilePath = "",
            Source = ModSource.Modrinth,
            RemoteId = h.ModId,
            Version = h.LatestVersion
        }).ToList() ?? new List<ModInfo>();
    }

    private async Task<IReadOnlyList<ModInfo>> SearchCurseForgeAsync(ModSearchQuery query)
    {
        await Task.CompletedTask;
        return new List<ModInfo>();
    }

    private class ModrinthSearchResult
    {
        public List<ModrinthHit> Hits { get; set; } = new();
    }

    private class ModrinthHit
    {
        public string ModId { get; set; } = "";
        public string Title { get; set; } = "";
        public string LatestVersion { get; set; } = "";
    }
}
