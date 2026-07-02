using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft.Scanner;

/// <summary>
/// 默认路径 Java 扫描器，使用广度优先搜索算法扫描常见目录
/// </summary>
public class DefaultPathsScanner : IJavaScannerStrategy
{
    private readonly ILogger<DefaultPathsScanner>? _logger;
    private const int MaxSearchDepth = 8;
    
    /// <summary>
    /// 高概率包含 Java 的关键词
    /// </summary>
    private static readonly string[] MostPossibleKeywords =
    {
        "java", "jdk", "jre",
        "dragonwell", "azul", "zulu", "oracle", "open", "amazon", "corretto",
        "eclipse", "temurin", "hotspot", "semeru", "kona", "bellsoft"
    };

    /// <summary>
    /// 可能包含 Java 的关键词
    /// </summary>
    private static readonly string[] PossibleKeywords =
    {
        "environment", "env", "runtime", "x86_64", "amd64", "arm64",
        "pcl", "hmcl", "baka", "minecraft"
    };

    /// <summary>
    /// 需要排除的文件夹名称（避免误扫描）
    /// </summary>
    private static readonly string[] ExcludeFolderNames =
    {
        "javapath", "java8path", "common files", "netease"
    };

    public DefaultPathsScanner() : this(null) { }

    public DefaultPathsScanner(ILogger<DefaultPathsScanner>? logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 执行默认路径扫描
    /// </summary>
    /// <param name="results">结果集合</param>
    public void Scan(ICollection<string> results)
    {
        try
        {
            _logger?.LogInformation("开始默认路径 Java 扫描");
            
            var searchRoots = GetSearchRoots();
            _logger?.LogDebug("搜索根目录: {Roots}", string.Join(", ", searchRoots));

            foreach (var root in searchRoots)
            {
                BfsSearch(root, results);
            }

            _logger?.LogInformation("默认路径扫描完成，找到 {Count} 个安装", results.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "默认路径扫描失败");
        }
    }

    /// <summary>
    /// 获取搜索根目录列表
    /// </summary>
    private HashSet<string> GetSearchRoots()
    {
        var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 用户目录
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!string.IsNullOrEmpty(appData) && Directory.Exists(appData))
                roots.Add(appData);

            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!string.IsNullOrEmpty(localAppData) && Directory.Exists(localAppData))
                roots.Add(localAppData);

            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrEmpty(userProfile) && Directory.Exists(userProfile))
                roots.Add(userProfile);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "获取用户目录失败");
        }

        // 程序文件目录
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                // 扫描所有固定磁盘的 Program Files
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.DriveType == DriveType.Fixed && d.IsReady)
                    .Select(d => d.Name);

                foreach (var drive in drives)
                {
                    try
                    {
                        var programFiles = Path.Combine(drive, "Program Files");
                        if (Directory.Exists(programFiles))
                            roots.Add(programFiles);

                        var programFilesX86 = Path.Combine(drive, "Program Files (x86)");
                        if (Directory.Exists(programFilesX86))
                            roots.Add(programFilesX86);

                        // 根目录关键词搜索
                        var rootDirs = Directory.EnumerateDirectories(drive)
                            .Where(dir => MostPossibleKeywords.Any(k =>
                                Path.GetFileName(dir).Contains(k, StringComparison.OrdinalIgnoreCase)));

                        foreach (var dir in rootDirs)
                            roots.Add(dir);
                    }
                    catch (UnauthorizedAccessException) { }
                    catch (IOException) { }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "获取驱动器列表失败");
            }
        }
        else
        {
            // 非Windows系统的程序目录
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (!string.IsNullOrEmpty(programFiles) && Directory.Exists(programFiles))
                roots.Add(programFiles);

            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (!string.IsNullOrEmpty(programFilesX86) && Directory.Exists(programFilesX86))
                roots.Add(programFilesX86);
        }

        return roots;
    }

    /// <summary>
    /// 广度优先搜索 Java 安装
    /// </summary>
    private void BfsSearch(string rootPath, ICollection<string> results)
    {
        if (!Directory.Exists(rootPath))
        {
            _logger?.LogDebug("目录不存在，跳过: {Path}", rootPath);
            return;
        }

        var queue = new Queue<(string Path, int Depth)>();
        queue.Enqueue((rootPath, 0));

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();
            
            if (depth > MaxSearchDepth)
            {
                _logger?.LogDebug("达到最大搜索深度，停止: {Path}", current);
                continue;
            }

            if (!Directory.Exists(current))
                continue;

            try
            {
                // 深度0时只遍历含关键词的目录，深度>0时遍历所有子目录
                var dirsToScan = depth == 0
                    ? Directory.EnumerateDirectories(current)
                        .Where(ShouldScanDirectory)
                    : Directory.EnumerateDirectories(current);

                foreach (var dir in dirsToScan)
                {
                    try
                    {
                        var javaExe = GetJavaExePath(dir);
                        if (File.Exists(javaExe))
                        {
                            results.Add(javaExe);
                            _logger?.LogDebug("找到 Java 安装: {Path}", javaExe);
                        }
                        else
                        {
                            // 继续向下搜索
                            queue.Enqueue((dir, depth + 1));
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _logger?.LogDebug("无权限访问目录: {Path}", dir);
                    }
                    catch (IOException ioEx)
                    {
                        _logger?.LogDebug("IO错误，跳过目录: {Path}, Error: {Error}", dir, ioEx.Message);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                _logger?.LogDebug("无权限访问目录: {Path}", current);
            }
            catch (IOException ioEx)
            {
                _logger?.LogDebug("IO错误，跳过目录: {Path}, Error: {Error}", current, ioEx.Message);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "搜索目录时出错: {Path}", current);
            }
        }
    }

    /// <summary>
    /// 判断是否应该扫描该目录（基于关键词过滤）
    /// </summary>
    private bool ShouldScanDirectory(string path)
    {
        var name = Path.GetFileName(path);
        
        // 排除特定文件夹
        if (ExcludeFolderNames.Any(ex => name.Contains(ex, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        // 包含关键词的目录才扫描
        return MostPossibleKeywords.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase)) ||
               PossibleKeywords.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 获取 Java 可执行文件路径（根据操作系统）
    /// </summary>
    private string GetJavaExePath(string directory)
    {
        var binDir = Path.Combine(directory, "bin");
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(binDir, "java.exe");
        }
        else
        {
            return Path.Combine(binDir, "java");
        }
    }
}