namespace PCL_CE.Neo.Core.Abstractions;

public interface IInstanceAdapter
{
    IReadOnlyList<GameInstance> Instances { get; }
    event Action<GameInstance>? InstanceAdded;
    event Action<GameInstance>? InstanceRemoved;
    event Action<GameInstance>? InstanceUpdated;

    Task<GameInstance?> GetInstanceAsync(string id);
    Task<IReadOnlyList<GameInstance>> GetAllInstancesAsync();
    Task<GameInstance> CreateInstanceAsync(CreateInstanceOptions options);
    Task<bool> UpdateInstanceAsync(GameInstance instance);
    Task<bool> DeleteInstanceAsync(string id);
    Task<GameInstance?> CloneInstanceAsync(string sourceId, string newName);

    Task<string> GetInstanceStateAsync(string id);
    Task SetInstanceStateAsync(string id, string state);
}

public record GameInstance
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Folder { get; init; }
    public required string MinecraftVersion { get; init; }
    public string? LoaderType { get; init; }
    public string? LoaderVersion { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime? LastPlayedAt { get; init; }
    public int PlayCount { get; init; }
    public bool IsStarred { get; init; }
    public InstanceState State { get; init; }

    public string? IconPath { get; init; }
    public string? BackgroundPath { get; init; }

    public Dictionary<string, string> CustomSettings { get; init; } = new();
}

public enum InstanceState
{
    Ready,
    Installing,
    Downloading,
    Launching,
    Running,
    Corrupted,
    Hidden
}

public record CreateInstanceOptions
{
    public required string Name { get; init; }
    public required string MinecraftVersion { get; init; }
    public string? LoaderType { get; init; }
    public string? LoaderVersion { get; init; }
    public string? SourceInstanceId { get; init; }
    public string? ModpackPath { get; init; }
}
