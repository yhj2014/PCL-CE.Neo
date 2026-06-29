using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.TaskManager;

public interface ITaskCenter
{
    Task<string> AddTaskAsync(ITask task);
    Task<ITask?> GetTaskAsync(string taskId);
    Task<List<ITask>> GetAllTasksAsync();
    Task<List<ITask>> GetTasksByStateAsync(TaskState state);
    Task<bool> CancelTaskAsync(string taskId);
    Task<bool> RemoveTaskAsync(string taskId);
    Task ClearCompletedTasksAsync();
    
    event Action<ITask>? TaskAdded;
    event Action<ITask>? TaskCompleted;
    event Action<ITask>? TaskFailed;
    event Action<ITask>? TaskCanceled;
}

public class TaskCenter : ITaskCenter
{
    private readonly ConcurrentDictionary<string, ITask> _tasks = new();
    
    public event Action<ITask>? TaskAdded;
    public event Action<ITask>? TaskCompleted;
    public event Action<ITask>? TaskFailed;
    public event Action<ITask>? TaskCanceled;

    public async Task<string> AddTaskAsync(ITask task)
    {
        try
        {
            _tasks[task.Id] = task;
            TaskAdded?.Invoke(task);

            _ = Task.Run(async () =>
            {
                await task.StartAsync();
                
                switch (task.State)
                {
                    case TaskState.Completed:
                        TaskCompleted?.Invoke(task);
                        break;
                    case TaskState.Failed:
                        TaskFailed?.Invoke(task);
                        break;
                    case TaskState.Canceled:
                        TaskCanceled?.Invoke(task);
                        break;
                }
            });

            return task.Id;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to add task: {task.Name}");
            throw;
        }
    }

    public Task<ITask?> GetTaskAsync(string taskId)
    {
        try
        {
            _tasks.TryGetValue(taskId, out var task);
            return Task.FromResult(task);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to get task: {taskId}");
            return Task.FromResult<ITask?>(null);
        }
    }

    public Task<List<ITask>> GetAllTasksAsync()
    {
        try
        {
            return Task.FromResult(_tasks.Values.ToList());
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to get all tasks");
            return Task.FromResult(new List<ITask>());
        }
    }

    public Task<List<ITask>> GetTasksByStateAsync(TaskState state)
    {
        try
        {
            var tasks = _tasks.Values.Where(t => t.State == state).ToList();
            return Task.FromResult(tasks);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to get tasks by state: {state}");
            return Task.FromResult(new List<ITask>());
        }
    }

    public async Task<bool> CancelTaskAsync(string taskId)
    {
        try
        {
            if (_tasks.TryGetValue(taskId, out var task))
            {
                if (task is ITaskCancelable cancelableTask && cancelableTask.IsCancelable)
                {
                    task.Cancel();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to cancel task: {taskId}");
            return false;
        }
    }

    public Task<bool> RemoveTaskAsync(string taskId)
    {
        try
        {
            return Task.FromResult(_tasks.TryRemove(taskId, out _));
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to remove task: {taskId}");
            return Task.FromResult(false);
        }
    }

    public Task ClearCompletedTasksAsync()
    {
        try
        {
            var completedTasks = _tasks.Where(kv => 
                kv.Value.State == TaskState.Completed ||
                kv.Value.State == TaskState.Failed ||
                kv.Value.State == TaskState.Canceled);

            foreach (var pair in completedTasks)
            {
                _tasks.TryRemove(pair.Key, out _);
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to clear completed tasks");
        }

        return Task.CompletedTask;
    }
}