using System;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft;

/// <summary>
/// 游戏版本类型
/// </summary>
public enum GameVersionType
{
    /// <summary>
    /// 正式版本
    /// </summary>
    Release,
    
    /// <summary>
    /// 快照版本
    /// </summary>
    Snapshot,
    
    /// <summary>
    /// 旧 Alpha 版本
    /// </summary>
    OldAlpha,
    
    /// <summary>
    /// 旧 Beta 版本
    /// </summary>
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
    string? ParentId = null
);

/// <summary>
/// 游戏核心信息
/// </summary>
public record GameCore(
    string Id,
    string Name,
    string Type,
    bool IsModLoader,
    string? ModLoaderName = null
);

/// <summary>
/// 游戏实例配置
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
    /// <summary>
    /// 工作目录
    /// </summary>
    public string WorkingDirectory => GameDirectory ?? Path.Combine(GetDefaultGameDir(), Id);
    
    /// <summary>
    /// 获取默认游戏目录
    /// </summary>
    public static string GetDefaultGameDir() 
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".minecraft"
        );
    }
}

/// <summary>
/// 游戏核心文件操作工具，用于修改 JAR 文件
/// </summary>
public class GameCoreOperator
{
    private readonly ILogger<GameCoreOperator>? _logger;
    private readonly string _corePath;

    /// <summary>
    /// 创建游戏核心操作器
    /// </summary>
    /// <param name="corePath">核心 JAR 文件路径</param>
    /// <param name="logger">日志记录器</param>
    public GameCoreOperator(string corePath, ILogger<GameCoreOperator>? logger = null)
    {
        if (string.IsNullOrEmpty(corePath))
            throw new ArgumentException("核心文件路径不能为空", nameof(corePath));
        
        if (!File.Exists(corePath))
            throw new FileNotFoundException($"核心文件不存在: {corePath}", corePath);
        
        _corePath = corePath;
        _logger = logger;
        _logger?.LogDebug("游戏核心操作器已创建: {Path}", corePath);
    }

    /// <summary>
    /// 将指定的 JAR 文件合并到游戏核心中（用于 Mod 安装）
    /// </summary>
    /// <param name="jarPath">要添加的 JAR 文件路径</param>
    /// <param name="filter">文件过滤器（默认为空，合并所有文件）</param>
    /// <exception cref="FileNotFoundException">文件不存在</exception>
    /// <exception cref="IOException">IO 操作失败</exception>
    public void AddToCore(string jarPath, string filter = "")
    {
        if (!File.Exists(jarPath))
            throw new FileNotFoundException($"要添加的 JAR 文件不存在: {jarPath}", jarPath);

        _logger?.LogInformation("开始将文件合并到核心: {JarPath} -> {CorePath}", jarPath, _corePath);

        try
        {
            using var coreStream = new FileStream(_corePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 16384, true);
            using var jarStream = new FileStream(jarPath, FileMode.Open, FileAccess.Read, FileShare.Read, 16384, true);
            using var coreArchive = new ZipArchive(coreStream, ZipArchiveMode.Update);
            using var jarArchive = new ZipArchive(jarStream);

            // Better Than Watts 的 Mod File 是 .zip 结尾的，需要特殊过滤
            var effectiveFilter = jarPath.EndsWith(".jar") ? filter : "MINECRAFT-JAR";

            var addedCount = 0;
            foreach (var entry in jarArchive.Entries)
            {
                if (!entry.FullName.Contains(effectiveFilter))
                    continue;

                try
                {
                    // 删除已存在的同名文件（避免冲突）
                    var existingEntry = coreArchive.GetEntry(entry.FullName);
                    if (existingEntry != null)
                    {
                        existingEntry.Delete();
                        _logger?.LogDebug("删除已存在的文件: {FullName}", entry.FullName);
                    }

                    // 添加新文件
                    var newEntry = coreArchive.CreateEntry(entry.FullName);
                    using var coreEntryStream = newEntry.Open();
                    using var jarEntryStream = entry.Open();
                    jarEntryStream.CopyTo(coreEntryStream);

                    addedCount++;
                    _logger?.LogDebug("添加文件: {FullName}", entry.FullName);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "添加文件失败: {FullName}", entry.FullName);
                }
            }

            // 删除签名文件目录，避免 Oracle JDK 加载时验证签名失败
            var metaInfEntry = coreArchive.GetEntry("META-INF");
            if (metaInfEntry != null)
            {
                metaInfEntry.Delete();
                _logger?.LogDebug("删除签名目录: META-INF");
            }

            // 删除 META-INF 下的所有签名相关文件
            foreach (var entry in coreArchive.Entries)
            {
                if (entry.FullName.StartsWith("META-INF/") &&
                    (entry.FullName.EndsWith(".SF") ||
                     entry.FullName.EndsWith(".DSA") ||
                     entry.FullName.EndsWith(".RSA") ||
                     entry.FullName.EndsWith(".EC")))
                {
                    try
                    {
                        entry.Delete();
                        _logger?.LogDebug("删除签名文件: {FullName}", entry.FullName);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "删除签名文件失败: {FullName}", entry.FullName);
                    }
                }
            }

