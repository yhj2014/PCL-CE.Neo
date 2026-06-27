using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft;

/// <summary>
/// 游戏版本类型
/// </summary>
public enum GameVersionType
{
    Release,
    Snapshot,
    OldAlpha,
    OldBeta
}

/// <summary>
/// 游戏版本信息
/// </summary>
public record GameVersion(
    string Id,
    string Name,
    GameVersionType Type,
    DateTime ReleaseDate,
    string? ParentId = null,
    string? AssetIndexId = null,
    string? MainClass = null
);

/// <summary>
/// 游戏核心信息
/// </summary>
public record GameCoreInfo(
    string Id,
    string Name,
    string Type,
    bool IsModLoader,
    string? ModLoaderName = null,
    string? ModLoaderVersion = null,
    string Path = ""
);

/// <summary>
/// 游戏实例信息
/// </summary>
public record GameInstance(
    string Id,
    string Name,
    string GameCoreId,
    string? JavaPath,
    int MaxMemory,
    int MinMemory,
    string? JvmArguments,
    string? GameDirectory,
    string? AssetsDirectory,
    string? VersionDirectory
)
{
    public string WorkingDirectory => GameDirectory ?? Path.Combine(GetDefaultGameDir(), Id);
    
    public static string GetDefaultGameDir() 
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".minecraft"
        );
    }
}

/// <summary>
/// 游戏核心类，用于处理 Minecraft JAR 文件
/// </summary>
public sealed class GameCore
{
    private readonly ILogger<GameCore>? _logger;
    private readonly string _corePath;

    public GameCore(string corePath, ILogger<GameCore>? logger = null)
    {
        if (!File.Exists(corePath))
            throw new FileNotFoundException($"未找到指定文件：{corePath}");
        
        _corePath = corePath;
        _logger = logger;
    }

    /// <summary>
    /// 获取核心文件路径
    /// </summary>
    public string CorePath => _corePath;

