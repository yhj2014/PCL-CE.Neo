namespace PCL_CE.Neo.Core.Utils.FileSystem;

public static class PathUtils
{
    public static string NormalizePath(string path)
    {
        return Path.GetFullPath(new Uri(path).LocalPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    public static bool ArePathsEqual(string path1, string path2)
    {
        string normalized1 = NormalizePath(path1);
        string normalized2 = NormalizePath(path2);
        return string.Equals(normalized1, normalized2, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsSubPathOf(string path, string parentPath)
    {
        string normalizedPath = NormalizePath(path);
        string normalizedParent = NormalizePath(parentPath);
        return normalizedPath.StartsWith(normalizedParent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || normalizedPath == normalizedParent;
    }

    public static string ResolvePath(string basePath, string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return basePath;

        if (Path.IsPathRooted(relativePath))
            return relativePath;

        return Path.GetFullPath(Path.Combine(basePath, relativePath));
    }

    public static string GetShortPathName(string path)
    {
        return Path.GetFileName(path);
    }

    public static string GetLongPathName(string path)
    {
        return path;
    }

    public static string GetExecutablePath()
    {
        return System.Reflection.Assembly.GetEntryAssembly()?.Location ?? string.Empty;
    }

    public static string GetExecutableDirectory()
    {
        string exePath = GetExecutablePath();
        return string.IsNullOrEmpty(exePath) ? Directory.GetCurrentDirectory() : Path.GetDirectoryName(exePath) ?? string.Empty;
    }

    public static string GetApplicationDataPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    }

    public static string GetLocalApplicationDataPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }

    public static string GetCommonApplicationDataPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
    }

    public static string GetDocumentsPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    public static string GetDesktopPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }

    public static string GetDownloadsPath()
    {
        string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        downloadsPath = Path.Combine(downloadsPath, "Downloads");
        if (Directory.Exists(downloadsPath))
            return downloadsPath;

        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    public static string GetTemporaryFilePath(string extension = "")
    {
        string tempPath = Path.GetTempFileName();
        if (!string.IsNullOrEmpty(extension))
        {
            string newPath = tempPath + extension;
            File.Move(tempPath, newPath);
            return newPath;
        }
        return tempPath;
    }

    public static string GetApplicationTempPath()
    {
        string appTempPath = Path.Combine(Path.GetTempPath(), "PCL-CE-NEO");
        if (!Directory.Exists(appTempPath))
            Directory.CreateDirectory(appTempPath);
        return appTempPath;
    }

    public static string GetSafePath(string basePath, string relativePath)
    {
        string fullPath = ResolvePath(basePath, relativePath);
        if (!IsSubPathOf(fullPath, basePath))
            throw new ArgumentException("相对路径不安全，超出基础路径范围。");
        return fullPath;
    }

    public static bool ContainsTraversal(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        string normalized = path.Replace('\\', '/');
        return normalized.Contains("/../") || normalized.Contains("\\..\\")
            || normalized.StartsWith("../") || normalized.StartsWith("..\\")
            || normalized.EndsWith("/..") || normalized.EndsWith("\\..");
    }

    public static string FixPathSeparators(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        char targetSeparator = Path.DirectorySeparatorChar;
        char otherSeparator = targetSeparator == '\\' ? '/' : '\\';
        return path.Replace(otherSeparator, targetSeparator);
    }
}