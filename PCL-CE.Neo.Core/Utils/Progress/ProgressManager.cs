using System.Collections.Concurrent;

namespace PCL_CE.Neo.Core.Utils.Progress;

public class ProgressManager : IProgressReporter
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cancelTokens = new ConcurrentDictionary<string, CancellationTokenSource>();

    public event EventHandler<ProgressEventArgs>? ProgressChanged;
    public event EventHandler<ProgressEventArgs>? TaskCompleted;
    public event EventHandler<ProgressEventArgs>? TaskFailed;

    public void ReportProgress(string taskId, string taskName, int percentage, long bytesProcessed, long totalBytes, string? message = null)
    {
        if (IsTaskCanceled(taskId))
            return;

        var args = new ProgressEventArgs(taskId, taskName, percentage, bytesProcessed, totalBytes, message);
        ProgressChanged?.Invoke(this, args);
    }

    public void ReportIndeterminate(string taskId, string taskName, string? message = null)
    {
        if (IsTaskCanceled(taskId))
            return;

        var args = new ProgressEventArgs(taskId, taskName, true, message);
        ProgressChanged?.Invoke(this, args);
    }

    public void ReportCompleted(string taskId, string taskName, string? message = null)
    {
        var args = new ProgressEventArgs(taskId, taskName, 100, 0, 0, message ?? "任务完成");
        TaskCompleted?.Invoke(this, args);
        _cancelTokens.TryRemove(taskId, out _);
    }

    public void ReportFailed(string taskId, string taskName, string? errorMessage = null)
    {
        var args = new ProgressEventArgs(taskId, taskName, 0, 0, 0, errorMessage ?? "任务失败");
        TaskFailed?.Invoke(this, args);
        _cancelTokens.TryRemove(taskId, out _);
    }

    public void CancelTask(string taskId)
    {
        if (_cancelTokens.TryGetValue(taskId, out CancellationTokenSource? cts))
        {
            try
            {
                cts.Cancel();
            }
            catch
            {
            }
        }
    }

    public bool IsTaskCanceled(string taskId)
    {
        return _cancelTokens.TryGetValue(taskId, out CancellationTokenSource? cts) && cts.IsCancellationRequested;
    }

    public CancellationToken CreateTaskToken(string taskId)
    {
        var cts = new CancellationTokenSource();
        _cancelTokens[taskId] = cts;
        return cts.Token;
    }

    public void CleanupTask(string taskId)
    {
        if (_cancelTokens.TryRemove(taskId, out CancellationTokenSource? cts))
        {
            try
            {
                cts.Dispose();
            }
            catch
            {
            }
        }
    }

    public void CleanupAll()
    {
        foreach (var (_, cts) in _cancelTokens)
        {
            try
            {
                cts.Dispose();
            }
            catch
            {
            }
        }
        _cancelTokens.Clear();
    }
}