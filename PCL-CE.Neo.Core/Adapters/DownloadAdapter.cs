using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class DownloadAdapter : IDownloadAdapter
{
    private readonly ILogger<DownloadAdapter> _logger;
    private readonly IPathsAdapter _pathsAdapter;
    private readonly INetworkAdapter _networkAdapter;
    private int _speedLimit = 0;
    private int _threadLimit = 8;

    public event Action<DownloadProgress>? ProgressChanged;

    public DownloadAdapter() : this(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<DownloadAdapter>.Instance,
        new PathsAdapter(),
        new NetworkAdapter())
    {
    }

    public DownloadAdapter(
        ILogger<DownloadAdapter> logger,
        IPathsAdapter pathsAdapter,
        INetworkAdapter networkAdapter)
    {
        _logger = logger;
        _pathsAdapter = pathsAdapter;
        _networkAdapter = networkAdapter;
    }

    public async Task<DownloadResult> DownloadFileAsync(DownloadRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogDebug("开始下载: {Url} -> {Path}", request.Url, request.DestinationPath);

            var directory = Path.GetDirectoryName(request.DestinationPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(30);

            if (request.Headers != null)
            {
                foreach (var header in request.Headers)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            using var response = await client.GetAsync(request.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            var canReportProgress = totalBytes > 0;

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(
                request.DestinationPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            var buffer = new byte[81920];
            long totalRead = 0;
            var lastReportedBytes = 0L;
            var lastReportedTime = DateTime.Now;

            int bytesRead;
            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalRead += bytesRead;

                if (canReportProgress)
                {
                    var now = DateTime.Now;
                    var elapsed = (now - lastReportedTime).TotalSeconds;

                    if (elapsed >= 0.5)
                    {
                        var currentSpeed = (totalRead - lastReportedBytes) / elapsed;

                        if (_speedLimit > 0 && currentSpeed > _speedLimit)
                        {
                            var delayMs = (int)((bytesRead / (double)_speedLimit) * 1000 - elapsed * 1000);
                            if (delayMs > 0)
                            {
                                await Task.Delay(Math.Min(delayMs, 100), cancellationToken);
                            }
                        }

                        ProgressChanged?.Invoke(new DownloadProgress
                        {
                            Url = request.Url,
                            FilePath = request.DestinationPath,
                            TotalBytes = totalBytes,
                            DownloadedBytes = totalRead,
                            SpeedBytesPerSecond = currentSpeed
                        });

                        lastReportedBytes = totalRead;
                        lastReportedTime = now;
                    }
                }
            }

            stopwatch.Stop();

            if (!string.IsNullOrEmpty(request.ExpectedHash))
            {
                var hash = await ComputeFileHashAsync(request.DestinationPath, cancellationToken);
                if (!hash.Equals(request.ExpectedHash, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("文件哈希校验失败: {Expected} != {Actual}", request.ExpectedHash, hash);
                    return DownloadResult.Failed(request.DestinationPath, "哈希校验失败");
                }
            }

            _logger.LogInformation("下载完成: {Url} ({Bytes} bytes in {Time})",
                request.Url, totalRead, stopwatch.Elapsed);

            return DownloadResult.Succeeded(request.DestinationPath, totalRead, stopwatch.Elapsed);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("下载已取消: {Url}", request.Url);
            if (File.Exists(request.DestinationPath))
            {
                File.Delete(request.DestinationPath);
            }
            return DownloadResult.Failed(request.DestinationPath, "下载已取消");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "下载失败: {Url}", request.Url);
            return DownloadResult.Failed(request.DestinationPath, ex.Message, ex);
        }
    }

    public async Task<DownloadResult[]> DownloadFilesAsync(DownloadRequest[] requests, CancellationToken cancellationToken = default)
    {
        var tasks = requests.Select(r => DownloadFileAsync(r, cancellationToken)).ToArray();
        return await Task.WhenAll(tasks);
    }

    public void SetSpeedLimit(int bytesPerSecond)
    {
        _speedLimit = bytesPerSecond;
        _logger.LogInformation("下载速度限制已设置为: {Speed} bytes/s", bytesPerSecond);
    }

    public void SetThreadLimit(int threadCount)
    {
        _threadLimit = Math.Max(1, Math.Min(threadCount, 32));
        _logger.LogInformation("下载线程数已设置为: {Count}", _threadLimit);
    }

    private async Task<string> ComputeFileHashAsync(string filePath, CancellationToken cancellationToken)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var hash = await sha256.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
