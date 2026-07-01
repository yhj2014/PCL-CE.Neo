namespace PCL_CE.Neo.Core.Utils.Progress;

public class ProgressEventArgs : EventArgs
{
    public string TaskId { get; }
    public string TaskName { get; }
    public int Percentage { get; }
    public long BytesProcessed { get; }
    public long TotalBytes { get; }
    public string? Message { get; }
    public bool IsIndeterminate { get; }

    public ProgressEventArgs(string taskId, string taskName, int percentage, long bytesProcessed, long totalBytes, string? message = null)
    {
        TaskId = taskId;
        TaskName = taskName;
        Percentage = Math.Clamp(percentage, 0, 100);
        BytesProcessed = bytesProcessed;
        TotalBytes = totalBytes;
        Message = message;
        IsIndeterminate = totalBytes <= 0;
    }

    public ProgressEventArgs(string taskId, string taskName, bool isIndeterminate, string? message = null)
    {
        TaskId = taskId;
        TaskName = taskName;
        Percentage = 0;
        BytesProcessed = 0;
        TotalBytes = 0;
        Message = message;
        IsIndeterminate = isIndeterminate;
    }
}