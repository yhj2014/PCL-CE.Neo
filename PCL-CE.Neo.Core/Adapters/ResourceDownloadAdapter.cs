using System.Text.Json;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class ResourceDownloadAdapter : IResourceDownloadAdapter
{
    private readonly ILogger<ResourceDownloadAdapter> _logger;
    private readonly IDownloadAdapter _download;
    private readonly INetworkAdapter _network;
    private readonly IPathsAdapter _paths;

    public ResourceDownloadAdapter(
        ILogger<ResourceDownloadAdapter> logger,
        IDownloadAdapter download,
        INetworkAdapter network,
        IPathsAdapter paths)
    {
        _logger = logger;
        _download = download;
        _network = network;
        _paths = paths;
    }

    public async Task<ResourceDownloadResult> DownloadVersionAsync(string versionId, IProgress<double>? progress = null)
    {
        try
        {
            _logger.LogInformation("下载版本: {Version}", versionId);

            var versions = await GetVersionManifestAsync();
            if (string.IsNullOrEmpty(versions))
            {
                return ResourceDownloadResult.Failed("", "无法获取版本列表");
            }

            var manifest = JsonSerializer.Deserialize<VersionManifest>(versions);
            var version = manifest?.Versions?.FirstOrDefault(v => v.Id == versionId);

            if (version == null)
            {
                return ResourceDownloadResult.Failed("", $"未找到版本: {versionId}");
            }

            var versionPath = Path.Combine(_paths.SharedLocalData, "versions", versionId, $"{versionId}.jar");
            Directory.CreateDirectory(Path.GetDirectoryName(versionPath)!);

            var request = new DownloadRequest
            {
                Url = version.Url,
                DestinationPath = versionPath,
                ExpectedHash = version.Sha1
            };

            var result = await _download.DownloadFileAsync(request);

            return result.Success
                ? ResourceDownloadResult.Succeeded(versionPath, result.BytesDownloaded, version.Sha1)
                : ResourceDownloadResult.Failed(versionPath, result.ErrorMessage ?? "下载失败", result.Exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "下载版本失败: {Version}", versionId);
            return ResourceDownloadResult.Failed("", ex.Message, ex);
        }
    }

    public async Task<ResourceDownloadResult> DownloadAssetAsync(string assetIndex, string assetPath, IProgress<double>? progress = null)
    {
        try
        {
            var assetIndexPath = Path.Combine(_paths.SharedLocalData, "assets", "indexes", $"{assetIndex}.json");
            Directory.CreateDirectory(Path.GetDirectoryName(assetIndexPath)!);

            var indexContent = await _network.GetAsync($"https://resources.download.minecraft.net/{assetIndex}");
            await File.WriteAllTextAsync(assetIndexPath, indexContent);

            var index = JsonSerializer.Deserialize<AssetIndex>(indexContent);
            if (index?.Objects == null)
            {
                return ResourceDownloadResult.Failed(assetPath, "无效的资源索引");
            }

            foreach (var obj in index.Objects.Values)
            {
                var hash = obj.Hash;
                var hashPath = Path.Combine(_paths.SharedLocalData, "assets", "objects", hash[..2], hash);
                Directory.CreateDirectory(Path.GetDirectoryName(hashPath)!);

                if (!File.Exists(hashPath))
                {
                    var url = $"https://resources.download.minecraft.net/{hash[..2]}/{hash}";
                    var request = new DownloadRequest
                    {
                        Url = url,
                        DestinationPath = hashPath
                    };

                    await _download.DownloadFileAsync(request);
                }
            }

            return ResourceDownloadResult.Succeeded(assetPath, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "下载资源失败");
            return ResourceDownloadResult.Failed(assetPath, ex.Message, ex);
        }
    }

    public async Task<ResourceDownloadResult> DownloadLibraryAsync(string libraryPath, IProgress<double>? progress = null)
    {
        try
        {
            var localPath = Path.Combine(_paths.SharedLocalData, "libraries", libraryPath);
            Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);

            if (File.Exists(localPath))
            {
                return ResourceDownloadResult.Succeeded(localPath, new FileInfo(localPath).Length);
            }

            var request = new DownloadRequest
            {
                Url = $"https://libraries.minecraft.net/{libraryPath}",
                DestinationPath = localPath
            };

            var result = await _download.DownloadFileAsync(request);

            return result.Success
                ? ResourceDownloadResult.Succeeded(localPath, result.BytesDownloaded)
                : ResourceDownloadResult.Failed(localPath, result.ErrorMessage ?? "下载失败", result.Exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "下载库失败: {Path}", libraryPath);
            return ResourceDownloadResult.Failed(libraryPath, ex.Message, ex);
        }
    }

    public async Task<ResourceDownloadResult> DownloadNativesAsync(string versionId, IProgress<double>? progress = null)
    {
        try
        {
            var nativesPath = Path.Combine(_paths.SharedLocalData, "versions", versionId, "natives");
            Directory.CreateDirectory(nativesPath);

            _logger.LogInformation("下载原生库: {Version}", versionId);

            var versionJson = await _network.GetAsync($"https://launchermeta.mojang.com/mc/game/version_manifest_v2.json");
            var manifest = JsonSerializer.Deserialize<VersionManifest>(versionJson);
            var version = manifest?.Versions?.FirstOrDefault(v => v.Id == versionId);

            if (version == null)
            {
                _logger.LogWarning("未找到版本: {Version}", versionId);
                return ResourceDownloadResult.Failed(versionId, $"未找到版本: {versionId}");
            }

            var versionManifestJson = await _network.GetAsync(version.Url);
            var versionManifest = JsonSerializer.Deserialize<VersionDetail>(versionManifestJson);

            if (versionManifest?.Natives == null)
            {
                return ResourceDownloadResult.Succeeded(nativesPath, 0);
            }

            var platformKey = GetPlatformKey();
            if (!versionManifest.Natives.TryGetValue(platformKey, out var nativeEntry))
            {
                platformKey = "linux";
                if (!versionManifest.Natives.TryGetValue(platformKey, out nativeEntry))
                {
                    return ResourceDownloadResult.Succeeded(nativesPath, 0);
                }
            }

            var nativeLibs = versionManifest.Libraries?
                .Where(l => l.Downloads?.Artifact != null && 
                           (l.Name.Contains("lwjgl") || 
                            l.Name.Contains("native") ||
                            l.Rules?.Any(r => r.OS?.Name == "linux") == true))
                .ToList() ?? new();

            long totalBytes = 0;
            long downloadedBytes = 0;

            foreach (var lib in nativeLibs)
            {
                if (lib.Downloads?.Artifact != null)
                {
                    totalBytes += lib.Downloads.Artifact.Size;
                }
            }

            foreach (var lib in nativeLibs)
            {
                if (lib.Downloads?.Artifact == null) continue;

                var artifact = lib.Downloads.Artifact;
                var nativeFileName = Path.GetFileName(artifact.Path);
                var targetPath = Path.Combine(nativesPath, nativeFileName);

                if (File.Exists(targetPath))
                {
                    downloadedBytes += artifact.Size;
                    progress?.Report((double)downloadedBytes / totalBytes);
                    continue;
                }

                var request = new DownloadRequest
                {
                    Url = artifact.Url,
                    DestinationPath = targetPath,
                    ExpectedHash = artifact.Sha1
                };

                var result = await _download.DownloadFileAsync(request);
                if (result.Success)
                {
                    downloadedBytes += artifact.Size;
                    progress?.Report((double)downloadedBytes / totalBytes);
                }
                else
                {
                    _logger.LogWarning("原生库下载失败: {Lib}", lib.Name);
                }
            }

            _logger.LogInformation("原生库下载完成: {Path}", nativesPath);
            return ResourceDownloadResult.Succeeded(nativesPath, downloadedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "下载原生库失败: {Version}", versionId);
            return ResourceDownloadResult.Failed(versionId, ex.Message, ex);
        }
    }

    private static string GetPlatformKey()
    {
        if (OperatingSystem.IsWindows())
            return "windows";
        if (OperatingSystem.IsMacOS())
            return "osx";
        if (OperatingSystem.IsLinux())
            return "linux";
        return "linux";
    }

    public async Task<string?> GetVersionManifestAsync()
    {
        try
        {
            return await _network.GetAsync("https://launchermeta.mojang.com/mc/game/version_manifest_v2.json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取版本清单失败");
            return null;
        }
    }

    public async Task<string?> GetAssetIndexAsync(string assetId)
    {
        try
        {
            return await _network.GetAsync($"https://resources.download.minecraft.net/indexes/{assetId}.json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取资源索引失败: {Asset}", assetId);
            return null;
        }
    }

    private class VersionManifest
    {
        public List<VersionInfo>? Versions { get; set; }
    }

    private class VersionInfo
    {
        public string Id { get; set; } = "";
        public string Url { get; set; } = "";
        public string Sha1 { get; set; } = "";
    }

    private class VersionDetail
    {
        public string Id { get; set; } = "";
        public Dictionary<string, string>? Natives { get; set; }
        public List<LibraryInfo>? Libraries { get; set; }
    }

    private class LibraryInfo
    {
        public string Name { get; set; } = "";
        public LibraryDownloads? Downloads { get; set; }
        public List<OSRule>? Rules { get; set; }
    }

    private class LibraryDownloads
    {
        public ArtifactInfo? Artifact { get; set; }
    }

    private class ArtifactInfo
    {
        public string Path { get; set; } = "";
        public string Url { get; set; } = "";
        public string Sha1 { get; set; } = "";
        public long Size { get; set; }
    }

    private class OSRule
    {
        public string Action { get; set; } = "";
        public OSInfo? OS { get; set; }
    }

    private class OSInfo
    {
        public string? Name { get; set; }
    }

    private class AssetIndex
    {
        public Dictionary<string, AssetObject>? Objects { get; set; }
    }

    private class AssetObject
    {
        public string Hash { get; set; } = "";
        public long Size { get; set; }
    }
}
