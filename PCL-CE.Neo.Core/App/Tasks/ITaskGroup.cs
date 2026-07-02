namespace PCL_CE.Neo.Core.App.Tasks;

/// <summary>
/// 任务添加事件委托
/// </summary>
/// <param name="task">添加的子任务</param>
public delegate void TaskAddEvent(ITask task);

/// <summary>
/// 任务移除事件委托
/// </summary>
/// <param name="task">移除的子任务</param>
public delegate void TaskRemoveEvent(ITask task);

/// <summary>
/// 任务组接口，支持包含子任务的复合任务
/// </summary>
public interface ITaskGroup : ITask
{
    /// <summary>
    /// 添加子任务事件
    /// </summary>
    public event TaskAddEvent? AddTask;
    
    /// <summary>
    /// 移除子任务事件
    /// </summary>
    public event TaskRemoveEvent? RemoveTask;
}