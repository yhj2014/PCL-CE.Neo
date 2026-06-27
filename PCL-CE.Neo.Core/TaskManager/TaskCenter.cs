using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.TaskManager;

/// <summary>
/// 任务状态
/// </summary>
public enum TaskState
{
    /// <summary>等待中</summary>
    Waiting,
    /// <summary>运行中</summary>
    Running,
    /// <summary>成功</summary>
    Success,
    /// <summary>已取消</summary>
    Canceled,
    /// <summary>失败</summary>
    Failed
}

/// <summary>
/// 任务状态改变事件
/// </summary>
public delegate void TaskStateEvent(TaskState state, string message);

/// <summary>
/// 任务进度改变事件
/// </summary>
public delegate void TaskProgressEvent(double progress);

/// <summary>
/// 任务组事件
/// </summary>
public delegate void TaskGroupEvent(ITask task);

/// <summary>
/// 响应式任务接口
/// </summary>
public interface ITask
{
    /// <summary>
    /// 任务标题
    /// </summary>
    string Title { get; }

    /// <summary>
    /// 运行任务
    /// </summary>
    Task ExecuteAsync(CancellationToken cancelToken = default);

    /// <summary>
    /// 任务状态改变事件
    /// </summary>
    event TaskStateEvent StateChanged;
}

/// <summary>
/// 可取消任务接口
/// </summary>
public interface ITaskCancelable
{
    /// <summary>
    /// 取消任务
    /// </summary>
    void Cancel();
}

/// <summary>
/// 可暂停任务接口
/// </summary>
public interface ITaskPausable
{
    /// <summary>
    /// 暂停任务
    /// </summary>
    void Pause();
    
    /// <summary>
    /// 继续任务
    /// </summary>
    void Resume();
}

/// <summary>
/// 可观察进度的任务接口
/// </summary>
public interface ITaskProgressive
{
    /// <summary>
    /// 任务进度改变事件
    /// </summary>
    event TaskProgressEvent ProgressChanged;
}

/// <summary>
/// 任务组接口
/// </summary>
public interface ITaskGroup : ITask
{
    /// <summary>
    /// 添加子任务事件
    /// </summary>
    event TaskGroupEvent AddTask;
    
    /// <summary>
    /// 移除子任务事件
    /// </summary>
    event TaskGroupEvent RemoveTask;
}

/// <summary>
/// 可观察的任务模型
/// </summary>
public class TaskModel
{
    /// <summary>
    /// 任务标题
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// 任务是否支持进度
    /// </summary>
    public required bool SupportProgress { get; init; }

    /// <summary>
    /// 任务当前状态
    /// </summary>
    public TaskState State { get; set; } = TaskState.Waiting;

    /// <summary>
    /// 任务当前状态信息
    /// </summary>
    public string StateMessage { get; set; } = string.Empty;

    /// <summary>
    /// 任务当前进度
    /// </summary>
    public double Progress { get; set; } = 0.0;

    /// <summary>
    /// 取消任务时触发的事件，值为 null 表示不支持取消
    /// </summary>
    public Action? OnCancel { get; init; }

    /// <summary>
    /// 暂停任务时触发的事件，值为 null 表示不支持暂停
    /// </summary>
    public Action? OnPause { get; init; }

    /// <summary>
    /// 任务是否为任务组
    /// </summary>
    public bool IsGroup { get; set; }

    /// <summary>
    /// 子任务模型
    /// </summary>
    public ObservableCollection<TaskModel> Children { get; } = new();

    public TaskModel()
    {
        Children.CollectionChanged += (sender, _) =>
        {
            if (sender is ObservableCollection<TaskModel> c) 
                IsGroup = c.Count > 0;
        };
    }
}

/// <summary>
/// 任务中心，用于管理任务
/// </summary>
public sealed class TaskCenter
{
    private readonly ILogger<TaskCenter> _logger;
    
    /// <summary>
    /// 可观察的任务模型集合
    /// </summary>
    public ObservableCollection<TaskModel> Tasks { get; } = new();

    private readonly ConditionalWeakTable<ITask, TaskModel> _modelMap = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public TaskCenter(ILogger<TaskCenter> logger)
    {
        _logger = logger;
    }

