namespace PCL_CE.Neo.Core.Utils.OS;

public static class SystemPaths
{
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

        return GetDocumentsPath();
    }

    public static string GetPicturesPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
    }

    public static string GetMusicPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
    }

    public static string GetVideosPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
    }

    public static string GetTempPath()
    {
        return Path.GetTempPath();
    }

    public static string GetProgramFilesPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
    }

    public static string GetProgramFilesX86Path()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
    }

    public static string GetWindowsPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.Windows);
    }

    public static string GetSystemPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.System);
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
}