using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.App.Tasks;

/// <summary>
/// 可观察的任务模型，用于 UI 层绑定和显示
/// </summary>
public class TaskModel
{
    private readonly ILogger<TaskModel>? _logger;
    
    /// <summary>
    /// 任务标题
    /// </summary>
    public string Title { get; init; }
    
    /// <summary>
    /// 任务是否支持进度
    /// </summary>
    public bool SupportProgress { get; init; }

    private TaskState _state = TaskState.Waiting;
    /// <summary>
    /// 任务当前状态
    /// </summary>
    public TaskState State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                _state = value;
                StateChanged?.Invoke(value);
                _logger?.LogDebug("任务状态改变: {Title} -> {State}", Title, value);
            }
        }
    }

    private string _stateMessage = string.Empty;
    /// <summary>
    /// 任务当前状态信息
    /// </summary>
    public string StateMessage
    {
        get => _stateMessage;
        set
        {
            if (_stateMessage != value)
            {
                _stateMessage = value;
                StateMessageChanged?.Invoke(value);
            }
        }
    }

    private double _progress = 0.0;
    /// <summary>
    /// 任务当前进度（0.0 - 1.0），仅在 <see cref="SupportProgress"/> 为 true 时有效
    /// </summary>
    public double Progress
    {
        get => _progress;
        set
        {
            var clamped = Math.Clamp(value, 0.0, 1.0);
            if (_progress != clamped)
            {
                _progress = clamped;
                ProgressChanged?.Invoke(clamped);
            }
        }
    }

    /// <summary>
    /// 取消任务时触发的事件，值为 null 表示不支持取消
    /// </summary>
    public Action? OnCancel { get; init; }

    /// <summary>
    /// 暂停任务时触发的事件，值为 null 表示不支持暂停
    /// </summary>
    public Action? OnPause { get; init; }

    /// <summary>
    /// 恢复任务时触发的事件，值为 null 表示不支持恢复
    /// </summary>
    public Action? OnResume { get; init; }

    private bool _isGroup;
    /// <summary>
    /// 任务是否为任务组（是否有子任务）
    /// </summary>
    public bool IsGroup
    {
        get => _isGroup;
        private set
        {
            if (_isGroup != value)
            {
                _isGroup = value;
                IsGroupChanged?.Invoke(value);
            }
        }
    }

    /// <summary>
    /// 子任务模型集合
    /// </summary>
    public ObservableCollection<TaskModel> Children { get; } = new();

    /// <summary>
    /// 状态改变事件
    /// </summary>
    public event Action<TaskState>? StateChanged;
    
    /// <summary>
    /// 状态消息改变事件
    /// </summary>
    public event Action<string>? StateMessageChanged;
    
    /// <summary>
    /// 进度改变事件
    /// </summary>
    public event Action<double>? ProgressChanged;
    
    /// <summary>
    /// 是否任务组改变事件
    /// </summary>
    public event Action<bool>? IsGroupChanged;

    public TaskModel() : this(null, "")
    {
    }

    public TaskModel(ILogger<TaskModel>? logger, string title = "")
    {
        _logger = logger;
        Title = title ?? "";
        
        Children.CollectionChanged += (sender, e) =>
        {
            if (sender is ObservableCollection<TaskModel> collection)
            {
                IsGroup = collection.Count > 0;
                _logger?.LogDebug("子任务数量改变: {Title} -> {Count}", Title, collection.Count);
            }
        };
    }

    /// <summary>
    /// 取消任务
    /// </summary>
    public void Cancel()
    {
        try
        {
            if (OnCancel != null)
            {
                OnCancel.Invoke();
                State = TaskState.Cancelled;
                StateMessage = "任务已取消";
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "取消任务失败: {Title}", Title);
        }
    }

    /// <summary>
    /// 暂停任务
    /// </summary>
    public void Pause()
    {
        try
        {
            if (OnPause != null && State == TaskState.Running)
            {
                OnPause.Invoke();
                State = TaskState.Paused;
                StateMessage = "任务已暂停";
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "暂停任务失败: {Title}", Title);
        }
    }

    /// <summary>
    /// 恢复任务
    /// </summary>
    public void Resume()
    {
        try
        {
            if (OnResume != null && State == TaskState.Paused)
            {
                OnResume.Invoke();
                State = TaskState.Running;
                StateMessage = "任务已恢复";
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "恢复任务失败: {Title}", Title);
        }
    }
}