namespace PCL_CE.Neo.Core.App;

public static class Metadata
{
    public const string ApplicationName = "PCL-CE.Neo";
    public const string Version = "1.0.0";
    public const string Author = "PCL-CE Team";
    public const string Description = "Plain Craft Launcher CE Neo";
    public const string Copyright = "Copyright (c) 2024 PCL-CE Team";

    public static string GetVersionString()
    {
        return $"{ApplicationName} v{Version}";
    }

    public static string GetFullVersionString()
    {
        return $"{ApplicationName} v{Version} - {Description}";
    }
}