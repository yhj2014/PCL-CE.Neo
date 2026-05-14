namespace PCL_CE.Neo.Core.Configuration;

public static class ConfigurationKeys
{
    public const string Theme = "Theme";
    public const string Language = "Language";
    public const string GameDataPath = "GameDataPath";
    public const string JavaPath = "JavaPath";
    public const string MaxMemory = "MaxMemory";
    public const string MinMemory = "MinMemory";
    public const string DownloadThreadCount = "DownloadThreadCount";
    public const string AutoSelectJava = "AutoSelectJava";
    public const string ShowHiddenFiles = "ShowHiddenFiles";
    public const string EnableTelemetry = "EnableTelemetry";
    public const string UpdateChannel = "UpdateChannel";
    public const string DownloadSource = "DownloadSource";
    public const string ProxyType = "ProxyType";
    public const string ProxyHost = "ProxyHost";
    public const string ProxyPort = "ProxyPort";
    public const string LastLoginType = "LastLoginType";
    public const string LastLoginUsername = "LastLoginUsername";
    public const string WindowWidth = "WindowWidth";
    public const string WindowHeight = "WindowHeight";
    public const string WindowState = "WindowState";
    public const string LogLevel = "LogLevel";
    public const string FileVersion = "FileVersion";
    public const string LocalFileVersion = "LocalFileVersion";
}

public enum ConfigSource
{
    Shared,
    SharedEncrypt,
    Local,
    GameInstance
}

public enum UpdateChannel
{
    Stable,
    Beta,
    Dev
}

public enum DownloadSource
{
    Official,
    Mirror
}

public enum ProxyType
{
    None,
    HTTP,
    SOCKS5
}
