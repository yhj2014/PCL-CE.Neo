using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Utils.Hash;

namespace PCL_CE.Neo.Core.IO.Download;

public class DownloadService : IDownloadService
{
    private readonly ILogger<DownloadService> _logger;
    private readonly HttpClient _httpClient;

    public DownloadService(ILogger<DownloadService> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(30),
            DefaultRequestHeaders = { { "User-Agent", "PCL-CE.Neo/1.0" } }
        };
    }

    public DownloadService() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<DownloadService>.Instance)
    {
    }

    public async Task<DownloadResult> DownloadFileAsync(string url, string destinationPath, CancellationToken cancellationToken = default)
    {
        return await DownloadFileAsync(url, destinationPath, null, cancellationToken).ConfigureAwait(false);
    }

    public async Task<DownloadResult> DownloadFileAsync(string url, string destinationPath, IProgress<DownloadProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        var request = new DownloadRequest(url, destinationPath);
        return await DownloadFileAsync(request, progress, cancellationToken).ConfigureAwait(false);
    }

    public async Task<DownloadResult> DownloadFileAsync(DownloadRequest request, CancellationToken cancellationToken = default)
    {
        return await DownloadFileAsync(request, null, cancellationToken).ConfigureAwait(false);
    }

    private async Task<DownloadResult> DownloadFileAsync(DownloadRequest request, IProgress<DownloadProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting download: {Url} -> {Destination}", request.Url, request.DestinationPath);

        var retryCount = request.RetryCount;
        var timeout = request.Timeout ?? TimeSpan.FromMinutes(30);

        for (int attempt = 1; attempt <= retryCount; attempt++)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(timeout);

                var destinationDir = Path.GetDirectoryName(request.DestinationPath);
                if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                }

                using var response = await _httpClient.GetAsync(request.Url, HttpCompletionOption.ResponseHeadersRead, cts.Token).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? request.ExpectedSize ?? -1;

                using var contentStream = await response.Content.ReadAsStreamAsync(cts.Token).ConfigureAwait(false);
                using var fileStream = new FileStream(request.DestinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[8192];
                long downloadedBytes = 0;
                int bytesRead;
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cts.Token).ConfigureAwait(false)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, cts.Token).ConfigureAwait(false);
                    downloadedBytes += bytesRead;

                    if (progress != null && totalBytes > 0)
                    {
                        var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                        var bytesPerSecond = elapsedSeconds > 0 ? downloadedBytes / elapsedSeconds : 0;
                        var progressPercent = (double)downloadedBytes / totalBytes * 100;

                        progress.Report(new DownloadProgress(downloadedBytes, totalBytes, progressPercent, bytesPerSecond));
                    }
                }

                stopwatch.Stop();

                if (!string.IsNullOrEmpty(request.ExpectedHash))
                {
                    var hash = await ComputeFileHash(request.DestinationPath, request.HashAlgorithm).ConfigureAwait(false);
                    if (!hash.Equals(request.ExpectedHash, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogError("Hash mismatch for {Path}: expected {Expected}, got {Actual}", request.DestinationPath, request.ExpectedHash, hash);
                        File.Delete(request.DestinationPath);
                        return new DownloadResult(false, "文件校验失败", downloadedBytes, totalBytes);
                    }
                }

                _logger.LogInformation("Download completed: {Path} ({Bytes} bytes)", request.DestinationPath, downloadedBytes);
                return new DownloadResult(true, null, downloadedBytes, totalBytes);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Download cancelled: {Url}", request.Url);
                return new DownloadResult(false, "下载已取消");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Download attempt {Attempt} failed for {Url}: {Message}", attempt, request.Url, ex.Message);

                if (attempt >= retryCount)
                {
                    _logger.LogError(ex, "Download failed after {Attempt} attempts: {Url}", attempt, request.Url);
                    return new DownloadResult(false, ex.Message);
                }

                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken).ConfigureAwait(false);
            }
        }

        return new DownloadResult(false, "下载失败");
    }

    private async Task<string> ComputeFileHash(string filePath, string? algorithm)
    {
        algorithm = (algorithm ?? "SHA1").ToUpperInvariant();

        IHashProvider provider = algorithm switch
        {
            "SHA256" => SHA256Provider.Instance,
            "SHA512" => SHA512Provider.Instance,
            "MD5" => MD5Provider.Instance,
            _ => SHA1Provider.Instance
        };

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return await provider.ComputeHashStringAsync(stream).ConfigureAwait(false);
    }
}