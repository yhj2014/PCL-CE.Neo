namespace PCL_CE.Neo.Core.App.Tasks;

/// <summary>
/// 可暂停任务接口，支持暂停和恢复任务执行
/// </summary>
public interface ITaskPausable : ITask
{
    /// <summary>
    /// 暂停任务
    /// </summary>
    public void Pause();
    
    /// <summary>
    /// 恢复任务
    /// </summary>
    public void Resume();
}