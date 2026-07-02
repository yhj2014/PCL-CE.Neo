namespace PCL_CE.Neo.Core.App.Tasks;

/// <summary>
/// 可取消任务接口，支持用户主动取消任务
/// </summary>
public interface ITaskCancelable : ITask
{
    /// <summary>
    /// 取消任务
    /// </summary>
    public void Cancel();
}