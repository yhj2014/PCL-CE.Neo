using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.IO;

public interface IDownloadTask
{
    string Id { get; }
    string Url { get; }
    string DestinationPath { get; }
    long TotalBytes { get; }
    long DownloadedBytes { get; }
    double Progress { get; }
    bool IsCompleted { get; }
    Exception? Error { get; }
    CancellationToken CancellationToken { get; }
}

public interface IDownloadService
{
    Task<string> DownloadFileAsync(string url, string destinationPath, 
        IProgress<double>? progress = null, 
        CancellationToken cancellationToken = default);
    Task DownloadFilesAsync(IEnumerable<(string Url, string Path)> files,
        IProgress<(int Current, int Total, double Progress)>? progress = null,
        CancellationToken cancellationToken = default);
    int MaxConcurrentDownloads { get; set; }
    int MaxRetryCount { get; set; }
}

public class DownloadService : IDownloadService, IDisposable
{
    private readonly ILogger<DownloadService> _logger;
    private readonly Http.HttpClient _httpClient;
    private readonly SemaphoreSlim _semaphore;

    public int MaxConcurrentDownloads { get; set; } = 4;
    public int MaxRetryCount { get; set; } = 3;

    public DownloadService() : this(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<DownloadService>.Instance)
    {
    }

    public DownloadService(ILogger<DownloadService> logger)
    {
        _logger = logger;
        _httpClient = new Http.HttpClient
        {
            Timeout = TimeSpan.FromMinutes(30)
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "PCL-CE.Neo/2.0");
        _semaphore = new SemaphoreSlim(MaxConcurrentDownloads);
    }

    public async Task<string> DownloadFileAsync(string url, string destinationPath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Downloading {Url} to {Path}", url, destinationPath);

        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var retryCount = 0;
        while (retryCount < MaxRetryCount)
        {
            try
            {
                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1;
                await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[8192];
                long totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    totalBytesRead += bytesRead;

                    if (totalBytes > 0)
                    {
                        progress?.Report((double)totalBytesRead / totalBytes);
                    }
                }

                _logger.LogInformation("Download completed: {Path}", destinationPath);
                return destinationPath;
            }
            catch (Exception ex) when (retryCount < MaxRetryCount - 1)
            {
                retryCount++;
                _logger.LogWarning(ex, "Download failed, retrying ({Retry}/{MaxRetry}): {Url}", 
                    retryCount, MaxRetryCount, url);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)), cancellationToken);
            }
        }

        throw new IOException($"Failed to download {url} after {MaxRetryCount} retries");
    }

    public async Task DownloadFilesAsync(IEnumerable<(string Url, string Path)> files,
        IProgress<(int Current, int Total, double Progress)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var fileList = files.ToList();
        var total = fileList.Count;
        var current = 0;

        var tasks = fileList.Select(async file =>
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                await DownloadFileAsync(file.Url, file.Path, 
                    new Progress<double>(p => progress?.Report((current, total, p))), 
                    cancellationToken);
                Interlocked.Increment(ref current);
            }
            finally
            {
                _semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    private bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _httpClient.Dispose();
        _semaphore.Dispose();
    }
}

public static class DownloadServiceExtensions
{
    public static IServiceCollection AddDownloadService(this IServiceCollection services)
    {
        services.AddSingleton<IDownloadService, DownloadService>();
        return services;
    }
}
