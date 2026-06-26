using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft.Java;

/// <summary>
/// Scanner that searches default installation paths for Java.
/// Uses breadth-first search with keyword filtering for efficiency.
/// </summary>
public sealed class DefaultPathsScanner : IJavaScanner
{
    private readonly ILogger? _logger;

    public string Name => "DefaultPaths";

    public DefaultPathsScanner() : this(null)
    {
    }

    public DefaultPathsScanner(ILogger? logger)
    {
        _logger = logger;
    }

    public void Scan(ICollection<string> results)
    {
        try
        {
            var searchRoots = GetSearchRoots();
            _logger?.LogDebug("Scanning default paths for Java in {Count} root directories", searchRoots.Count);

            foreach (var root in searchRoots)
            {
                BfsSearch(root, results);
            }

            _logger?.LogInformation("DefaultPathsScanner found {Count} Java installations", results.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "DefaultPathsScanner failed");
        }
    }

    private HashSet<string> GetSearchRoots()
    {
        var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            if (!string.IsNullOrEmpty(appData) && Directory.Exists(appData))
                roots.Add(appData);

            if (!string.IsNullOrEmpty(localAppData) && Directory.Exists(localAppData))
                roots.Add(localAppData);

            if (!string.IsNullOrEmpty(userProfile) && Directory.Exists(userProfile))
                roots.Add(userProfile);

            if (!string.IsNullOrEmpty(programFiles) && Directory.Exists(programFiles))
                roots.Add(programFiles);

            if (!string.IsNullOrEmpty(programFilesX86) && Directory.Exists(programFilesX86))
                roots.Add(programFilesX86);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                AddWindowsSpecificRoots(roots);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                AddLinuxSpecificRoots(roots);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                AddMacOsSpecificRoots(roots);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to get search roots");
        }

        return roots;
    }

    private void AddWindowsSpecificRoots(HashSet<string> roots)
    {
        try
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.Fixed && d.IsReady)
                .Select(d => d.Name);

            foreach (var drive in drives)
            {
                roots.Add(Path.Combine(drive, "Program Files"));
                roots.Add(Path.Combine(drive, "Program Files (x86)"));

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
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to add Windows-specific roots");
        }
    }

    private void AddLinuxSpecificRoots(HashSet<string> roots)
    {
        var linuxRoots = new[]
        {
            "/usr/lib/jvm",
            "/usr/java",
            "/opt/java",
            "/opt",
            "/usr/local/java",
            "/home"
        };

        foreach (var root in linuxRoots)
        {
            if (Directory.Exists(root))
                roots.Add(root);
        }
    }

    private void AddMacOsSpecificRoots(HashSet<string> roots)
    {
        var macRoots = new[]
        {
            "/Library/Java/JavaVirtualMachines",
            "/System/Library/Java/JavaVirtualMachines",
            "/Users"
        };

        foreach (var root in macRoots)
        {
            if (Directory.Exists(root))
                roots.Add(root);
        }
    }

    private void BfsSearch(string rootPath, ICollection<string> results)
    {
        if (!Directory.Exists(rootPath))
            return;

        var queue = new Queue<(string Path, int Depth)>();
        queue.Enqueue((rootPath, 0));

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();

            if (depth > JavaConsts.MaxSearchDepth)
                continue;

            try
            {
                var dirsToScan = depth == 0
                    ? Directory.EnumerateDirectories(current)
                        .Where(ShouldScanDirectory)
                    : Directory.EnumerateDirectories(current);

                foreach (var dir in dirsToScan)
                {
                    var javaExe = GetJavaExecutable(dir);
                    if (javaExe != null && File.Exists(javaExe))
                    {
                        if (!results.Contains(javaExe))
                        {
                            results.Add(javaExe);
                            _logger?.LogDebug("Found Java: {Path}", javaExe);
                        }
                    }
                    else
                    {
                        queue.Enqueue((dir, depth + 1));
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                _logger?.LogDebug("Skipping directory (access denied): {Path}", current);
            }
            catch (IOException ex)
            {
                _logger?.LogDebug("Skipping directory (IO error): {Path} - {Message}", current, ex.Message);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error scanning directory: {Path}", current);
            }
        }
    }

    private static bool ShouldScanDirectory(string path)
    {
        var name = Path.GetFileName(path);

        if (JavaConsts.ExcludeFolderNames.Contains(name))
            return false;

        return JavaConsts.AllKeyworkds.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private static string? GetJavaExecutable(string folder)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var javaExe = Path.Combine(folder, "bin", "java.exe");
            return File.Exists(javaExe) ? javaExe : null;
        }
        else
        {
            var javaBin = Path.Combine(folder, "bin", "java");
            return File.Exists(javaBin) ? javaBin : null;
        }
    }
}