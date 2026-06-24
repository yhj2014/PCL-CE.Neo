using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Update;

public class UpdateService : IUpdateService, IDisposable
{
    private readonly ILogger<UpdateService> _logger;
    private readonly INetworkAdapter _networkAdapter;
    private readonly IPathsAdapter _pathsAdapter;
    private readonly IPlatformService _platformService;
    private readonly HttpClient _httpClient;
    
    private readonly List<IUpdateSource> _updateSources;
    private UpdateChannel _currentChannel = UpdateChannel.Stable;
    private AutoUpdateBehavior _autoUpdateBehavior = AutoUpdateBehavior.CheckOnly;
    
    private UpdateStatus _currentStatus = UpdateStatus.Unknown;
    private VersionInfo? _latestVersion;
    private VersionInfo? _downloadedVersion;
    private bool _isUpdateWaitingRestart;
    private readonly string _updateTempPath;
    private readonly string _updateTargetPath;
    
    private bool _disposed;

    public event Action<UpdateStatus>? StatusChanged;

    public UpdateStatus CurrentStatus => _currentStatus;
    public VersionInfo? LatestVersion => _latestVersion;
    public VersionInfo? DownloadedVersion => _downloadedVersion;
    public bool IsUpdateWaitingRestart => _isUpdateWaitingRestart;

    public UpdateService() : this(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<UpdateService>.Instance,
        new NetworkAdapter(),
        new PathsAdapter(),
        CreateDefaultPlatformService())
    {
    }

    public UpdateService(
        ILogger<UpdateService> logger,
        INetworkAdapter networkAdapter,
        IPathsAdapter pathsAdapter,
        IPlatformService platformService)
    {
        _logger = logger;
        _networkAdapter = networkAdapter;
        _pathsAdapter = pathsAdapter;
        _platformService = platformService;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
        
        _updateSources = new List<IUpdateSource>
        {
            new MirrorChyanUpdateSource(_logger),
            new MinioUpdateSource(_logger, "https://s3.pysio.online/pcl2-ce/", "Pysio"),
            new MinioUpdateSource(_logger, "https://staticassets.naids.com/resources/pclce/", "Naids")
        };
        
        _updateTempPath = Path.Combine(_pathsAdapter.SharedLocalData, "updates");
        _updateTargetPath = Path.Combine(_pathsAdapter.SharedLocalData, "PCL-CE.exe");
        
        Directory.CreateDirectory(_updateTempPath);
    }

    private static IPlatformService CreateDefaultPlatformService()
    {
        return new DefaultPlatformService();
    }

    public void SetUpdateChannel(UpdateChannel channel)
    {
        _currentChannel = channel;
        _logger.LogInformation("Update channel set to: {Channel}", channel);
    }

    public void SetAutoUpdateBehavior(AutoUpdateBehavior behavior)
    {
        _autoUpdateBehavior = behavior;
        _logger.LogInformation("Auto update behavior set to: {Behavior}", behavior);
    }

    public async Task<UpdateStatus> CheckForUpdateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking for updates on channel: {Channel}", _currentChannel);
        
        try
        {
            var arch = GetArchitecture();
            
            foreach (var source in _updateSources)
            {
                try
                {
                    var latest = await source.GetLatestVersionAsync(_currentChannel, arch);
                    if (latest != null)
                    {
                        _latestVersion = latest;
                        _logger.LogInformation("Found latest version: {Version} from {Source}", 
                            latest.VersionName, source.Name);
                        
                        var isLatest = await source.IsLatestAsync(
                            _currentChannel, arch, 
                            GetCurrentVersion(), 
                            GetCurrentBuild());
                        
                        if (isLatest)
                        {
                            SetStatus(UpdateStatus.Latest);
                        }
                        else
                        {
                            SetStatus(UpdateStatus.UpdateAvailable);
                        }
                        
                        return _currentStatus;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to check updates from {Source}", source.Name);
                }
            }
            
            SetStatus(UpdateStatus.Unknown);
            return _currentStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check for updates");
            SetStatus(UpdateStatus.Unknown);
            return _currentStatus;
        }
    }

