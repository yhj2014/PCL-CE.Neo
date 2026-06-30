using PCL_CE.Neo.Core.Logging;
using System;
using System.IO;
using System.Linq;

namespace PCL_CE.Neo.Core.IO;

public static class Directories
{
    private const string ModuleName = "Directories";

    public static void Create(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        try
        {
            LogWrapper.Debug($"Creating directory: {path}", ModuleName);
            Directory.CreateDirectory(path);
            LogWrapper.Debug($"Created directory: {path}", ModuleName);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to create directory: {path}");
            throw;
        }
    }

    public static bool Exists(string path)
    {
        var result = Directory.Exists(path);
        LogWrapper.Debug($"Directory exists check: {path} = {result}", ModuleName);
        return result;
    }

    public static void Delete(string path, bool recursive = false)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        try
        {
            LogWrapper.Debug($"Deleting directory: {path} (recursive: {recursive})", ModuleName);
            Directory.Delete(path, recursive);
            LogWrapper.Debug($"Deleted directory: {path}", ModuleName);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to delete directory: {path}");
            throw;
        }
    }

    public static void Copy(string sourcePath, string destinationPath, bool recursive = false)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new ArgumentNullException(nameof(sourcePath));
        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentNullException(nameof(destinationPath));

        try
        {
            LogWrapper.Debug($"Copying directory: {sourcePath} -> {destinationPath}", ModuleName);

            Create(destinationPath);

            foreach (var file in Directory.GetFiles(sourcePath))
            {
                var destFile = Path.Combine(destinationPath, Path.GetFileName(file));
                Files.Copy(file, destFile, true);
            }

            if (recursive)
            {
                foreach (var subDir in Directory.GetDirectories(sourcePath))
                {
                    var destSubDir = Path.Combine(destinationPath, Path.GetFileName(subDir));
                    Copy(subDir, destSubDir, true);
                }
            }

            LogWrapper.Debug($"Copied directory: {sourcePath} -> {destinationPath}", ModuleName);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to copy directory: {sourcePath} -> {destinationPath}");
            throw;
        }
    }

    public static void Move(string sourcePath, string destinationPath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new ArgumentNullException(nameof(sourcePath));
        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentNullException(nameof(destinationPath));

        try
        {
            LogWrapper.Debug($"Moving directory: {sourcePath} -> {destinationPath}", ModuleName);
            Directory.Move(sourcePath, destinationPath);
            LogWrapper.Debug($"Moved directory: {sourcePath} -> {destinationPath}", ModuleName);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to move directory: {sourcePath} -> {destinationPath}");
            throw;
        }
    }

    public static string[] GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        try
        {
            LogWrapper.Debug($"Getting files from: {path}, pattern: {searchPattern}", ModuleName);
            var files = Directory.GetFiles(path, searchPattern, searchOption);
            LogWrapper.Debug($"Found {files.Length} files in: {path}", ModuleName);
            return files;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to get files from: {path}");
            throw;
        }
    }

    public static string[] GetDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        try
        {
            LogWrapper.Debug($"Getting directories from: {path}", ModuleName);
            var directories = Directory.GetDirectories(path, searchPattern, searchOption);
            LogWrapper.Debug($"Found {directories.Length} directories in: {path}", ModuleName);
            return directories;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to get directories from: {path}");
            throw;
        }
    }

    public static long GetSize(string path)
    {
        if (!Exists(path))
            throw new DirectoryNotFoundException($"Directory not found: {path}");

        try
        {
            LogWrapper.Debug($"Calculating size of directory: {path}", ModuleName);
            
            var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            var size = files.Sum(f => new FileInfo(f).Length);
            
            LogWrapper.Debug($"Directory size: {path} = {size} bytes", ModuleName);
            return size;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to calculate directory size: {path}");
            throw;
        }
    }

    public static void EnsureExists(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        if (!Exists(path))
        {
            Create(path);
        }
    }

    public static DirectoryInfo GetInfo(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        return new DirectoryInfo(path);
    }

    public static DateTime GetCreationTime(string path)
    {
        if (!Exists(path))
            throw new DirectoryNotFoundException($"Directory not found: {path}");

        return Directory.GetCreationTime(path);
    }

    public static DateTime GetLastWriteTime(string path)
    {
        if (!Exists(path))
            throw new DirectoryNotFoundException($"Directory not found: {path}");

        return Directory.GetLastWriteTime(path);
    }
}