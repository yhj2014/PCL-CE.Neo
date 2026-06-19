using System.IO;

namespace PCL_CE.Neo.Core.Utils.Platform;

public static class SystemPaths
{
    public static string GetHomeDirectory()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }

    public static string GetAppDataDirectory()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    }

    public static string GetLocalAppDataDirectory()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }

    public static string GetProgramFilesDirectory()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
    }

    public static string GetProgramFilesX86Directory()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
    }

    public static string GetDesktopDirectory()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }

    public static string GetDocumentsDirectory()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    public static string GetDownloadsDirectory()
    {
        var downloadsPath = Path.Combine(GetHomeDirectory(), "Downloads");
        return Directory.Exists(downloadsPath) ? downloadsPath : GetDocumentsDirectory();
    }

    public static string GetTempDirectory()
    {
        return Path.GetTempPath();
    }

    public static string GetExecutableDirectory()
    {
        return AppContext.BaseDirectory;
    }

    public static string GetExecutablePath()
    {
        return Environment.ProcessPath ?? string.Empty;
    }
}