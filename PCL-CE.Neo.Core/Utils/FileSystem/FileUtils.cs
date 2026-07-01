using System.IO.Compression;
using System.Text;

namespace PCL_CE.Neo.Core.Utils.FileSystem;

public static class FileUtils
{
    public static string GetTempPath()
    {
        return Path.GetTempPath();
    }

    public static string GetTempFileName()
    {
        return Path.GetTempFileName();
    }

    public static string Combine(params string[] paths)
    {
        return Path.Combine(paths);
    }

    public static bool Exists(string path)
    {
        return File.Exists(path);
    }

    public static bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    public static void EnsureParentDirectoryExists(string filePath)
    {
        string? parentDir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
            Directory.CreateDirectory(parentDir);
    }

    public static void Copy(string source, string destination, bool overwrite = false)
    {
        File.Copy(source, destination, overwrite);
    }

    public static void Move(string source, string destination, bool overwrite = false)
    {
        if (overwrite && File.Exists(destination))
            File.Delete(destination);
        File.Move(source, destination);
    }

    public static void Delete(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    public static void DeleteDirectory(string path, bool recursive = true)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive);
    }

    public static string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }

    public static string ReadAllText(string path, Encoding encoding)
    {
        return File.ReadAllText(path, encoding);
    }

    public static byte[] ReadAllBytes(string path)
    {
        return File.ReadAllBytes(path);
    }

    public static void WriteAllText(string path, string content)
    {
        EnsureParentDirectoryExists(path);
        File.WriteAllText(path, content);
    }

    public static void WriteAllText(string path, string content, Encoding encoding)
    {
        EnsureParentDirectoryExists(path);
        File.WriteAllText(path, content, encoding);
    }

    public static void WriteAllBytes(string path, byte[] content)
    {
        EnsureParentDirectoryExists(path);
        File.WriteAllBytes(path, content);
    }

    public static void AppendAllText(string path, string content)
    {
        EnsureParentDirectoryExists(path);
        File.AppendAllText(path, content);
    }

    public static string GetExtension(string path)
    {
        return Path.GetExtension(path);
    }

    public static string GetFileName(string path)
    {
        return Path.GetFileName(path);
    }

    public static string GetFileNameWithoutExtension(string path)
    {
        return Path.GetFileNameWithoutExtension(path);
    }

    public static string GetDirectoryName(string path)
    {
        return Path.GetDirectoryName(path) ?? string.Empty;
    }

    public static string GetFullPath(string path)
    {
        return Path.GetFullPath(path);
    }

    public static string GetRelativePath(string relativeTo, string path)
    {
        return Path.GetRelativePath(relativeTo, path);
    }

    public static long GetFileSize(string path)
    {
        if (!File.Exists(path))
            return 0;
        return new FileInfo(path).Length;
    }

    public static DateTime GetLastWriteTime(string path)
    {
        return File.GetLastWriteTime(path);
    }

    public static DateTime GetCreationTime(string path)
    {
        return File.GetCreationTime(path);
    }

    public static void SetLastWriteTime(string path, DateTime time)
    {
        File.SetLastWriteTime(path, time);
    }

    public static void SetCreationTime(string path, DateTime time)
    {
        File.SetCreationTime(path, time);
    }

    public static void Touch(string path)
    {
        if (File.Exists(path))
            File.SetLastWriteTime(path, DateTime.Now);
        else
            File.Create(path).Dispose();
    }

    public static IEnumerable<string> GetFiles(string directory, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        return Directory.EnumerateFiles(directory, searchPattern, searchOption);
    }

    public static IEnumerable<string> GetDirectories(string directory, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        return Directory.EnumerateDirectories(directory, searchPattern, searchOption);
    }

    public static void CopyDirectory(string sourceDir, string destDir, bool overwrite = false)
    {
        EnsureDirectoryExists(destDir);

        foreach (string file in GetFiles(sourceDir))
        {
            string destFile = Combine(destDir, GetFileName(file));
            Copy(file, destFile, overwrite);
        }

        foreach (string subDir in GetDirectories(sourceDir))
        {
            string destSubDir = Combine(destDir, GetFileName(subDir));
            CopyDirectory(subDir, destSubDir, overwrite);
        }
    }

    public static void MoveDirectory(string sourceDir, string destDir, bool overwrite = false)
    {
        if (overwrite && DirectoryExists(destDir))
            DeleteDirectory(destDir);
        Directory.Move(sourceDir, destDir);
    }

    public static string GetUniqueFileName(string directory, string fileName)
    {
        string baseName = GetFileNameWithoutExtension(fileName);
        string extension = GetExtension(fileName);
        string newPath = Combine(directory, fileName);
        int counter = 1;

        while (Exists(newPath))
        {
            newPath = Combine(directory, $"{baseName} ({counter++}){extension}");
        }

        return newPath;
    }

    public static bool IsPathRooted(string path)
    {
        return Path.IsPathRooted(path);
    }

    public static bool IsValidPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            GetFullPath(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string SanitizeFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        return new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
    }

    public static string SanitizePath(string path)
    {
        char[] invalidChars = Path.GetInvalidPathChars();
        return new string(path.Where(c => !invalidChars.Contains(c)).ToArray());
    }

    public static string GetCommonParentPath(IEnumerable<string> paths)
    {
        string[] pathArray = paths.Where(p => !string.IsNullOrEmpty(p)).ToArray();
        if (pathArray.Length == 0)
            return string.Empty;

        string[] firstParts = pathArray[0].Split(Path.DirectorySeparatorChar);
        for (int i = 0; i < firstParts.Length; i++)
        {
            for (int j = 1; j < pathArray.Length; j++)
            {
                string[] otherParts = pathArray[j].Split(Path.DirectorySeparatorChar);
                if (i >= otherParts.Length || !string.Equals(firstParts[i], otherParts[i], StringComparison.OrdinalIgnoreCase))
                    return string.Join(Path.DirectorySeparatorChar.ToString(), firstParts.Take(i));
            }
        }
        return pathArray[0];
    }

    public static void ZipDirectory(string sourceDir, string zipPath)
    {
        EnsureParentDirectoryExists(zipPath);
        if (File.Exists(zipPath))
            File.Delete(zipPath);
        ZipFile.CreateFromDirectory(sourceDir, zipPath);
    }

    public static void UnzipFile(string zipPath, string destDir)
    {
        EnsureDirectoryExists(destDir);
        ZipFile.ExtractToDirectory(zipPath, destDir);
    }
}