using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class TaskAdapter : ITaskAdapter
{
    private readonly ILogger<TaskAdapter> _logger;
    private readonly ConcurrentDictionary<string, TaskInfo> _tasks = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public event Action<ITaskInfo>? TaskStarted;
    public event Action<ITaskInfo>? TaskProgressChanged;
    public event Action<ITaskInfo, TaskResult>? TaskCompleted;

    public IReadOnlyList<ITaskInfo> RunningTasks => _tasks.Values.Where(t => t.State == Abstractions.TaskState.Running).ToList();
    public ITaskInfo? CurrentTask => RunningTasks.FirstOrDefault();

    public TaskAdapter(ILogger<TaskAdapter> logger)
    {
        _logger = logger;
    }

    public async Task<ITaskInfo> RunTaskAsync(string name, Func<IProgress<double>, Task> action, CancellationToken cancellationToken = default)
    {
        return await RunTaskAsync<object?>(name, async progress =>
        {
            await action(progress);
            return null;
        }, cancellationToken);
    }

    public async Task<ITaskInfo> RunTaskAsync<T>(string name, Func<IProgress<double>, Task<T>> action, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var info = new TaskInfo(id, name, cts);
        _tasks[id] = info;

        info.State = Abstractions.TaskState.Running;
        info.StartTime = DateTime.Now;
        TaskStarted?.Invoke(info);

        var progress = new Progress<double>(p =>
        {
            info.Progress = p;
            TaskProgressChanged?.Invoke(info);
        });

        var startTime = DateTime.Now;

        try
        {
            _logger.LogDebug("任务开始: {Name}", name);
            var result = await action(progress);
            var duration = DateTime.Now - startTime;

            info.State = Abstractions.TaskState.Completed;
            info.EndTime = DateTime.Now;
            info.Progress = 1.0;

            _logger.LogDebug("任务完成: {Name} ({Duration}ms)", name, duration.TotalMilliseconds);

            var taskResult = new TaskResult
            {
                Success = true,
                Result = result,
                Duration = duration
            };

            TaskCompleted?.Invoke(info, taskResult);
            return info;
        }
        catch (OperationCanceledException)
        {
            var duration = DateTime.Now - startTime;
            info.State = Abstractions.TaskState.Cancelled;
            info.EndTime = DateTime.Now;

            _logger.LogInformation("任务取消: {Name}", name);

            var taskResult = new TaskResult
            {
                Success = false,
                Error = new OperationCanceledException(),
                Duration = duration
            };

            TaskCompleted?.Invoke(info, taskResult);
            return info;
        }
        catch (Exception ex)
        {
            var duration = DateTime.Now - startTime;
            info.State = Abstractions.TaskState.Failed;
            info.EndTime = DateTime.Now;
            info.Error = ex;

            _logger.LogError(ex, "任务失败: {Name}", name);

            var taskResult = new TaskResult
            {
                Success = false,
                Error = ex,
                Duration = duration
            };

            TaskCompleted?.Invoke(info, taskResult);
            return info;
        }
        finally
        {
            _tasks.TryRemove(id, out _);
        }
    }

    public void CancelTask(ITaskInfo task)
    {
        if (task is TaskInfo info)
        {
            info.Cancel();
            _logger.LogInformation("任务已请求取消: {Name}", task.Name);
        }
    }

    public void CancelAllTasks()
    {
        foreach (var task in RunningTasks)
        {
            CancelTask(task);
        }
        _logger.LogInformation("已取消所有运行中的任务");
    }

    public async Task WaitAllTasksAsync(CancellationToken cancellationToken = default)
    {
        var tasks = RunningTasks.ToList();
        if (tasks.Count == 0) return;

        _logger.LogDebug("等待 {Count} 个任务完成", tasks.Count);

        while (RunningTasks.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(100, cancellationToken);
        }
    }

    private class TaskInfo : ITaskInfo
    {
        private readonly TaskAdapter _parent;
        private double _progress;
        private Abstractions.TaskState _state;

        public string Id { get; }
        public string Name { get; }
        public CancellationTokenSource CancellationTokenSource { get; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public Exception? Error { get; set; }

        public Abstractions.TaskState State
        {
            get => _state;
            set => _state = value;
        }

        public double Progress
        {
            get => _progress;
            set => _progress = Math.Clamp(value, 0, 1);
        }

        public string? Status { get; set; }

        public TaskInfo(string id, string name, CancellationTokenSource cts)
        {
            Id = id;
            Name = name;
            CancellationTokenSource = cts;
            _parent = null!;
        }

        public void ReportProgress(double progress, string? status = null)
        {
            Progress = progress;
            if (status != null) Status = status;
        }

        public void Cancel()
        {
            CancellationTokenSource.Cancel();
        }
    }
}