    /// <summary>
    /// 将指定的 Jar 文件添加到游戏核心
    /// </summary>
    /// <param name="jarPath">要添加的 Jar 文件</param>
    /// <exception cref="FileNotFoundException">提供的文件路径不存在</exception>
    public void AddToCore(string jarPath)
    {
        if (!File.Exists(jarPath))
            throw new FileNotFoundException($"未找到指定文件：{jarPath}");
        
        _logger?.LogInformation("正在将 {JarPath} 添加到游戏核心 {CorePath}", jarPath, _corePath);
        
        try
        {
            using var coreStream = new FileStream(_corePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 16384, true);
            using var jarStream = new FileStream(jarPath, FileMode.Open, FileAccess.Read, FileShare.Read, 16384, true);
            using var coreArchive = new ZipArchive(coreStream, ZipArchiveMode.Update);
            using var jarArchive = new ZipArchive(jarStream);
            
            // Better Than Wolves 的 Mod File 是 .zip 结尾的
            var filter = jarPath.EndsWith(".jar") ? "" : "MINECRAFT-JAR";
            var addedCount = 0;
            
            foreach (var entry in jarArchive.Entries)
            {
                if (!entry.FullName.Contains(filter))
                    continue;
                
                try
                {
                    // 删除已存在的条目
                    var existingEntry = coreArchive.GetEntry(entry.FullName);
                    if (existingEntry != null)
                        existingEntry.Delete();
                    
                    // 创建新条目
                    using var coreArchiveStream = coreArchive.CreateEntry(entry.FullName).Open();
                    using var jarArchiveStream = entry.Open();
                    jarArchiveStream.CopyTo(coreArchiveStream);
                    
                    addedCount++;
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "添加条目 {EntryName} 失败", entry.FullName);
                }
            }
            
            // 删除包含签名文件的目录，避免 Oracle JDK 加载时验证签名失败导致无法启动
            try
            {
                var metaInfEntries = coreArchive.Entries
                    .Where(e => e.FullName.StartsWith("META-INF/"))
                    .ToList();
                
                foreach (var metaEntry in metaInfEntries)
                {
                    metaEntry.Delete();
                }
                
                _logger?.LogDebug("已删除 {Count} 个 META-INF 条目", metaInfEntries.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "删除 META-INF 条目失败");
            }
            
            _logger?.LogInformation("已添加 {Count} 个条目到游戏核心", addedCount);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "添加文件到游戏核心失败");
            throw;
        }
    }

    /// <summary>
    /// 批量添加 Jar 文件到游戏核心
    /// </summary>
    /// <param name="jarPaths">Jar 文件路径列表</param>
    public void AddMultipleToCore(IEnumerable<string> jarPaths)
    {
        foreach (var jarPath in jarPaths)
        {
            AddToCore(jarPath);
        }
    }

    /// <summary>
    /// 从核心中移除指定条目
    /// </summary>
    /// <param name="entryName">条目名称</param>
    public void RemoveEntry(string entryName)
    {
        try
        {
            using var coreStream = new FileStream(_corePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            using var coreArchive = new ZipArchive(coreStream, ZipArchiveMode.Update);
            
            var entry = coreArchive.GetEntry(entryName);
            if (entry != null)
            {
                entry.Delete();
                _logger?.LogInformation("已从核心移除条目: {EntryName}", entryName);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "移除条目失败: {EntryName}", entryName);
            throw;
        }
    }

    /// <summary>
    /// 获取核心中的所有条目名称
    /// </summary>
    public IEnumerable<string> GetEntryNames()
    {
        using var coreStream = new FileStream(_corePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var coreArchive = new ZipArchive(coreStream, ZipArchiveMode.Read);
        
        return coreArchive.Entries.Select(e => e.FullName).ToList();
    }

    /// <summary>
    /// 检查核心是否包含指定条目
    /// </summary>
    public bool HasEntry(string entryName)
    {
        using var coreStream = new FileStream(_corePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var coreArchive = new ZipArchive(coreStream, ZipArchiveMode.Read);
        
        return coreArchive.GetEntry(entryName) != null;
    }
}

/// <summary>
/// 游戏核心服务接口
/// </summary>
public interface IGameCoreService
{
    /// <summary>
    /// 获取所有游戏版本
    /// </summary>
    Task<IReadOnlyList<GameVersion>> GetGameVersionsAsync();
    
    /// <summary>
    /// 获取指定版本的游戏版本信息
    /// </summary>
    Task<GameVersion?> GetGameVersionAsync(string versionId);
    
    /// <summary>
    /// 获取所有游戏实例
    /// </summary>
    Task<IReadOnlyList<GameInstance>> GetGameInstancesAsync();
    
    /// <summary>
    /// 创建新的游戏实例
    /// </summary>
    Task<GameInstance> CreateInstanceAsync(string name, string gameVersionId);
    
    /// <summary>
    /// 删除游戏实例
    /// </summary>
    Task DeleteInstanceAsync(string instanceId);
    
    /// <summary>
    /// 获取游戏核心信息
    /// </summary>
    Task<GameCoreInfo?> GetGameCoreAsync(string coreId);
    
    /// <summary>
    /// 合并 Mod 到游戏核心
    /// </summary>
    Task MergeModsToCoreAsync(string corePath, IEnumerable<string> modPaths);
}

/// <summary>
/// 游戏核心服务实现
/// </summary>
public sealed class GameCoreService : IGameCoreService
{
    private readonly ILogger<GameCoreService> _logger;
    private readonly string _gameDirectory;
    private readonly List<GameInstance> _instances = new();
    private readonly List<GameVersion> _versions = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public GameCoreService(ILogger<GameCoreService> logger, string? gameDirectory = null)
    {
        _logger = logger;
        _gameDirectory = gameDirectory ?? GameInstance.GetDefaultGameDir();
    }

    /// <summary>
    /// 获取所有游戏版本
    /// </summary>
    public async Task<IReadOnlyList<GameVersion>> GetGameVersionsAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (_versions.Count == 0)
            {
                await LoadVersionsFromDirectoryAsync();
            }
            
            return _versions.ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// 获取指定版本的游戏版本信息
    /// </summary>
    public async Task<GameVersion?> GetGameVersionAsync(string versionId)
    {
        var versions = await GetGameVersionsAsync();
        return versions.FirstOrDefault(v => v.Id == versionId);
    }

    /// <summary>
    /// 获取所有游戏实例
    /// </summary>
    public async Task<IReadOnlyList<GameInstance>> GetGameInstancesAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (_instances.Count == 0)
            {
                await LoadInstancesFromDirectoryAsync();
            }
            
            return _instances.ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// 创建新的游戏实例
    /// </summary>
    public async Task<GameInstance> CreateInstanceAsync(string name, string gameVersionId)
    {
        var instance = new GameInstance(
            Id: Guid.NewGuid().ToString("N"),
            Name: name,
            GameCoreId: gameVersionId,
            JavaPath: null,
            MaxMemory: 2048,
            MinMemory: 512,
            JvmArguments: null,
            GameDirectory: Path.Combine(_gameDirectory, name),
            AssetsDirectory: Path.Combine(_gameDirectory, "assets"),
            VersionDirectory: Path.Combine(_gameDirectory, "versions", gameVersionId)
        );
        
        await _lock.WaitAsync();
        try
        {
            // 创建目录
            Directory.CreateDirectory(instance.WorkingDirectory);
            Directory.CreateDirectory(instance.AsmentsDirectory ?? Path.Combine(_gameDirectory, "assets"));
            
            _instances.Add(instance);
            _logger.LogInformation("已创建游戏实例: {Name}", name);
            
            return instance;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// 删除游戏实例
    /// </summary>
    public async Task DeleteInstanceAsync(string instanceId)
    {
        await _lock.WaitAsync();
        try
        {
            var instance = _instances.FirstOrDefault(i => i.Id == instanceId);
            if (instance == null)
            {
                _logger.LogWarning("尝试删除不存在的实例: {InstanceId}", instanceId);
                return;
            }
            
            // 删除目录
            if (Directory.Exists(instance.WorkingDirectory))
            {
                Directory.Delete(instance.WorkingDirectory, true);
            }
            
            _instances.Remove(instance);
            _logger.LogInformation("已删除游戏实例: {Name}", instance.Name);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// 获取游戏核心信息
    /// </summary>
    public async Task<GameCoreInfo?> GetGameCoreAsync(string coreId)
    {
        var versions = await GetGameVersionsAsync();
        var version = versions.FirstOrDefault(v => v.Id == coreId);
        
        if (version == null)
            return null;
        
        var versionDir = Path.Combine(_gameDirectory, "versions", coreId);
        var jarPath = Path.Combine(versionDir, $"{coreId}.jar");
        
        return new GameCoreInfo(
            Id: coreId,
            Name: version.Name,
            Type: version.Type.ToString(),
            IsModLoader: false,
            Path: jarPath
        );
    }

    /// <summary>
    /// 合并 Mod 到游戏核心
    /// </summary>
    public async Task MergeModsToCoreAsync(string corePath, IEnumerable<string> modPaths)
    {
        try
        {
            var gameCore = new GameCore(corePath, _logger as ILogger<GameCore>);
            gameCore.AddMultipleToCore(modPaths);
            
            _logger.LogInformation("已合并 {Count} 个 Mod 到核心", modPaths.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "合并 Mod 到核心失败");
            throw;
        }
    }

    /// <summary>
    /// 从目录加载游戏版本
    /// </summary>
    private async Task LoadVersionsFromDirectoryAsync()
    {
        var versionsDir = Path.Combine(_gameDirectory, "versions");
        
        if (!Directory.Exists(versionsDir))
        {
            _logger.LogWarning("版本目录不存在: {Dir}", versionsDir);
            return;
        }
        
        try
        {
            foreach (var versionDir in Directory.GetDirectories(versionsDir))
            {
                var versionJsonPath = Path.Combine(versionDir, $"{Path.GetFileName(versionDir)}.json");
                
                if (!File.Exists(versionJsonPath))
                    continue;
                
                try
                {
                    var json = await File.ReadAllTextAsync(versionJsonPath);
                    var versionData = JsonSerializer.Deserialize<VersionJsonModel>(json);
                    
                    if (versionData == null || string.IsNullOrEmpty(versionData.Id))
                        continue;
                    
                    var versionType = ParseVersionType(versionData.Type);
                    
                    _versions.Add(new GameVersion(
                        Id: versionData.Id,
                        Name: versionData.Id,
                        Type: versionType,
                        ReleaseDate: versionData.ReleaseTime ?? DateTime.MinValue,
                        ParentId: versionData.InheritsFrom,
                        AssetIndexId: versionData.AssetIndex?.Id,
                        MainClass: versionData.MainClass
                    ));
                    
                    _logger.LogDebug("加载版本: {Id}", versionData.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "解析版本 JSON 失败: {Path}", versionJsonPath);
                }
            }
            
            _logger.LogInformation("加载了 {Count} 个游戏版本", _versions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载游戏版本失败");
        }
    }

    /// <summary>
    /// 从目录加载游戏实例
    /// </summary>
    private async Task LoadInstancesFromDirectoryAsync()
    {
        // 暂时从 versions 目录模拟加载实例
        var versions = await GetGameVersionsAsync();
        
        foreach (var version in versions)
        {
            var instance = new GameInstance(
                Id: version.Id,
                Name: version.Name,
                GameCoreId: version.Id,
                JavaPath: null,
                MaxMemory: 2048,
                MinMemory: 512,
                JvmArguments: null,
                GameDirectory: _gameDirectory,
                AssetsDirectory: Path.Combine(_gameDirectory, "assets"),
                VersionDirectory: Path.Combine(_gameDirectory, "versions", version.Id)
            );
            
            _instances.Add(instance);
        }
        
        _logger.LogInformation("加载了 {Count} 个游戏实例", _instances.Count);
    }

    /// <summary>
    /// 解析版本类型
    /// </summary>
    private static GameVersionType ParseVersionType(string? type)
    {
        return type?.ToLower() switch
        {
            "release" => GameVersionType.Release,
            "snapshot" => GameVersionType.Snapshot,
            "old_alpha" => GameVersionType.OldAlpha,
            "old_beta" => GameVersionType.OldBeta,
            _ => GameVersionType.Release
        };
    }

    /// <summary>
    /// 版本 JSON 模型（用于解析）
    /// </summary>
    private class VersionJsonModel
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public DateTime? ReleaseTime { get; set; }
        public string? InheritsFrom { get; set; }
        public string? MainClass { get; set; }
        public AssetIndexModel? AssetIndex { get; set; }
    }

    private class AssetIndexModel
    {
        public string? Id { get; set; }
    }
}