namespace PCL_CE.Neo.Core.Abstractions;

public interface IModAdapter
{
    event Action<ModInfo>? ModAdded;
    event Action<ModInfo>? ModRemoved;
    event Action<ModInfo>? ModUpdated;

    Task<IReadOnlyList<ModInfo>> GetModsAsync(string instanceId);
    Task<ModInfo?> GetModAsync(string instanceId, string modId);
    Task<bool> AddModAsync(string instanceId, string filePath);
    Task<bool> RemoveModAsync(string instanceId, string modId);
    Task<bool> UpdateModAsync(string instanceId, ModInfo mod);
    Task<bool> ToggleModAsync(string instanceId, string modId, bool enabled);

    Task<IReadOnlyList<ModInfo>> SearchModsAsync(ModSearchQuery query);
    Task<DownloadResult> DownloadModAsync(ModDownloadRequest request, CancellationToken cancellationToken = default);

    Task<Dictionary<string, ModUpdateInfo>> CheckUpdatesAsync(string instanceId);
}

public record ModInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string FilePath { get; init; }
    public required ModSource Source { get; init; }
    public string? RemoteId { get; init; }
    public string? Version { get; init; }
    public string? LoaderType { get; init; }
    public bool IsEnabled { get; init; } = true;
    public ModState State { get; init; } = ModState.Ready;

    public long FileSize { get; init; }
    public string? FileHash { get; init; }
    public DateTime? LastUpdated { get; init; }
}

public enum ModSource
{
    Local,
    CurseForge,
    Modrinth
}

public enum ModState
{
    Ready,
    Downloading,
    Updating,
    Corrupted
}

public record ModSearchQuery
{
    public string? Query { get; init; }
    public string? MinecraftVersion { get; init; }
    public string? LoaderType { get; init; }
    public ModSource? Source { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public ModSearchSortBy SortBy { get; init; } = ModSearchSortBy.Relevance;
}

public enum ModSearchSortBy
{
    Relevance,
    Popularity,
    UpdatedDate,
    CreateDate,
    Name
}

public record ModDownloadRequest
{
    public required string Url { get; init; }
    public required string InstanceId { get; init; }
    public required ModSource Source { get; init; }
    public required string ModId { get; init; }
    public string? ExpectedHash { get; init; }
}

public record ModUpdateInfo
{
    public required string ModId { get; init; }
    public string? CurrentVersion { get; init; }
    public string? LatestVersion { get; init; }
    public string? DownloadUrl { get; init; }
    public bool IsUpdateAvailable { get; init; }
}
