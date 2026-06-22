using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class InstanceAdapter : IInstanceAdapter
{
    private readonly ILogger<InstanceAdapter> _logger;
    private readonly IPathsAdapter _paths;
    private readonly IDatabaseAdapter _database;
    private readonly IDownloadAdapter _download;
    private readonly string _instancesPath;

    public event Action<GameInstance>? InstanceAdded;
    public event Action<GameInstance>? InstanceRemoved;
    public event Action<GameInstance>? InstanceUpdated;

    public IReadOnlyList<GameInstance> Instances { get; private set; } = new List<GameInstance>();

    public InstanceAdapter(
        ILogger<InstanceAdapter> logger,
        IPathsAdapter paths,
        IDatabaseAdapter database) : this(logger, paths, database, new DownloadAdapter())
    {
    }

    public InstanceAdapter(
        ILogger<InstanceAdapter> logger,
        IPathsAdapter paths,
        IDatabaseAdapter database,
        IDownloadAdapter download)
    {
        _logger = logger;
        _paths = paths;
        _database = database;
        _download = download;
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

        try
        {
            if (!File.Exists(modpackPath))
            {
                _logger.LogError("Modpack 文件不存在: {Path}", modpackPath);
                return;
            }

            // Detect modpack type and install
            var packType = await DetectModpackTypeAsync(modpackPath);
            _logger.LogInformation("检测到 modpack 类型: {PackType}", packType);

            switch (packType)
            {
                case ModpackType.CurseForge:
                    await InstallCurseForgeModpackAsync(instance, modpackPath);
                    break;
                case ModpackType.Modrinth:
                    await InstallModrinthModpackAsync(instance, modpackPath);
                    break;
                case ModpackType.MCBBS:
                    await InstallMCBBSModpackAsync(instance, modpackPath);
                    break;
                case ModpackType.MMC:
                    await InstallMMCModpackAsync(instance, modpackPath);
                    break;
                case ModpackType.HMCL:
                    await InstallHMCLModpackAsync(instance, modpackPath);
                    break;
                case ModpackType.BasicZip:
                    await InstallBasicZipModpackAsync(instance, modpackPath);
                    break;
                default:
                    _logger.LogWarning("不支持的 modpack 类型: {PackType}", packType);
                    break;
            }

            _logger.LogInformation("Modpack 安装完成: {Instance}", instance.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "安装 modpack 失败: {Path}", modpackPath);
        }
    }

    private async Task<ModpackType> DetectModpackTypeAsync(string modpackPath)
    {
        return await Task.Run(() =>
        {
            using var archive = ZipFile.OpenRead(modpackPath);
            var entryNames = archive.Entries.Select(e => e.FullName).ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Check for MCBBS modpack
            if (entryNames.Contains("mcbbs.packmeta"))
                return ModpackType.MCBBS;

            // Check for MMC modpack
            if (entryNames.Contains("mmc-pack.json"))
                return ModpackType.MMC;

            // Check for Modrinth modpack
            if (entryNames.Contains("modrinth.index.json"))
                return ModpackType.Modrinth;

            // Check for CurseForge modpack (manifest.json without addons)
            var manifestEntry = archive.GetEntry("manifest.json");
            if (manifestEntry != null)
            {
                using var stream = manifestEntry.Open();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var manifestContent = reader.ReadToEnd();
                using var doc = JsonDocument.Parse(manifestContent);
                if (doc.RootElement.TryGetProperty("addons", out var _) == false)
                    return ModpackType.CurseForge;
                return ModpackType.MCBBS; // manifest.json with addons is MCBBS
            }

            // Check for HMCL modpack
            if (entryNames.Contains("modpack.json"))
                return ModpackType.HMCL;

            // Check for overrides in root
            if (entryNames.Any(e => e.StartsWith("overrides/", StringComparison.OrdinalIgnoreCase)))
                return ModpackType.BasicZip;

            return ModpackType.BasicZip;
        });
    }

    private async Task InstallCurseForgeModpackAsync(GameInstance instance, string modpackPath)
    {
        _logger.LogInformation("安装 CurseForge modpack: {Instance}", instance.Name);

        await Task.Run(() =>
        {
            using var archive = ZipFile.OpenRead(modpackPath);
            
            // Read manifest.json
            var manifestEntry = archive.GetEntry("manifest.json");
            if (manifestEntry == null)
            {
                _logger.LogWarning("未找到 manifest.json");
                return;
            }

            using var stream = manifestEntry.Open();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var manifestContent = reader.ReadToEnd();
            using var doc = JsonDocument.Parse(manifestContent);

            // Extract minecraft version and mod loader info
            if (doc.RootElement.TryGetProperty("minecraft", out var minecraft))
            {
                var version = minecraft.TryGetProperty("version", out var v) ? v.GetString() : null;
                var modLoader = minecraft.TryGetProperty("modLoaders", out var loaders) 
                    ? loaders.EnumerateArray().FirstOrDefault().GetString() : null;
                
                _logger.LogDebug("CurseForge modpack: Minecraft {Version}, Loader: {Loader}", version, modLoader);
            }

            // Extract overrides (game files)
            var overridesEntry = archive.GetEntry("overrides/");
            if (overridesEntry != null)
            {
                var overridesDir = Path.Combine(instance.Folder, "overrides");
                archive.ExtractToDirectory(overridesDir, overwrite: true);
                MoveOverridesToRoot(instance.Folder, overridesDir);
            }

            // Extract mod files
            var modsEntry = archive.GetEntry("mods/");
            if (modsEntry != null)
            {
                var modsDir = Path.Combine(instance.Folder, "mods");
                archive.ExtractToDirectory(modsDir, overwrite: true);
            }
        });

        await Task.CompletedTask;
    }

    private async Task InstallModrinthModpackAsync(GameInstance instance, string modpackPath)
    {
        _logger.LogInformation("安装 Modrinth modpack: {Instance}", instance.Name);

        await Task.Run(() =>
        {
            using var archive = ZipFile.OpenRead(modpackPath);
            
            // Modrinth modpacks use .mrpack format which is a zip with modrinth.index.json
            var indexEntry = archive.GetEntry("modrinth.index.json");
            if (indexEntry == null)
            {
                _logger.LogWarning("未找到 modrinth.index.json");
                return;
            }

            using var stream = indexEntry.Open();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var indexContent = reader.ReadToEnd();
            using var doc = JsonDocument.Parse(indexContent);

            // Get game version and loader
            var gameVersion = doc.RootElement.TryGetProperty("game", out var gv) ? gv.GetString() : null;
            var loader = doc.RootElement.TryGetProperty("loader", out var l) ? l.GetString() : null;
            
            _logger.LogDebug("Modrinth modpack: Game {Version}, Loader: {Loader}", gameVersion, loader);

            // Find and extract overrides
            var overridesEntry = archive.GetEntry("overrides/");
            if (overridesEntry != null)
            {
                var overridesDir = Path.Combine(instance.Folder, "overrides");
                archive.ExtractToDirectory(overridesDir, overwrite: true);
                MoveOverridesToRoot(instance.Folder, overridesDir);
            }

            // Modrinth modpacks may have a "files" directory for mods
            var filesEntry = archive.GetEntry("files/");
            if (filesEntry != null)
            {
                var modsDir = Path.Combine(instance.Folder, "mods");
                archive.ExtractToDirectory(modsDir, overwrite: true);
            }
        });

        await Task.CompletedTask;
    }

    private async Task InstallMCBBSModpackAsync(GameInstance instance, string modpackPath)
    {
        _logger.LogInformation("安装 MCBBS modpack: {Instance}", instance.Name);

        await Task.Run(() =>
        {
            using var archive = ZipFile.OpenRead(modpackPath);
            
            var packmetaEntry = archive.GetEntry("mcbbs.packmeta");
            if (packmetaEntry != null)
            {
                using var stream = packmetaEntry.Open();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var content = reader.ReadToEnd();
                _logger.LogDebug("MCBBS packmeta: {Content}", content);
            }

            // MCBBS modpacks typically have overrides
            var overridesEntry = archive.GetEntry("overrides/");
            if (overridesEntry != null)
            {
                var overridesDir = Path.Combine(instance.Folder, "overrides");
                archive.ExtractToDirectory(overridesDir, overwrite: true);
                MoveOverridesToRoot(instance.Folder, overridesDir);
            }

            // Also check for manifest.json
            var manifestEntry = archive.GetEntry("manifest.json");
            if (manifestEntry != null)
            {
                using var stream = manifestEntry.Open();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var manifestContent = reader.ReadToEnd();
                using var doc = JsonDocument.Parse(manifestContent);
                
                if (doc.RootElement.TryGetProperty("minecraft", out var minecraft))
                {
                    var version = minecraft.TryGetProperty("version", out var v) ? v.GetString() : null;
                    _logger.LogDebug("MCBBS modpack Minecraft version: {Version}", version);
                }
            }
        });

        await Task.CompletedTask;
    }

    private async Task InstallMMCModpackAsync(GameInstance instance, string modpackPath)
    {
        _logger.LogInformation("安装 MMC modpack: {Instance}", instance.Name);

        await Task.Run(() =>
        {
            using var archive = ZipFile.OpenRead(modpackPath);
            
            var mmcPackEntry = archive.GetEntry("mmc-pack.json");
            if (mmcPackEntry == null)
            {
                _logger.LogWarning("未找到 mmc-pack.json");
                return;
            }

            using var stream = mmcPackEntry.Open();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var content = reader.ReadToEnd();
            _logger.LogDebug("MMC pack: {Content}", content);

            // MMC modpacks have a components list
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("components", out var components))
            {
                foreach (var component in components.EnumerateArray())
                {
                    var uid = component.TryGetProperty("uid", out var u) ? u.GetString() : null;
                    var version = component.TryGetProperty("version", out var ver) ? ver.GetString() : null;
                    _logger.LogDebug("MMC component: {Uid} {Version}", uid, version);
                }
            }

            // Extract overrides
            var overridesEntry = archive.GetEntry("overrides/");
            if (overridesEntry != null)
            {
                var overridesDir = Path.Combine(instance.Folder, "overrides");
                archive.ExtractToDirectory(overridesDir, overwrite: true);
                MoveOverridesToRoot(instance.Folder, overridesDir);
            }
        });

        await Task.CompletedTask;
    }

    private async Task InstallHMCLModpackAsync(GameInstance instance, string modpackPath)
    {
        _logger.LogInformation("安装 HMCL modpack: {Instance}", instance.Name);

        await Task.Run(() =>
        {
            using var archive = ZipFile.OpenRead(modpackPath);
            
            var modpackJsonEntry = archive.GetEntry("modpack.json");
            if (modpackJsonEntry == null)
            {
                _logger.LogWarning("未找到 modpack.json");
                return;
            }

            using var stream = modpackJsonEntry.Open();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var content = reader.ReadToEnd();
            _logger.LogDebug("HMCL modpack: {Content}", content);

            // HMCL modpacks typically have an overrides directory
            var overridesEntry = archive.GetEntry("overrides/");
            if (overridesEntry != null)
            {
                var overridesDir = Path.Combine(instance.Folder, "overrides");
                archive.ExtractToDirectory(overridesDir, overwrite: true);
                MoveOverridesToRoot(instance.Folder, overridesDir);
            }
        });

        await Task.CompletedTask;
    }

    private async Task InstallBasicZipModpackAsync(GameInstance instance, string modpackPath)
    {
        _logger.LogInformation("安装 Basic ZIP modpack: {Instance}", instance.Name);

        await Task.Run(() =>
        {
            using var archive = ZipFile.OpenRead(modpackPath);
            
            // Just extract everything to the instance folder
            archive.ExtractToDirectory(instance.Folder, overwrite: true);
            _logger.LogDebug("已解压基础 ZIP modpack 到 {Folder}", instance.Folder);
        });

        await Task.CompletedTask;
    }

    private void MoveOverridesToRoot(string instanceFolder, string overridesDir)
    {
        if (!Directory.Exists(overridesDir))
            return;

        try
        {
            foreach (var file in Directory.GetFiles(overridesDir, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(overridesDir, file);
                var targetPath = Path.Combine(instanceFolder, relativePath);
                var targetDir = Path.GetDirectoryName(targetPath);

                if (targetDir != null)
                {
                    Directory.CreateDirectory(targetDir);
                }

                File.Copy(file, targetPath, overwrite: true);
            }

            // Clean up the overrides directory
            Directory.Delete(overridesDir, recursive: true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "移动 overrides 文件失败");
        }
    }

    private enum ModpackType
    {
        Unknown = -1,
        CurseForge = 0,
        MCBBS = 1,
        HMCL = 2,
        MMC = 3,
        Modrinth = 4,
        BasicZip = 9
    }
}
