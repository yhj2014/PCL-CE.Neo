namespace PCL_CE.Neo.Core.Update;

public enum UpdateChannel
{
    Stable,
    Beta
}

public enum UpdateStatus
{
    Unknown,
    Latest,
    NotLatest,
    UpdateAvailable,
    Downloaded,
    Installed
}

public record VersionInfo
{
    public required string VersionName { get; init; }
    public required string VersionCode { get; init; }
    public required string DownloadUrl { get; init; }
    public string? SHA256 { get; init; }
    public long Size { get; init; }
    public string? Changelog { get; init; }
    public DateTime? ReleaseDate { get; init; }
    public UpdateChannel Channel { get; init; }
}

public interface IUpdateService
{
    event Action<UpdateStatus>? StatusChanged;
    
    UpdateStatus CurrentStatus { get; }
    VersionInfo? LatestVersion { get; }
    VersionInfo? DownloadedVersion { get; }
    
    bool IsUpdateWaitingRestart { get; }
    
    Task<UpdateStatus> CheckForUpdateAsync(CancellationToken cancellationToken = default);
    Task<bool> DownloadUpdateAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default);
    Task<bool> InstallUpdateAsync(CancellationToken cancellationToken = default);
    void SetUpdateChannel(UpdateChannel channel);
    void SetAutoUpdateBehavior(AutoUpdateBehavior behavior);
}

public enum AutoUpdateBehavior
{
    None,
    CheckOnly,
    DownloadOnly,
    DownloadAndPrompt,
    DownloadAndInstall
}

public interface IUpdateSource
{
    string Name { get; }
    Task<VersionInfo?> GetLatestVersionAsync(UpdateChannel channel, string arch);
    Task<bool> IsLatestAsync(UpdateChannel channel, string arch, string currentVersion, long currentBuild);
    Task<byte[]?> DownloadVersionAsync(VersionInfo version, IProgress<double>? progress = null, CancellationToken cancellationToken = default);
}