    private TaskModel _InitModel(ITask instance)
    {
        var cancelable = instance as ITaskCancelable;
        var pausable = instance as ITaskPausable;
        var progressive = instance as ITaskProgressive;

        var model = new TaskModel
        {
            Title = instance.Title,
            SupportProgress = progressive != null,
            OnCancel = cancelable == null ? null : (() => cancelable.Cancel()),
            OnPause = pausable == null ? null : (() => pausable.Pause()),
        };

        // 状态事件
        instance.StateChanged += (state, message) =>
        {
            _logger.LogTrace("TaskCenter: {Title} 状态改变 ({State}): {Message}", instance.Title, state, message);
            model.State = state;
            model.StateMessage = message;
        };

        // 进度事件
        if (progressive != null)
        {
            progressive.ProgressChanged += progress =>
            {
                model.Progress = Math.Clamp(progress, 0.0, 1.0);
            };
        }

        // 组事件
        if (instance is ITaskGroup group)
        {
            group.AddTask += task =>
            {
                var taskModel = _InitModel(task);
                _modelMap.Add(task, taskModel);
                model.Children.Add(taskModel);
            };
            group.RemoveTask += task =>
            {
                if (_modelMap.TryGetValue(task, out var taskModel))
                    model.Children.Remove(taskModel);
            };
        }

        return model;
    }

    /// <summary>
    /// 注册响应式任务实例
    /// </summary>
    /// <param name="instance">任务实例</param>
    /// <param name="start">是否立即启动该实例</param>
    public async Task RegisterAsync(ITask instance, bool start = true)
    {
        await _lock.WaitAsync();
        try
        {
            var model = _InitModel(instance);
            _modelMap.Add(instance, model);
            Tasks.Add(model);

            if (start)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await instance.ExecuteAsync();
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("TaskCenter: {Title} 已取消", instance.Title);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "TaskCenter: {Title} 抛出异常", instance.Title);
                        model.State = TaskState.Failed;
                        model.StateMessage = ex.Message;
                    }
                });
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// 注册响应式任务实例（同步版本）
    /// </summary>
    public void Register(ITask instance, bool start = true)
    {
        _ = RegisterAsync(instance, start);
    }

    /// <summary>
    /// 移除所有已结束的任务
    /// </summary>
    public async Task RemoveFinishedAsync()
    {
        await _lock.WaitAsync();
        try
        {
            var finished = Tasks.Where(x => x.State > TaskState.Running).ToList();
            foreach (var model in finished)
                Tasks.Remove(model);
            
            _logger.LogDebug("TaskCenter: 已移除 {Count} 个已结束的任务", finished.Count);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// 移除所有已结束的任务（同步版本）
    /// </summary>
    public void RemoveFinished()
    {
        _ = RemoveFinishedAsync();
    }

    /// <summary>
    /// 取消所有正在运行的任务
    /// </summary>
    public async Task CancelAllAsync()
    {
        await _lock.WaitAsync();
        try
        {
            var running = Tasks.Where(x => x.State == TaskState.Running).ToList();
            foreach (var model in running)
            {
                model.OnCancel?.Invoke();
            }
            
            _logger.LogInformation("TaskCenter: 已取消 {Count} 个正在运行的任务", running.Count);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// 取消所有正在运行的任务（同步版本）
    /// </summary>
    public void CancelAll()
    {
        _ = CancelAllAsync();
    }

    /// <summary>
    /// 获取任务统计信息
    /// </summary>
    public TaskStatistics GetStatistics()
    {
        return new TaskStatistics
        {
            Total = Tasks.Count,
            Waiting = Tasks.Count(x => x.State == TaskState.Waiting),
            Running = Tasks.Count(x => x.State == TaskState.Running),
            Success = Tasks.Count(x => x.State == TaskState.Success),
            Failed = Tasks.Count(x => x.State == TaskState.Failed),
            Canceled = Tasks.Count(x => x.State == TaskState.Canceled)
        };
    }
}

/// <summary>
/// 任务统计信息
/// </summary>
public class TaskStatistics
{
    public int Total { get; set; }
    public int Waiting { get; set; }
    public int Running { get; set; }
    public int Success { get; set; }
    public int Failed { get; set; }
    public int Canceled { get; set; }
}