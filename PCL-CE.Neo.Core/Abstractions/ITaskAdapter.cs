namespace PCL_CE.Neo.Core.Abstractions;

public interface ITaskAdapter
{
    event Action<ITaskInfo>? TaskStarted;
    event Action<ITaskInfo>? TaskProgressChanged;
    event Action<ITaskInfo, TaskResult>? TaskCompleted;

    IReadOnlyList<ITaskInfo> RunningTasks { get; }
    ITaskInfo? CurrentTask { get; }

    Task<ITaskInfo> RunTaskAsync(string name, Func<IProgress<double>, Task> action, CancellationToken cancellationToken = default);
    Task<ITaskInfo> RunTaskAsync<T>(string name, Func<IProgress<double>, Task<T>> action, CancellationToken cancellationToken = default);

    void CancelTask(ITaskInfo task);
    void CancelAllTasks();

    Task WaitAllTasksAsync(CancellationToken cancellationToken = default);
}

public interface ITaskInfo
{
    string Id { get; }
    string Name { get; }
    TaskState State { get; }
    double Progress { get; }
    string? Status { get; }
    Exception? Error { get; }
    DateTime StartTime { get; }
    DateTime? EndTime { get; }
    CancellationTokenSource? CancellationTokenSource { get; }

    void ReportProgress(double progress, string? status = null);
    void Cancel();
}

public enum TaskState
{
    Pending,
    Running,
    Paused,
    Completed,
    Failed,
    Cancelled
}

public record TaskResult
{
    public bool Success { get; init; }
    public object? Result { get; init; }
    public Exception? Error { get; init; }
    public TimeSpan Duration { get; init; }
}

public interface ITaskGroup
{
    string Name { get; }
    IReadOnlyList<ITaskInfo> Tasks { get; }
    double OverallProgress { get; }
    TaskState State { get; }

    Task<ITaskGroup> StartAsync();
    void Pause();
    void Resume();
    void Cancel();
}
