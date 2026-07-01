namespace PCL_CE.Neo.Core.Utils.Download;

public class DownloadProgressEventArgs : EventArgs
{
    public string TaskId { get; }
    public DownloadStatus Status { get; }
    public long DownloadedSize { get; }
    public long TotalSize { get; }
    public long SpeedBytesPerSecond { get; }
    public string? ErrorMessage { get; }

    public DownloadProgressEventArgs(string taskId, DownloadStatus status, long downloadedSize, long totalSize, long speedBytesPerSecond, string? errorMessage = null)
    {
        TaskId = taskId;
        Status = status;
        DownloadedSize = downloadedSize;
        TotalSize = totalSize;
        SpeedBytesPerSecond = speedBytesPerSecond;
        ErrorMessage = errorMessage;
    }

    public double Progress => TotalSize > 0 ? (double)DownloadedSize / TotalSize * 100 : 0;
}