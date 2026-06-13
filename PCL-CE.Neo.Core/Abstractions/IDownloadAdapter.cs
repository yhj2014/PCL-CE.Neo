namespace PCL_CE.Neo.Core.Abstractions;

public interface IDownloadAdapter
{
    event Action<DownloadProgress>? ProgressChanged;

    Task<DownloadResult> DownloadFileAsync(DownloadRequest request, CancellationToken cancellationToken = default);
    Task<DownloadResult[]> DownloadFilesAsync(DownloadRequest[] requests, CancellationToken cancellationToken = default);

    void SetSpeedLimit(int bytesPerSecond);
    void SetThreadLimit(int threadCount);
}

public record DownloadRequest
{
    public required string Url { get; init; }
    public required string DestinationPath { get; init; }
    public string? ExpectedHash { get; init; }
    public Dictionary<string, string>? Headers { get; init; }
    public bool ResumeSupported { get; init; } = true;
}

public record DownloadResult
{
    public bool Success { get; init; }
    public string FilePath { get; init; } = "";
    public long BytesDownloaded { get; init; }
    public TimeSpan Elapsed { get; init; }
    public string? ErrorMessage { get; init; }
    public Exception? Exception { get; init; }

    public static DownloadResult Succeeded(string path, long bytes, TimeSpan elapsed) =>
        new() { Success = true, FilePath = path, BytesDownloaded = bytes, Elapsed = elapsed };
    public static DownloadResult Succeeded(long bytes) => new() { Success = true, BytesDownloaded = bytes };
    public static DownloadResult Failed(string path, string message, Exception? ex = null) =>
        new() { Success = false, FilePath = path, ErrorMessage = message, Exception = ex };
    public static DownloadResult Failed(string message) => new() { Success = false, FilePath = "", ErrorMessage = message };
}

public record DownloadProgress
{
    public required string Url { get; init; }
    public required string FilePath { get; init; }
    public long TotalBytes { get; init; }
    public long DownloadedBytes { get; init; }
    public double ProgressPercent => TotalBytes > 0 ? (double)DownloadedBytes / TotalBytes * 100 : 0;
    public double SpeedBytesPerSecond { get; init; }
    public TimeSpan? RemainingTime { get; init; }
}
