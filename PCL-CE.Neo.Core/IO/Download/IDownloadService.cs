using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.IO.Download;

public interface IDownloadService
{
    Task<DownloadTask> CreateDownloadTaskAsync(string url, string destinationPath, 
        string? checksum = null, string? checksumAlgorithm = null);

    Task StartDownloadAsync(DownloadTask task, CancellationToken cancellationToken = default);

    Task PauseDownloadAsync(DownloadTask task);

    Task ResumeDownloadAsync(DownloadTask task);

    Task CancelDownloadAsync(DownloadTask task);

    Task<bool> VerifyChecksumAsync(DownloadTask task);

    event EventHandler<DownloadProgressEventArgs>? ProgressChanged;
    event EventHandler<DownloadStateChangedEventArgs>? StateChanged;
}

public class DownloadProgressEventArgs : EventArgs
{
    public DownloadTask Task { get; }
    public long BytesDownloaded { get; }
    public long TotalBytes { get; }
    public double Speed { get; }

    public DownloadProgressEventArgs(DownloadTask task, long bytesDownloaded, long totalBytes, double speed)
    {
        Task = task;
        BytesDownloaded = bytesDownloaded;
        TotalBytes = totalBytes;
        Speed = speed;
    }
}

public class DownloadStateChangedEventArgs : EventArgs
{
    public DownloadTask Task { get; }
    public DownloadState OldState { get; }
    public DownloadState NewState { get; }

    public DownloadStateChangedEventArgs(DownloadTask task, DownloadState oldState, DownloadState newState)
    {
        Task = task;
        OldState = oldState;
        NewState = newState;
    }
}