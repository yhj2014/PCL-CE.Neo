using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Core.IO;

namespace PCL_CE.Neo.Core.Minecraft;

public record JavaInstallation(
    string Path,
    string Version,
    string? Vendor = null,
    long Memory = 0,
    JavaBrandType Brand = JavaBrandType.Unknown
);

public enum JavaBrandType
{
    Unknown,
    Oracle,
    AdoptOpenJDK,
    EclipseTemurin,
    AmazonCorretto,
    Microsoft,
    AzulZulu,
    OpenJ9
}

public interface IJavaManager
{
    Task<IReadOnlyList<JavaInstallation>> GetInstalledJavaAsync();
    Task<JavaInstallation?> GetJavaByVersionAsync(string version);
    Task<string?> FindJavaForGameVersionAsync(string gameVersion);
    Task<string> DownloadJavaAsync(string url, IProgress<double>? progress = null);
}

public class JavaManager : IJavaManager
{
    private readonly ILogger<JavaManager> _logger;
    private readonly IJavaScanner _javaScanner;
    private readonly IDownloadService _downloadService;
    private readonly List<JavaInstallation> _cachedJavaList = new();
    private bool _cacheInitialized;

    public JavaManager(
        ILogger<JavaManager> logger,
        IJavaScanner javaScanner,
        IDownloadService downloadService)
    {
        _logger = logger;
        _javaScanner = javaScanner;
        _downloadService = downloadService;
    }

    public async Task<IReadOnlyList<JavaInstallation>> GetInstalledJavaAsync()
    {
        if (_cacheInitialized)
        {
            return _cachedJavaList.AsReadOnly();
        }
        
        try
        {
            var javaPaths = _javaScanner.ScanJavaPaths();
            _cachedJavaList.Clear();
            
            foreach (var path in javaPaths)
            {
                if (_javaScanner.IsValidJavaPath(path))
                {
                    _cachedJavaList.Add(new JavaInstallation(
                        Path: path,
                        Version: "Unknown",
                        Vendor: null,
                        Memory: 0,
                        Brand: JavaBrandType.Unknown
                    ));
                }
            }
            
            _logger.LogInformation("Found {Count} Java installations", _cachedJavaList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scan Java installations");
        }

        _cacheInitialized = true;
        return _cachedJavaList.AsReadOnly();
    }

    public Task<JavaInstallation?> GetJavaByVersionAsync(string version)
    {
        var java = _cachedJavaList.FirstOrDefault(j => j.Version == version);
        return Task.FromResult(java);
    }

    public Task<string?> FindJavaForGameVersionAsync(string gameVersion)
    {
        string requiredVersion;
        
        if (gameVersion.StartsWith("1.7") || gameVersion.StartsWith("1.8"))
        {
            requiredVersion = "8";
        }
        else if (gameVersion.StartsWith("1.12") || gameVersion.StartsWith("1.13") || gameVersion.StartsWith("1.14") || gameVersion.StartsWith("1.15") || gameVersion.StartsWith("1.16"))
        {
            requiredVersion = "8";
        }
        else if (gameVersion.StartsWith("1.17") || gameVersion.StartsWith("1.18"))
        {
            requiredVersion = "16";
        }
        else if (gameVersion.StartsWith("1.19") || gameVersion.StartsWith("1.20") || gameVersion.StartsWith("1.21"))
        {
            requiredVersion = "17";
        }
        else
        {
            requiredVersion = "8";
        }

        var suitableJava = _cachedJavaList
            .Where(j => j.Version.StartsWith(requiredVersion))
            .OrderByDescending(j => j.Memory)
            .FirstOrDefault();

        return Task.FromResult(suitableJava?.Path);
    }

    public async Task<string> DownloadJavaAsync(string url, IProgress<double>? progress = null)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"PCLCE_java_{Guid.NewGuid()}.zip");
        
        try
        {
            await _downloadService.DownloadFileAsync(url, tempPath, progress);
            
            var javaDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PCLCE",
                "Java"
            );
            Directory.CreateDirectory(javaDir);
            
            var extractDir = Path.Combine(javaDir, Path.GetFileNameWithoutExtension(url));
            
            if (Directory.Exists(extractDir))
            {
                Directory.Delete(extractDir, true);
            }
            
            System.IO.Compression.ZipFile.ExtractToDirectory(tempPath, extractDir);
            
            var javaExe = Directory.GetFiles(extractDir, "java.exe", SearchOption.AllDirectories)
                .FirstOrDefault();
            
            if (javaExe == null)
            {
                javaExe = Directory.GetFiles(extractDir, "java", SearchOption.AllDirectories)
                    .FirstOrDefault();
            }
            
            if (javaExe != null)
            {
                _logger.LogInformation("Downloaded Java to {Path}", javaExe);
                return javaExe;
            }
            
            throw new InvalidOperationException("Java executable not found after extraction");
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}