            _logger?.LogInformation("合并完成，共添加 {Count} 个文件", addedCount);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "合并文件到核心失败");
            throw;
        }
    }

    /// <summary>
    /// 从游戏核心中移除指定的文件
    /// </summary>
    /// <param name="entryName">要移除的文件名称（完整路径）</param>
    public void RemoveFromCore(string entryName)
    {
        if (string.IsNullOrEmpty(entryName))
            throw new ArgumentException("文件名称不能为空", nameof(entryName));

        _logger?.LogInformation("从核心移除文件: {EntryName}", entryName);

        try
        {
            using var coreStream = new FileStream(_corePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 16384, true);
            using var coreArchive = new ZipArchive(coreStream, ZipArchiveMode.Update);

            var entry = coreArchive.GetEntry(entryName);
            if (entry != null)
            {
                entry.Delete();
                _logger?.LogInformation("文件已移除: {EntryName}", entryName);
            }
            else
            {
                _logger?.LogWarning("文件不存在: {EntryName}", entryName);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "移除文件失败: {EntryName}", entryName);
            throw;
        }
    }

    /// <summary>
    /// 列出核心中的所有文件
    /// </summary>
    /// <returns>文件列表</returns>
    public string[] ListEntries()
    {
        try
        {
            using var coreStream = new FileStream(_corePath, FileMode.Open, FileAccess.Read, FileShare.Read, 16384, true);
            using var coreArchive = new ZipArchive(coreStream);

            var entries = new string[coreArchive.Entries.Count];
            var i = 0;
            foreach (var entry in coreArchive.Entries)
            {
                entries[i++] = entry.FullName;
            }

            _logger?.LogDebug("核心包含 {Count} 个文件", entries.Length);
            return entries;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "列出核心文件失败");
            throw;
        }
    }

    /// <summary>
    /// 提取核心中的指定文件到目标路径
    /// </summary>
    /// <param name="entryName">文件名称</param>
    /// <param name="targetPath">目标路径</param>
    public void ExtractEntry(string entryName, string targetPath)
    {
        if (string.IsNullOrEmpty(entryName))
            throw new ArgumentException("文件名称不能为空", nameof(entryName));
        
        if (string.IsNullOrEmpty(targetPath))
            throw new ArgumentException("目标路径不能为空", nameof(targetPath));

        _logger?.LogInformation("提取文件: {EntryName} -> {TargetPath}", entryName, targetPath);

        try
        {
            // 确保目标目录存在
            var targetDir = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            using var coreStream = new FileStream(_corePath, FileMode.Open, FileAccess.Read, FileShare.Read, 16384, true);
            using var coreArchive = new ZipArchive(coreStream);

            var entry = coreArchive.GetEntry(entryName);
            if (entry == null)
            {
                throw new FileNotFoundException($"核心中不存在该文件: {entryName}");
            }

            using var entryStream = entry.Open();
            using var targetStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None, 16384, true);
            entryStream.CopyTo(targetStream);

            _logger?.LogInformation("文件提取成功: {TargetPath}", targetPath);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "提取文件失败: {EntryName}", entryName);
            throw;
        }
    }

    /// <summary>
    /// 验证核心文件完整性
    /// </summary>
    /// <returns>是否完整</returns>
    public bool ValidateIntegrity()
    {
        try
        {
            using var coreStream = new FileStream(_corePath, FileMode.Open, FileAccess.Read, FileShare.Read, 16384, true);
            using var coreArchive = new ZipArchive(coreStream);

            // 检查是否存在基本的 Minecraft 类文件
            var hasMainClass = coreArchive.Entries.Any(e => 
                e.FullName.StartsWith("net/minecraft/") || 
                e.FullName.StartsWith("com/mojang/") ||
                e.FullName == "META-INF/MANIFEST.MF");

            if (!hasMainClass)
            {
                _logger?.LogWarning("核心文件可能不完整：缺少主要的类文件");
                return false;
            }

            _logger?.LogInformation("核心文件完整性验证通过");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "验证核心文件完整性失败");
            return false;
        }
    }
}