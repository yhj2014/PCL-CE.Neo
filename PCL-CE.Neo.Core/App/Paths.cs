using System;
using System.IO;

namespace PCL_CE.Neo.Core.App;

/// <summary>
/// Application global paths.
/// </summary>
public static class Paths
{
    /// <summary>
    /// The default directory used for relative path combining.
    /// </summary>
    public static string DefaultDirectory => Basics.ExecutableDirectory;

    private static string? _data;
    private static string? _sharedData;
    private static string? _sharedLocalData;
    private static string? _temp;
    
    /// <summary>
    /// Per-instance data directory.
    /// </summary>
    public static string Data
    {
        get
        {
            EnsureInitialized();
            return _data!;
        }
        set
        {
            _data = value;
            if (!string.IsNullOrEmpty(_data) && !Directory.Exists(_data))
            {
                Directory.CreateDirectory(_data);
            }
        }
    }

    /// <summary>
    /// Shared synchronized data directory.
    /// </summary>
    public static string SharedData
    {
        get
        {
            EnsureInitialized();
            return _sharedData!;
        }
        set
        {
            _sharedData = value;
            if (!string.IsNullOrEmpty(_sharedData) && !Directory.Exists(_sharedData))
            {
                Directory.CreateDirectory(_sharedData);
            }
        }
    }
    
    /// <summary>
    /// Shared synchronized data directory of old versions.
    /// </summary>
    public static string OldSharedData { get; set; } = string.Empty;

    /// <summary>
    /// Shared local data directory.
    /// </summary>
    public static string SharedLocalData
    {
        get
        {
            EnsureInitialized();
            return _sharedLocalData!;
        }
        set
        {
            _sharedLocalData = value;
            if (!string.IsNullOrEmpty(_sharedLocalData) && !Directory.Exists(_sharedLocalData))
            {
                Directory.CreateDirectory(_sharedLocalData);
            }
        }
    }
    
    /// <summary>
    /// Temporary files directory.
    /// </summary>
    public static string Temp
    {
        get
        {
            EnsureInitialized();
            return _temp!;
        }
        set
        {
            _temp = value;
            if (!string.IsNullOrEmpty(_temp) && !Directory.Exists(_temp))
            {
                Directory.CreateDirectory(_temp);
            }
        }
    }

    private static bool _initialized;

    private static void EnsureInitialized()
    {
        if (_initialized) return;
        
        #if DEBUG
        const string name = "PCLCE_Debug";
        const string oldName = ".PCLCEDebug";
        #else
        const string name = "PCLCE";
        const string oldName = ".PCLCE";
        #endif
        
        // Fill paths
        _data ??= Path.Combine(DefaultDirectory, "PCL");
        _sharedData ??= GetSpecialPath(Environment.SpecialFolder.ApplicationData, name);
        _sharedLocalData ??= GetSpecialPath(Environment.SpecialFolder.LocalApplicationData, name);
        _temp ??= Path.Combine(Path.GetTempPath(), name);
        OldSharedData = GetSpecialPath(Environment.SpecialFolder.ApplicationData, oldName);
        
        // Create directories
        EnsureDirectoryExists(_data!);
        EnsureDirectoryExists(_sharedData!);
        EnsureDirectoryExists(_sharedLocalData!);
        EnsureDirectoryExists(_temp!);
        
        _initialized = true;
    }

    /// <summary>
    /// Get path string relative to a special folder.
    /// </summary>
    public static string GetSpecialPath(Environment.SpecialFolder folder, string relative)
    {
        var folderPath = Environment.GetFolderPath(folder);
        return Path.Combine(folderPath, relative);
    }

    /// <summary>
    /// Ensures directory exists.
    /// </summary>
    public static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
