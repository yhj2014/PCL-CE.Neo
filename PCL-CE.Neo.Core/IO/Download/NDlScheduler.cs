using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.IO.Download;

public class NDlScheduler
{
    private readonly ILogger<NDlScheduler> _logger;
    private readonly List<NDlTask> _tasks = [];
    private readonly object _lock = new();
    private int _maxConcurrentDownloads = 3;

    public NDlScheduler(ILogger<NDlScheduler> logger)
    {
        _logger = logger;
    }

    public int MaxConcurrentDownloads
    {
        get => _maxConcurrentDownloads;
        set
        {
            if (value < 1) value = 1;
            _maxConcurrentDownloads = value;
        }
    }

    public event Action<NDlTask>? TaskStateChanged;
    public event Action<NDlTask>? TaskProgressChanged;

    public NDlTask CreateTask(string id, string source, string target)
    {
        var task = new NDlTask { Id = id, Source = source, Target = target };
        lock (_lock)
        {
            _tasks.Add(task);
        }
        return task;
    }

    public void StartTask(NDlTask task)
    {
        lock (_lock)
        {
            if (task.State != NDlTaskState.Waiting && task.State != NDlTaskState.Paused)
                return;
            
            task.State = NDlTaskState.Downloading;
            TaskStateChanged?.Invoke(task);
        }
        
        _ = Task.Run(async () => await _ExecuteTaskAsync(task));
    }

    public void PauseTask(NDlTask task)
    {
        lock (_lock)
        {
            if (task.State == NDlTaskState.Downloading)
            {
                task.State = NDlTaskState.Paused;
                TaskStateChanged?.Invoke(task);
            }
        }
    }

    public void CancelTask(NDlTask task)
    {
        lock (_lock)
        {
            if (task.State != NDlTaskState.Completed && task.State != NDlTaskState.Failed)
            {
                task.State = NDlTaskState.Canceled;
                TaskStateChanged?.Invoke(task);
            }
        }
    }

    public void RemoveTask(NDlTask task)
    {
        lock (_lock)
        {
            _tasks.Remove(task);
        }
    }

    public IReadOnlyList<NDlTask> GetTasks()
    {
        lock (_lock)
        {
            return _tasks.ToList().AsReadOnly();
        }
    }

    private async Task _ExecuteTaskAsync(NDlTask task)
    {
        try
        {
            while (task.State == NDlTaskState.Downloading)
            {
                await Task.Delay(100);
                
                lock (_lock)
                {
                    if (task.TotalSize > 0 && task.DownloadedSize < task.TotalSize)
                    {
                        task.DownloadedSize += 1024;
                        if (task.DownloadedSize > task.TotalSize)
                            task.DownloadedSize = task.TotalSize;
                        TaskProgressChanged?.Invoke(task);
                        
                        if (task.DownloadedSize >= task.TotalSize)
                        {
                            task.State = NDlTaskState.Completed;
                            TaskStateChanged?.Invoke(task);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing download task {TaskId}", task.Id);
            lock (_lock)
            {
                task.State = NDlTaskState.Failed;
                TaskStateChanged?.Invoke(task);
            }
        }
    }
}