    public async Task<bool> DownloadUpdateAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        if (_latestVersion == null)
        {
            _logger.LogWarning("No update version available to download");
            return false;
        }

        _logger.LogInformation("Downloading update: {Version}", _latestVersion.VersionName);
        
        try
        {
            SetStatus(UpdateStatus.NotLatest);
            
            var tempFilePath = Path.Combine(_updateTempPath, $"PCL-CE-{_latestVersion.VersionName}.exe");
            Directory.CreateDirectory(_updateTempPath);

            var responseBytes = await _networkAdapter.GetBytesAsync(_latestVersion.DownloadUrl);
            
            if (responseBytes == null || responseBytes.Length < 1024)
            {
                _logger.LogError("Failed to download update file");
                return false;
            }

            await File.WriteAllBytesAsync(tempFilePath, responseBytes, cancellationToken);
            
            progress?.Report(1.0);
            
            if (!string.IsNullOrEmpty(_latestVersion.SHA256))
            {
                await using var fs = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read);
                using var sha256 = SHA256.Create();
                var hash = await sha256.ComputeHashAsync(fs, cancellationToken);
                var hashString = Convert.ToHexString(hash).ToLowerInvariant();
                
                if (hashString != _latestVersion.SHA256.ToLowerInvariant())
                {
                    _logger.LogError("SHA256 mismatch for downloaded update. Expected: {Expected}, Got: {Actual}",
                        _latestVersion.SHA256, hashString);
                    File.Delete(tempFilePath);
                    return false;
                }
                
                _logger.LogInformation("SHA256 verification passed for downloaded update");
            }

            _downloadedVersion = _latestVersion;
            SetStatus(UpdateStatus.Downloaded);
            
            _logger.LogInformation("Update downloaded successfully to: {Path}", tempFilePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download update");
            return false;
        }
    }

    public async Task<bool> InstallUpdateAsync(CancellationToken cancellationToken = default)
    {
        if (_downloadedVersion == null)
        {
            _logger.LogWarning("No downloaded update available to install");
            return false;
        }

        _logger.LogInformation("Installing update: {Version}", _downloadedVersion.VersionName);
        
        try
        {
            var tempFilePath = Path.Combine(_updateTempPath, $"PCL-CE-{_downloadedVersion.VersionName}.exe");
            
            if (!File.Exists(tempFilePath))
            {
                _logger.LogError("Update file not found at: {Path}", tempFilePath);
                return false;
            }

            _isUpdateWaitingRestart = true;
            
            await Task.Run(() =>
            {
                var currentExePath = Environment.ProcessPath ?? _updateTargetPath;
                var updateExePath = tempFilePath;
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = updateExePath,
                    Arguments = $"update {Process.GetCurrentProcess().Id} \"{currentExePath}\" \"{updateExePath}\" true",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                
                Process.Start(startInfo);
            }, cancellationToken);
            
            _logger.LogInformation("Update installer started, waiting for restart");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to install update");
            _isUpdateWaitingRestart = false;
            return false;
        }
    }

    private void SetStatus(UpdateStatus status)
    {
        if (_currentStatus != status)
        {
            _currentStatus = status;
            StatusChanged?.Invoke(status);
        }
    }

    private string GetArchitecture()
    {
        var arch = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
        if (arch == "ARM64")
            return "arm64";
        return "x64";
    }

    private static string GetCurrentVersion()
    {
        return "2.0.0";
    }

    private static long GetCurrentBuild()
    {
        return 1;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}

internal class DefaultPlatformService : IPlatformService
{
    public string PlatformName => OperatingSystem.IsWindows() ? "Windows" : 
                                  OperatingSystem.IsMacOS() ? "macOS" : "Linux";
    public string OSVersion => Environment.OSVersion.ToString();
    public string Architecture => Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") ?? "x64";

