namespace PCL_CE.Neo.Core.App.Tasks;

/// <summary>
/// 进度改变事件委托
/// </summary>
/// <param name="progress">当前进度（0.0 - 1.0）</param>
public delegate void ProgressChangedEvent(double progress);

/// <summary>
/// 有进度任务接口，支持实时报告任务执行进度
/// </summary>
public interface ITaskProgressive : ITask
{
    /// <summary>
    /// 进度改变事件
    /// </summary>
    public event ProgressChangedEvent? ProgressChanged;
}