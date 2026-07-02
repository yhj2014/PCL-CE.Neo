using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.App.Tasks;

/// <summary>
/// 任务中心，用于统一管理所有任务的生命周期和状态
/// </summary>
public class TaskCenter
{
    private readonly ILogger<TaskCenter>? _logger;
    private readonly ConditionalWeakTable<ITask, TaskModel> _modelMap = new();
    
    /// <summary>
    /// 可观察的任务模型集合
    /// </summary>
    public ObservableCollection<TaskModel> Tasks { get; } = new();

    public TaskCenter() : this(null)
    {
    }

    public TaskCenter(ILogger<TaskCenter>? logger)
    {
        _logger = logger;
        _logger?.LogInformation("任务中心已初始化");
    }

    /// <summary>
    /// 初始化任务模型并绑定事件
    /// </summary>
    private TaskModel InitModel(ITask instance)
    {
        var cancelable = instance as ITaskCancelable;
        var pausable = instance as ITaskPausable;
        var progressive = instance as ITaskProgressive;

        var model = new TaskModel(_logger as ILogger<TaskModel>, instance.Title)
        {
            SupportProgress = progressive != null,
            OnCancel = cancelable == null ? null : () => cancelable.Cancel(),
            OnPause = pausable == null ? null : () => pausable.Pause(),
            OnResume = pausable == null ? null : () => pausable.Resume(),
        };

        // 状态事件绑定
        instance.StateChanged += (state, message) =>
        {
            _logger?.LogDebug("任务状态改变: {Title} -> {State}: {Message}", instance.Title, state, message);
            model.State = state;
            model.StateMessage = message;
        };

        // 进度事件绑定
        if (progressive != null)
        {
            progressive.ProgressChanged += progress =>
            {
                model.Progress = Math.Clamp(progress, 0.0, 1.0);
                _logger?.LogTrace("任务进度更新: {Title} -> {Progress:P}", instance.Title, progress);
            };
        }

        // 任务组事件绑定
        if (instance is ITaskGroup group)
        {
            group.AddTask += task =>
            {
                try
                {
                    var taskModel = InitModel(task);
                    _modelMap.Add(task, taskModel);
                    model.Children.Add(taskModel);
                    _logger?.LogDebug("子任务添加: {ParentTitle} -> {ChildTitle}", instance.Title, task.Title);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "添加子任务失败: {Title}", task.Title);
                }
            };
            
            group.RemoveTask += task =>
            {
                try
                {
                    if (_modelMap.TryGetValue(task, out var taskModel))
                    {
                        model.Children.Remove(taskModel);
                        _logger?.LogDebug("子任务移除: {ParentTitle} -> {ChildTitle}", instance.Title, task.Title);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "移除子任务失败: {Title}", task.Title);
                }
            };
        }

        return model;
    }

    /// <summary>
    /// 注册响应式任务实例
    /// </summary>
    /// <param name="instance">任务实例</param>
    /// <param name="start">是否立即启动该实例</param>
    public void Register(ITask instance, bool start = true)
    {
        try
        {
            var model = InitModel(instance);
            Tasks.Add(model);
            _logger?.LogInformation("任务注册: {Title}", instance.Title);

            if (start)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        model.State = TaskState.Running;
                        model.StateMessage = "任务开始执行";
                        await instance.ExecuteAsync();
                        model.State = TaskState.Completed;
                        model.StateMessage = "任务完成";
                        _logger?.LogInformation("任务完成: {Title}", instance.Title);
                    }
                    catch (OperationCanceledException)
                    {
                        model.State = TaskState.Cancelled;
                        model.StateMessage = "任务已取消";
                        _logger?.LogInformation("任务取消: {Title}", instance.Title);
                    }
                    catch (Exception ex)
                    {
                        model.State = TaskState.Failed;
                        model.StateMessage = ex.Message;
                        _logger?.LogError(ex, "任务失败: {Title}", instance.Title);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "注册任务失败: {Title}", instance.Title);
        }
    }

    /// <summary>
    /// 移除所有已结束的任务
    /// </summary>
    public void RemoveFinished()
    {
        try
        {
            var finishedModels = Tasks
                .Where(x => x.State == TaskState.Completed || 
                            x.State == TaskState.Failed || 
                            x.State == TaskState.Cancelled)
                .ToList();

            foreach (var model in finishedModels)
            {
                Tasks.Remove(model);
                _logger?.LogDebug("移除已结束任务: {Title}", model.Title);
            }

            _logger?.LogInformation("移除 {Count} 个已结束任务", finishedModels.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "移除已结束任务失败");
        }
    }

    /// <summary>
    /// 取消所有运行中的任务
    /// </summary>
    public void CancelAll()
    {
        try
        {
            var runningModels = Tasks.Where(x => x.State == TaskState.Running).ToList();
            foreach (var model in runningModels)
            {
                model.Cancel();
            }

            _logger?.LogInformation("取消 {Count} 个运行中的任务", runningModels.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "取消所有任务失败");
        }
    }

    /// <summary>
    /// 获取任务数量统计
    /// </summary>
    public TaskStatistics GetStatistics()
    {
        return new TaskStatistics
        {
            Total = Tasks.Count,
            Waiting = Tasks.Count(x => x.State == TaskState.Waiting),
            Running = Tasks.Count(x => x.State == TaskState.Running),
            Paused = Tasks.Count(x => x.State == TaskState.Paused),
            Completed = Tasks.Count(x => x.State == TaskState.Completed),
            Failed = Tasks.Count(x => x.State == TaskState.Failed),
            Cancelled = Tasks.Count(x => x.State == TaskState.Cancelled)
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
    public int Paused { get; set; }
    public int Completed { get; set; }
    public int Failed { get; set; }
    public int Cancelled { get; set; }
}