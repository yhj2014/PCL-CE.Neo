namespace PCL_CE.Neo.Core.Utils.Download;

public enum DownloadStatus
{
    Pending = 0,
    Downloading = 1,
    Paused = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}