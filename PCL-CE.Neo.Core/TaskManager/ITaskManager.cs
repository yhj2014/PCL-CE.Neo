namespace PCL_CE.Neo.Core.TaskManager;

public enum TaskState
{
    Pending,
    Running,
    Paused,
    Completed,
    Failed,
    Cancelled
}

public interface ITask
{
    string Id { get; }
    string Name { get; }
    TaskState State { get; }
    double Progress { get; }
    string? ErrorMessage { get; }
    Exception? Exception { get; }
    CancellationToken CancellationToken { get; }
    event Action<ITask>? StateChanged;
    event Action<ITask, double>? ProgressChanged;
    Task StartAsync();
    void Pause();
    void Resume();
    void Cancel();
}

public abstract class TaskBase : ITask
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public virtual TaskState State { get; protected set; } = TaskState.Pending;
    public virtual double Progress { get; protected set; }
    public virtual string? ErrorMessage { get; protected set; }
    public virtual Exception? Exception { get; protected set; }
    public CancellationTokenSource CancellationTokenSource { get; } = new();
    public CancellationToken CancellationToken => CancellationTokenSource.Token;

    public event Action<ITask>? StateChanged;
    public event Action<ITask, double>? ProgressChanged;

    protected void SetProgress(double progress)
    {
        Progress = progress;
        ProgressChanged?.Invoke(this, progress);
    }

    protected void SetState(TaskState state)
    {
        State = state;
        StateChanged?.Invoke(this);
    }

    public abstract Task StartAsync();

    private Task? _runningTask;

    public void Start()
    {
        SetState(TaskState.Running);
        _runningTask = Task.Run(async () =>
        {
            try
            {
                await StartAsync();
                if (State == TaskState.Running)
                {
                    SetState(TaskState.Completed);
                }
            }
            catch (OperationCanceledException)
            {
                if (State != TaskState.Cancelled)
                {
                    SetState(TaskState.Cancelled);
                }
            }
            catch (Exception ex)
            {
                if (State != TaskState.Cancelled)
                {
                    Fail("Task failed", ex);
                }
            }
        });
    }

    public virtual void Pause()
    {
        if (State == TaskState.Running)
        {
            SetState(TaskState.Paused);
        }
    }

    public virtual void Resume()
    {
        if (State == TaskState.Paused)
        {
            SetState(TaskState.Running);
        }
    }

    public virtual void Cancel()
    {
        CancellationTokenSource.Cancel();
        if (State == TaskState.Running || State == TaskState.Paused)
        {
            SetState(TaskState.Cancelled);
        }
    }

    protected void Fail(string message, Exception? ex = null)
    {
        ErrorMessage = message;
        Exception = ex;
        SetState(TaskState.Failed);
    }

    protected void Complete()
    {
        SetProgress(1.0);
        SetState(TaskState.Completed);
    }
}

public interface ITaskManager
{
    IReadOnlyCollection<ITask> Tasks { get; }
    ITask? CurrentTask { get; }
    event Action<ITask>? TaskAdded;
    event Action<ITask>? TaskRemoved;
    event Action<ITask>? TaskStarted;
    event Action<ITask>? TaskCompleted;
    void Add(ITask task);
    void Remove(ITask task);
    void Start(ITask task);
    void CancelAll();
}

public class TaskManager : ITaskManager
{
    private readonly List<ITask> _tasks = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public IReadOnlyCollection<ITask> Tasks => _tasks.AsReadOnly();
    public ITask? CurrentTask { get; private set; }

    public event Action<ITask>? TaskAdded;
    public event Action<ITask>? TaskRemoved;
    public event Action<ITask>? TaskStarted;
    public event Action<ITask>? TaskCompleted;

    public void Add(ITask task)
    {
        lock (_lock)
        {
            _tasks.Add(task);
        }
        TaskAdded?.Invoke(task);
    }

    public void Remove(ITask task)
    {
        lock (_lock)
        {
            _tasks.Remove(task);
        }
        TaskRemoved?.Invoke(task);
    }

    public async void Start(ITask task)
    {
        CurrentTask = task;
        TaskStarted?.Invoke(task);
        
        try
        {
            await task.StartAsync();
            TaskCompleted?.Invoke(task);
        }
        catch (OperationCanceledException)
        {
            task.Cancel();
        }
        finally
        {
            if (CurrentTask == task)
            {
                CurrentTask = null;
            }
        }
    }

    public void CancelAll()
    {
        lock (_lock)
        {
            foreach (var task in _tasks)
            {
                task.Cancel();
            }
        }
    }
}
