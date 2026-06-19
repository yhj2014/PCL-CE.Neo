using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.IO.Download;

public interface IDownloadService
{
    Task<DownloadResult> DownloadFileAsync(string url, string destinationPath, CancellationToken cancellationToken = default);
    
    Task<DownloadResult> DownloadFileAsync(string url, string destinationPath, IProgress<DownloadProgress>? progress = null, CancellationToken cancellationToken = default);
    
    Task<DownloadResult> DownloadFileAsync(DownloadRequest request, CancellationToken cancellationToken = default);
}

public record DownloadRequest(
    string Url,
    string DestinationPath,
    long? ExpectedSize = null,
    string? ExpectedHash = null,
    string? HashAlgorithm = null,
    int RetryCount = 3,
    TimeSpan? Timeout = null);

public record DownloadResult(
    bool Success,
    string? ErrorMessage = null,
    long DownloadedBytes = 0,
    long TotalBytes = 0);

public record DownloadProgress(
    long DownloadedBytes,
    long TotalBytes,
    double ProgressPercent,
    double BytesPerSecond);

public enum DownloadStatus
{
    Pending,
    Downloading,
    Completed,
    Failed,
    Cancelled
}