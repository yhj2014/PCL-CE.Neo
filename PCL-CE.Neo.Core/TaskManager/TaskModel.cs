using System;
using System.Threading;

namespace PCL_CE.Neo.Core.TaskManager;

public interface ITask
{
    string Id { get; }
    string Name { get; }
    TaskState State { get; }
    double Progress { get; }
    string? Message { get; }
    CancellationTokenSource? CancellationTokenSource { get; }
    
    Task StartAsync();
    void Cancel();
}

public interface ITaskCancelable : ITask
{
    bool IsCancelable { get; }
}

public interface ITaskProgressive : ITask
{
    event Action<double, string?>? ProgressChanged;
}

public interface ITaskGroup : ITask
{
    void AddTask(ITask task);
    void RemoveTask(string taskId);
    int TaskCount { get; }
    int CompletedTaskCount { get; }
}

public enum TaskState
{
    Pending,
    Running,
    Completed,
    Failed,
    Canceled,
    Paused
}

public abstract class BaseTask : ITask, ITaskCancelable, ITaskProgressive
{
    public string Id { get; }
    public string Name { get; protected set; }
    public TaskState State { get; protected set; } = TaskState.Pending;
    public double Progress { get; protected set; } = 0;
    public string? Message { get; protected set; }
    public CancellationTokenSource? CancellationTokenSource { get; protected set; }
    public bool IsCancelable { get; protected set; } = true;
    
    public event Action<double, string?>? ProgressChanged;

    protected BaseTask(string name)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
    }

    public async Task StartAsync()
    {
        if (State != TaskState.Pending)
            throw new InvalidOperationException("Task has already been started");

        try
        {
            State = TaskState.Running;
            CancellationTokenSource = new CancellationTokenSource();
            
            await ExecuteAsync(CancellationTokenSource.Token);
            
            if (State == TaskState.Running)
                State = TaskState.Completed;
        }
        catch (OperationCanceledException)
        {
            State = TaskState.Canceled;
        }
        catch (Exception ex)
        {
            State = TaskState.Failed;
            Message = ex.Message;
            LogWrapper.Error(ex, $"Task {Name} failed");
            throw;
        }
    }

    public void Cancel()
    {
        if (!IsCancelable)
            throw new InvalidOperationException("Task is not cancelable");

        CancellationTokenSource?.Cancel();
        State = TaskState.Canceled;
    }

    protected abstract Task ExecuteAsync(CancellationToken cancellationToken);

    protected void UpdateProgress(double progress, string? message = null)
    {
        Progress = Math.Clamp(progress, 0, 100);
        Message = message;
        ProgressChanged?.Invoke(Progress, message);
    }
}