    public void OpenUrl(string url) => _ = Launcher.OpenAsync(url);
    public void OpenFolder(string path) { }
    public string GetLocalApplicationDataPath() => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    public string GetTempPath() => Path.GetTempPath();
    public string GetGameDataPath() => throw new NotImplementedException();
}

internal static class Launcher
{
    public static Task OpenAsync(string url)
    {
        var psi = new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        };
        Process.Start(psi);
        return Task.CompletedTask;
    }
}

internal class MirrorChyanUpdateSource : IUpdateSource
{
    private readonly ILogger _logger;
    private const string BaseUrl = "https://pcl-link.example.com/api";

    public string Name => "MirrorChyan";

    public MirrorChyanUpdateSource(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<VersionInfo?> GetLatestVersionAsync(UpdateChannel channel, string arch)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            var channelStr = channel == UpdateChannel.Beta ? "beta" : "stable";
            var response = await client.GetStringAsync($"{BaseUrl}/versions/{channelStr}/{arch}");
            
            return JsonSerializer.Deserialize<VersionInfo>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get latest version from MirrorChyan");
            return null;
        }
    }

    public async Task<bool> IsLatestAsync(UpdateChannel channel, string arch, string currentVersion, long currentBuild)
    {
        var latest = await GetLatestVersionAsync(channel, arch);
        if (latest == null) return true;
        
        return CompareVersions(latest.VersionCode, currentVersion) <= 0 && 
               latest.Size <= currentBuild;
    }

    public async Task<byte[]?> DownloadVersionAsync(VersionInfo version, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
            return await client.GetByteArrayAsync(version.DownloadUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download version from MirrorChyan");
            return null;
        }
    }

    private static int CompareVersions(string v1, string v2)
    {
        var parts1 = v1.Split('.');
        var parts2 = v2.Split('.');
        var length = Math.Max(parts1.Length, parts2.Length);
        
        for (int i = 0; i < length; i++)
        {
            var p1 = i < parts1.Length && int.TryParse(parts1[i], out var n1) ? n1 : 0;
            var p2 = i < parts2.Length && int.TryParse(parts2[i], out var n2) ? n2 : 0;
            
            if (p1 > p2) return 1;
            if (p1 < p2) return -1;
        }
        
        return 0;
    }
}

internal class MinioUpdateSource : IUpdateSource
{
    private readonly ILogger _logger;
    private readonly string _baseUrl;
    private readonly string _name;

    public string Name => _name;

    public MinioUpdateSource(ILogger logger, string baseUrl, string name)
    {
        _logger = logger;
        _baseUrl = baseUrl;
        _name = name;
    }

    public async Task<VersionInfo?> GetLatestVersionAsync(UpdateChannel channel, string arch)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            var channelStr = channel == UpdateChannel.Beta ? "beta" : "stable";
            var response = await client.GetStringAsync($"{_baseUrl}versions/{channelStr}/{arch}/latest.json");
            
            return JsonSerializer.Deserialize<VersionInfo>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get latest version from {Name}", _name);
            return null;
        }
    }

    public async Task<bool> IsLatestAsync(UpdateChannel channel, string arch, string currentVersion, long currentBuild)
    {
        var latest = await GetLatestVersionAsync(channel, arch);
        if (latest == null) return true;
        
        return CompareVersions(latest.VersionCode, currentVersion) <= 0;
    }

    public async Task<byte[]?> DownloadVersionAsync(VersionInfo version, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
            return await client.GetByteArrayAsync(version.DownloadUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download version from {Name}", _name);
            return null;
        }
    }

    private static int CompareVersions(string v1, string v2)
    {
        var parts1 = v1.Split('.');
        var parts2 = v2.Split('.');
        var length = Math.Max(parts1.Length, parts2.Length);
        
        for (int i = 0; i < length; i++)
        {
            var p1 = i < parts1.Length && int.TryParse(parts1[i], out var n1) ? n1 : 0;
            var p2 = i < parts2.Length && int.TryParse(parts2[i], out var n2) ? n2 : 0;
            
            if (p1 > p2) return 1;
            if (p1 < p2) return -1;
        }
        
        return 0;
    }
}
