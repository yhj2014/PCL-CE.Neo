using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.IO.Download;

public enum DownloadState
{
    Pending,
    Running,
    Paused,
    Completed,
    Failed,
    Canceled
}

public enum DownloadErrorType
{
    None,
    NetworkError,
    ServerError,
    FileError,
    ChecksumMismatch,
    Timeout,
    Canceled
}

public class DownloadTask
{
    public Guid Id { get; }
    public string Url { get; }
    public string DestinationPath { get; }
    public long TotalSize { get; set; }
    public long DownloadedSize { get; set; }
    public DownloadState State { get; set; }
    public DownloadErrorType ErrorType { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public double Speed { get; set; }
    public string? Checksum { get; set; }
    public string? ChecksumAlgorithm { get; set; }

    public double Progress => TotalSize > 0 ? (DownloadedSize * 100.0 / TotalSize) : 0;

    public TimeSpan? ElapsedTime => StartedAt.HasValue ? (CompletedAt ?? DateTime.Now) - StartedAt.Value : null;

    public DownloadTask(string url, string destinationPath)
    {
        Id = Guid.NewGuid();
        Url = url ?? throw new ArgumentNullException(nameof(url));
        DestinationPath = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
        State = DownloadState.Pending;
        CreatedAt = DateTime.Now;
    }

    public void Start()
    {
        if (State != DownloadState.Pending && State != DownloadState.Paused)
            throw new InvalidOperationException("Cannot start download in current state");

        State = DownloadState.Running;
        StartedAt = DateTime.Now;
    }

    public void Pause()
    {
        if (State != DownloadState.Running)
            throw new InvalidOperationException("Cannot pause download in current state");

        State = DownloadState.Paused;
    }

    public void Resume()
    {
        if (State != DownloadState.Paused)
            throw new InvalidOperationException("Cannot resume download in current state");

        State = DownloadState.Running;
    }

    public void Complete()
    {
        if (State != DownloadState.Running && State != DownloadState.Paused)
            throw new InvalidOperationException("Cannot complete download in current state");

        State = DownloadState.Completed;
        CompletedAt = DateTime.Now;
    }

    public void Fail(DownloadErrorType errorType, string errorMessage)
    {
        State = DownloadState.Failed;
        ErrorType = errorType;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.Now;
    }

    public void Cancel()
    {
        if (State == DownloadState.Completed || State == DownloadState.Failed)
            return;

        State = DownloadState.Canceled;
        ErrorType = DownloadErrorType.Canceled;
        CompletedAt = DateTime.Now;
    }
}