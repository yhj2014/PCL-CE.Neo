using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft.Java.Scanner;

public class DefaultPathsScanner(ILogger<DefaultPathsScanner> logger) : IJavaScanner
{
    private const int MaxSearchDepth = 8;

    public void Scan(ICollection<string> results)
    {
        try
        {
            var searchRoots = _GetSearchRoots();
            logger.LogInformation("[Java] 对下列目录进行广度关键词搜索:{SearchRoots}", string.Join(Environment.NewLine, searchRoots));

            foreach (var root in searchRoots)
            {
                _BfsSearch(root, results);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "默认路径扫描失败");
        }
    }

    private static HashSet<string> _GetSearchRoots()
    {
        var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            Path.Combine(AppContext.BaseDirectory, "PCL")
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var keyFolders = new[] { "Program Files", "Program Files (x86)" };
            var drives = DriveInfo.GetDrives()
                .Where(d => d.DriveType.Equals(DriveType.Fixed) && d.IsReady)
                .Select(d => d.Name);

            foreach (var drive in drives)
            {
                foreach (var folder in keyFolders)
                {
                    roots.Add(Path.Combine(drive, folder));
                }

                try
                {
                    var rootDirs = Directory.EnumerateDirectories(drive)
                        .Where(dir => JavaConsts.MostPossibleKeywords.Any(k =>
                            Path.GetFileName(dir).Contains(k, StringComparison.OrdinalIgnoreCase)));

                    foreach (var dir in rootDirs)
                        roots.Add(dir);
                }
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
            }
        }
        else
        {
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            if (!string.IsNullOrEmpty(programFiles) && Directory.Exists(programFiles))
                roots.Add(programFiles);
            if (!string.IsNullOrEmpty(programFilesX86) && Directory.Exists(programFilesX86))
                roots.Add(programFilesX86);
        }

        return roots;
    }

    private void _BfsSearch(string rootPath, ICollection<string> results)
    {
        if (!Directory.Exists(rootPath)) return;

        var queue = new Queue<(string Path, int Depth)>();
        queue.Enqueue((rootPath, 0));

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();
            if (depth > MaxSearchDepth || !Directory.Exists(current)) continue;

            try
            {
                var dirsToScan = depth == 0
                    ? Directory.EnumerateDirectories(current)
                        .Where(dir => _ShouldScanDirectory(dir))
                    : Directory.EnumerateDirectories(current);

                foreach (var dir in dirsToScan)
                {
                    var javaExe = Path.Combine(dir, "java.exe");
                    if (File.Exists(javaExe))
                    {
                        results.Add(javaExe);
                    }
                    else
                    {
                        var javaBin = Path.Combine(dir, "bin", "java.exe");
                        if (File.Exists(javaBin))
                        {
                            results.Add(javaBin);
                        }
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        {
                            var javaUnix = Path.Combine(dir, "java");
                            if (File.Exists(javaUnix))
                            {
                                results.Add(javaUnix);
                            }
                            else
                            {
                                javaUnix = Path.Combine(dir, "bin", "java");
                                if (File.Exists(javaUnix))
                                {
                                    results.Add(javaUnix);
                                }
                            }
                        }
                        else
                        {
                            queue.Enqueue((dir, depth + 1));
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or DirectoryNotFoundException)
            {
                logger.LogDebug("跳过目录 {Path}: {Message}", current, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "搜索目录 {Path} 时出错", current);
            }
        }
    }

    private static bool _ShouldScanDirectory(string path)
    {
        var name = Path.GetFileName(path);
        if (JavaConsts.ExcludeFolderNames.Any(ex => name.Contains(ex, StringComparison.OrdinalIgnoreCase)))
            return false;

        return JavaConsts.AllKeyworkds.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
}