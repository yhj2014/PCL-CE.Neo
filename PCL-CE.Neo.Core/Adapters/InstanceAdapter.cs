using System.Text.Json;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class InstanceAdapter : IInstanceAdapter
{
    private readonly ILogger<InstanceAdapter> _logger;
    private readonly IPathsAdapter _paths;
    private readonly IDatabaseAdapter _database;
    private readonly string _instancesPath;

    public event Action<GameInstance>? InstanceAdded;
    public event Action<GameInstance>? InstanceRemoved;
    public event Action<GameInstance>? InstanceUpdated;

    public IReadOnlyList<GameInstance> Instances { get; private set; } = new List<GameInstance>();

    public InstanceAdapter(
        ILogger<InstanceAdapter> logger,
        IPathsAdapter paths,
        IDatabaseAdapter database)
    {
        _logger = logger;
        _paths = paths;
        _database = database;
        _instancesPath = Path.Combine(paths.Data, "instances");
    }

    public async Task<GameInstance?> GetInstanceAsync(string id)
    {
        return await _database.GetAsync<GameInstance>("instances", id);
    }

    public async Task<IReadOnlyList<GameInstance>> GetAllInstancesAsync()
    {
        var instances = await _database.GetAllAsync<GameInstance>("instances");
        Instances = instances.ToList();
        return Instances;
    }

    public async Task<GameInstance> CreateInstanceAsync(CreateInstanceOptions options)
    {
        var id = Guid.NewGuid().ToString();
        var instance = new GameInstance
        {
            Id = id,
            Name = options.Name,
            Folder = Path.Combine(_instancesPath, id),
            MinecraftVersion = options.MinecraftVersion,
            LoaderType = options.LoaderType,
            LoaderVersion = options.LoaderVersion,
            CreatedAt = DateTime.Now,
            State = InstanceState.Ready
        };

        Directory.CreateDirectory(instance.Folder);

        if (options.SourceInstanceId != null)
        {
            await CloneFromSourceAsync(instance, options.SourceInstanceId);
        }
        else if (options.ModpackPath != null)
        {
            await InstallModpackAsync(instance, options.ModpackPath);
        }

        await _database.InsertAsync("instances", id, instance);
        InstanceAdded?.Invoke(instance);
        _logger.LogInformation("创建实例: {Name} ({Id})", options.Name, id);

        return instance;
    }

    public async Task<bool> UpdateInstanceAsync(GameInstance instance)
    {
        var updated = await _database.UpdateAsync("instances", instance.Id, instance);
        if (updated)
        {
            InstanceUpdated?.Invoke(instance);
            _logger.LogDebug("更新实例: {Name}", instance.Name);
        }
        return updated;
    }

    public async Task<bool> DeleteInstanceAsync(string id)
    {
        var instance = await GetInstanceAsync(id);
        if (instance == null) return false;

        if (Directory.Exists(instance.Folder))
        {
            Directory.Delete(instance.Folder, recursive: true);
        }

        var deleted = await _database.DeleteAsync("instances", id);
        if (deleted)
        {
            InstanceRemoved?.Invoke(instance);
            _logger.LogInformation("删除实例: {Name}", instance.Name);
        }
        return deleted;
    }

    public async Task<GameInstance?> CloneInstanceAsync(string sourceId, string newName)
    {
        var source = await GetInstanceAsync(sourceId);
        if (source == null) return null;

        var options = new CreateInstanceOptions
        {
            Name = newName,
            MinecraftVersion = source.MinecraftVersion,
            LoaderType = source.LoaderType,
            LoaderVersion = source.LoaderVersion,
            SourceInstanceId = sourceId
        };

        return await CreateInstanceAsync(options);
    }

    public async Task<string> GetInstanceStateAsync(string id)
    {
        return await _database.GetAsync<string>("instance_states", id) ?? "";
    }

    public async Task SetInstanceStateAsync(string id, string state)
    {
        await _database.InsertAsync("instance_states", id, state);
    }

    private async Task CloneFromSourceAsync(GameInstance target, string sourceId)
    {
        var source = await GetInstanceAsync(sourceId);
        if (source == null || !Directory.Exists(source.Folder)) return;

        await Task.Run(() =>
        {
            foreach (var file in Directory.GetFiles(source.Folder, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(source.Folder, file);
                var targetPath = Path.Combine(target.Folder, relativePath);
                var targetDir = Path.GetDirectoryName(targetPath);

                if (targetDir != null)
                {
                    Directory.CreateDirectory(targetDir);
                }

                File.Copy(file, targetPath, overwrite: true);
            }
        });

        _logger.LogDebug("从 {Source} 克隆到 {Target}", source.Name, target.Name);
    }

    private async Task InstallModpackAsync(GameInstance instance, string modpackPath)
    {
        _logger.LogInformation("从 {Path} 安装 modpack 到 {Instance}", modpackPath, instance.Name);
        await Task.CompletedTask;
    }
}
