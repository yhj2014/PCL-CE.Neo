namespace PCL_CE.Neo.Core.Utils.Progress;

public interface IProgressReporter
{
    event EventHandler<ProgressEventArgs> ProgressChanged;
    event EventHandler<ProgressEventArgs> TaskCompleted;
    event EventHandler<ProgressEventArgs> TaskFailed;

    void ReportProgress(string taskId, string taskName, int percentage, long bytesProcessed, long totalBytes, string? message = null);
    void ReportIndeterminate(string taskId, string taskName, string? message = null);
    void ReportCompleted(string taskId, string taskName, string? message = null);
    void ReportFailed(string taskId, string taskName, string? errorMessage = null);
    void CancelTask(string taskId);
    bool IsTaskCanceled(string taskId);
}