using System.Collections.Generic;

namespace PCL_CE.Neo.Core.IO.Download;

public class NDlTask
{
    public string Id { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public long TotalSize { get; set; }
    public long DownloadedSize { get; set; }
    public NDlTaskState State { get; set; } = NDlTaskState.Waiting;
    public List<NDlTaskSegment> Segments { get; } = [];
    public NDlSourceReport? SourceReport { get; set; }
}

public enum NDlTaskState
{
    Waiting,
    Downloading,
    Paused,
    Completed,
    Failed,
    Canceled
}