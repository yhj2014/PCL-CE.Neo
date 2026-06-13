using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class ApplicationAdapter : IApplicationAdapter
{
    private readonly ILogger<ApplicationAdapter> _logger;
    private readonly IPlatformService _platformService;
    private MetadataModel? _metadata;

    public ApplicationAdapter() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<ApplicationAdapter>.Instance)
    {
    }

    public ApplicationAdapter(ILogger<ApplicationAdapter> logger) : this(logger, CreateDefaultPlatformService())
    {
    }

    public ApplicationAdapter(ILogger<ApplicationAdapter> logger, IPlatformService platformService)
    {
        _logger = logger;
        _platformService = platformService;
    }

    private static IPlatformService CreateDefaultPlatformService()
    {
        try
        {
            var platformName = OperatingSystem.IsWindows() ? "Windows"
                : OperatingSystem.IsMacOS() ? "macOS"
                : "Linux";
            var assemblyName = $"PCL-CE.Neo.Platform.{platformName}";
            var typeName = $"PCL_CE.Neo.Platform.{platformName}.{platformName}PlatformService";
            var assembly = Assembly.Load(assemblyName);
            var type = assembly.GetType(typeName);
            if (type != null)
            {
                return Activator.CreateInstance(type) as IPlatformService
                    ?? throw new InvalidOperationException($"无法创建平台服务: {typeName}");
            }
        }
        catch
        {
        }
        return new DefaultPlatformService();
    }

    public string VersionName => Metadata?.Version.BaseName ?? "Unknown";
    public int VersionCode => Metadata?.Version.Code ?? 0;
    public string VersionBranch => Metadata?.Version.BranchName ?? "Unknown";
    public bool IsAprilFool => DateTime.Now is { Month: 4, Day: 1 };

    public int ProcessId => Environment.ProcessId;
    public string ExecutablePath => Environment.ProcessPath ?? "";
    public string ExecutableDirectory => Path.GetDirectoryName(ExecutablePath) ?? "";
    public string ExecutableName => Path.GetFileName(ExecutablePath);
    public string[] CommandLineArguments => Environment.GetCommandLineArgs()[1..];
    public string CurrentDirectory => Environment.CurrentDirectory;

    private MetadataModel Metadata
    {
        get
        {
            if (_metadata == null)
            {
                var assembly = Assembly.GetEntryAssembly();
                if (assembly == null)
                {
                    _logger.LogWarning("无法获取入口程序集");
                    return new MetadataModel { Version = new VersionModel() };
                }

                var stream = assembly.GetManifestResourceStream("PCL.metadata.json");
                if (stream != null)
                {
                    _metadata = JsonSerializer.Deserialize<MetadataModel>(stream);
                }
                else
                {
                    _logger.LogWarning("未找到元数据资源");
                    _metadata = new MetadataModel { Version = new VersionModel() };
                }
            }
            return _metadata;
        }
    }

    public Thread RunInNewThread(Action action, string? name = null, ThreadPriority priority = ThreadPriority.Normal)
    {
        var threadName = new AtomicVariable<string>(name);
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (ThreadInterruptedException) { _logger.LogTrace("Thread {Name}: 已中止", threadName.Value); }
            catch (Exception ex) { _logger.LogError(ex, "Thread {Name}: 抛出异常", threadName.Value); }
        })
        { Priority = priority };
        threadName.Value ??= $"Worker#{thread.ManagedThreadId}";
        thread.Name = threadName.Value;
        thread.Start();
        return thread;
    }

    public void OpenPath(string path, string? workingDirectory = null)
    {
        try
        {
            var psi = new ProcessStartInfo(path)
            {
                WorkingDirectory = workingDirectory ?? CurrentDirectory,
                UseShellExecute = true,
                CreateNoWindow = true
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "打开路径失败: {Path}", path);
        }
    }

    public Stream? GetResourceStream(string path)
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null) return null;

        var uri = new Uri($"resource://{assembly.GetName().Name}/{path}", UriKind.Absolute);
        return assembly.GetManifestResourceStream(path);
    }

    public string GetAppImagePath(string imageName) => $"resource://PCL-CE.Neo.App/Images/{imageName}";
}

internal class AtomicVariable<T>
{
    public T Value { get; set; }
    public AtomicVariable(T value) => Value = value;
}

public class MetadataModel
{
    public VersionModel Version { get; set; } = new();
}

public class VersionModel
{
    public string BaseName { get; set; } = "0.0.0";
    public int Code { get; set; }
    public string BranchName { get; set; } = "unknown";
}

public class DefaultPlatformService : IPlatformService
{
    public string PlatformName => "Unknown";
    public string OSVersion => Environment.OSVersion.ToString();
    public string Architecture => Environment.Is64BitOperatingSystem ? "x64" : "x86";

    public void OpenUrl(string url) => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    public void OpenFolder(string path) => Process.Start("explorer.exe", path);
    public string GetLocalApplicationDataPath() => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    public string GetTempPath() => Path.GetTempPath();
    public string GetGameDataPath() => Path.Combine(GetLocalApplicationDataPath(), ".minecraft");
}
