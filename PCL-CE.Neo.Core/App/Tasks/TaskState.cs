namespace PCL_CE.Neo.Core.App.Tasks;

/// <summary>
/// 任务状态枚举，用于标识任务的当前执行状态
/// </summary>
public enum TaskState
{
    /// <summary>
    /// 等待中，任务尚未开始执行
    /// </summary>
    Waiting = 0,
    
    /// <summary>
    /// 运行中，任务正在执行
    /// </summary>
    Running = 1,
    
    /// <summary>
    /// 已暂停，任务暂停执行（支持暂停的任务）
    /// </summary>
    Paused = 2,
    
    /// <summary>
    /// 已完成，任务成功完成
    /// </summary>
    Completed = 3,
    
    /// <summary>
    /// 已失败，任务执行失败
    /// </summary>
    Failed = 4,
    
    /// <summary>
    /// 已取消，任务被用户取消
    /// </summary>
    Cancelled = 5
}