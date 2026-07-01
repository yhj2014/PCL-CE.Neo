namespace PCL_CE.Neo.Core.Utils.Download;

public class DownloadTask
{
    public string TaskId { get; } = Guid.NewGuid().ToString();
    public string Url { get; set; } = string.Empty;
    public string SavePath { get; set; } = string.Empty;
    public long TotalSize { get; set; }
    public long DownloadedSize { get; set; }
    public DownloadStatus Status { get; set; } = DownloadStatus.Pending;
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetryCount { get; set; } = 3;
    public long SpeedBytesPerSecond { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public CancellationTokenSource? CancellationTokenSource { get; set; }

    public double Progress => TotalSize > 0 ? (double)DownloadedSize / TotalSize * 100 : 0;
    public TimeSpan? ElapsedTime => EndTime.HasValue ? EndTime - StartTime : DateTime.Now - StartTime;
    public TimeSpan? RemainingTime => SpeedBytesPerSecond > 0 && TotalSize > DownloadedSize 
        ? TimeSpan.FromSeconds((TotalSize - DownloadedSize) / SpeedBytesPerSecond) 
        : null;

    public void Cancel()
    {
        CancellationTokenSource?.Cancel();
    }
}