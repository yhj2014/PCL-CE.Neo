using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using PCL_CE.Neo.Core.Models;

namespace PCL_CE.Neo.Core.App;

/// <summary>
/// Basic utilities.
/// </summary>
public static class Basics
{
    #region Basic Info

    private static MetadataModel? _metadata;

    /// <summary>
    /// Launcher metadata.
    /// </summary>
    public static MetadataModel Metadata
    {
        get
        {
            if (_metadata == null)
            {
                var assembly = Assembly.GetEntryAssembly();
                if (assembly != null)
                {
                    try
                    {
                        var stream = assembly.GetManifestResourceStream("PCL.metadata.json");
                        if (stream != null)
                        {
                            _metadata = JsonSerializer.Deserialize<MetadataModel>(stream) ?? new MetadataModel();
                        }
                    }
                    catch
                    {
                        // Ignore errors
                    }
                }
                _metadata ??= new MetadataModel();
            }
            return _metadata;
        }
    }

    /// <summary>
    /// Version name.
    /// </summary>
    public static string VersionName => Metadata.Version.BaseName;

    /// <summary>
    /// Version internal code.
    /// </summary>
    public static int VersionCode => Metadata.Version.Code;

    /// <summary>
    /// Version branch name.
    /// </summary>
    public static string VersionBranch => Metadata.Version.BranchName;

    /// <summary>
    /// Whether the current date is April Fools' Day.
    /// </summary>
    public static bool IsAprilFool => DateTime.Now is { Month: 4, Day: 1 };

    #endregion

    #region Process Path Info

    private static Process? _currentProcess;

    /// <summary>
    /// Current process instance.
    /// </summary>
    public static Process CurrentProcess
    {
        get
        {
            _currentProcess ??= Process.GetCurrentProcess();
            return _currentProcess;
        }
    }

    /// <summary>
    /// Current process ID.
    /// </summary>
    public static int CurrentProcessId => CurrentProcess.Id;

    /// <summary>
    /// Absolute path to current process executable.
    /// </summary>
    public static string ExecutablePath => Environment.ProcessPath ?? string.Empty;

    /// <summary>
    /// Directory of current process executable.
    /// </summary>
    public static string ExecutableDirectory => Path.GetDirectoryName(ExecutablePath) ?? CurrentDirectory;

    /// <summary>
    /// Name of current process executable, including extension.
    /// </summary>
    public static string ExecutableName => Path.GetFileName(ExecutablePath);

    /// <summary>
    /// Name of current process executable, without extension.
    /// </summary>
    public static string ExecutableNameWithoutExtension => Path.GetFileNameWithoutExtension(ExecutablePath);

    /// <summary>
    /// Full command line arguments including the first one (filename).
    /// </summary>
    public static string[] FullCommandLineArguments => Environment.GetCommandLineArgs();

    /// <summary>
    /// Command line arguments excluding the first one (filename).
    /// </summary>
    public static string[] CommandLineArguments => FullCommandLineArguments.Length > 1 ? FullCommandLineArguments[1..] : Array.Empty<string>();

    /// <summary>
    /// Current directory, obtained in real-time.
    /// </summary>
    public static string CurrentDirectory => Environment.CurrentDirectory;

    #endregion

    #region Thread Operations

    /// <summary>
    /// Runs specified delegate in a new worker thread.
    /// </summary>
    public static Thread RunInNewThread(Action action, string? name = null, ThreadPriority priority = ThreadPriority.Normal)
    {
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (ThreadInterruptedException)
            {
                // LogWrapper.Trace("Thread", $"{name}: Thread interrupted");
            }
            catch (Exception ex)
            {
                // LogWrapper.Error(ex, "Thread", $"{name}: Exception thrown");
                Debug.WriteLine($"Thread error: {ex}");
            }
        })
        {
            Priority = priority
        };
        
        thread.Name = name ?? $"Worker#{thread.ManagedThreadId}";
        thread.Start();
        return thread;
    }

    #endregion

    #region Path Operations

    /// <summary>
    /// Gets parent path of a given path.
    /// </summary>
    public static string? GetParentPath(string path) => Path.GetDirectoryName(path) ?? Path.GetPathRoot(path);

    /// <summary>
    /// Gets parent path of a given path, or empty.
    /// </summary>
    public static string GetParentPathOrEmpty(string path) => GetParentPath(path) ?? string.Empty;

    /// <summary>
    /// Gets parent path of a given path, or default.
    /// </summary>
    public static string GetParentPathOrDefault(string path) => GetParentPath(path) ?? CurrentDirectory;

    /// <summary>
    /// Opens a path (file or directory) with default method.
    /// </summary>
    public static void OpenPath(string path, string? workingDirectory = null)
    {
        var psi = new ProcessStartInfo(path)
        {
            WorkingDirectory = workingDirectory ?? CurrentDirectory,
            UseShellExecute = true,
            CreateNoWindow = true
        };
        Process.Start(psi);
    }

    #endregion

    #region Application Operations

    /// <summary>
    /// Gets stream to an application package resource.
    /// </summary>
    public static Stream? GetResourceStream(string path)
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly != null)
        {
            try
            {
                return assembly.GetManifestResourceStream(path);
            }
            catch
            {
                // Ignore errors
            }
        }
        return null;
    }

    #endregion
}
