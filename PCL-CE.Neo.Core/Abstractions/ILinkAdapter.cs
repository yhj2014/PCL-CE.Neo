namespace PCL_CE.Neo.Core.Abstractions;

public interface IResourceDownloadAdapter
{
    Task<ResourceDownloadResult> DownloadVersionAsync(string versionId, IProgress<double>? progress = null);
    Task<ResourceDownloadResult> DownloadAssetAsync(string assetIndex, string assetPath, IProgress<double>? progress = null);
    Task<ResourceDownloadResult> DownloadLibraryAsync(string libraryPath, IProgress<double>? progress = null);
    Task<ResourceDownloadResult> DownloadNativesAsync(string versionId, IProgress<double>? progress = null);

    Task<string?> GetVersionManifestAsync();
    Task<string?> GetAssetIndexAsync(string assetId);
}

public record ResourceDownloadResult
{
    public bool Success { get; init; }
    public required string Path { get; init; }
    public long Size { get; init; }
    public string? Hash { get; init; }
    public string? Error { get; init; }
    public Exception? Exception { get; init; }

    public static ResourceDownloadResult Succeeded(string path, long size, string? hash = null) =>
        new() { Success = true, Path = path, Size = size, Hash = hash };
    public static ResourceDownloadResult Failed(string path, string error, Exception? ex = null) =>
        new() { Success = false, Path = path, Error = error, Exception = ex };
}

public interface ILinkAdapter
{
    event Action<LinkState>? StateChanged;
    event Action<string>? MessageReceived;
    event Action<PlayerInfo>? PlayerJoined;
    event Action<PlayerInfo>? PlayerLeft;

    LinkState CurrentState { get; }
    string? RoomCode { get; }
    IReadOnlyList<PlayerInfo> Players { get; }

    Task<string> CreateRoomAsync();
    Task<bool> JoinRoomAsync(string roomCode);
    Task LeaveRoomAsync();

    Task SendMessageAsync(string message);
    Task RequestSyncAsync();
}

public enum LinkState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting,
    Error
}

public record PlayerInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? AvatarUrl { get; init; }
    public bool IsHost { get; init; }
    public PlayerState State { get; init; }
    public double Latency { get; init; }
}

public enum PlayerState
{
    Connected,
    Syncing,
    Ready,
    InGame,
    Disconnected
}

public interface IEasyTierAdapter
{
    event Action<ETState>? StateChanged;
    event Action<ETPeerInfo>? PeerConnected;
    event Action<ETPeerInfo>? PeerDisconnected;
    event Action<ETPeerInfo, byte[]>? DataReceived;

    bool IsRunning { get; }
    string? NodeId { get; }

    Task StartAsync();
    Task StopAsync();
    Task<ETPeerInfo?> ConnectToPeerAsync(string peerId);
    Task DisconnectPeerAsync(string peerId);
    Task SendDataAsync(string peerId, byte[] data);
}

public enum ETState
{
    Stopped,
    Starting,
    Running,
    Error
}

public record ETPeerInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public ETPeerType Type { get; init; }
    public string? PublicKey { get; init; }
    public double Latency { get; init; }
}

public enum ETPeerType
{
    Unknown,
    Relay,
    PublicNode,
    Private
}
