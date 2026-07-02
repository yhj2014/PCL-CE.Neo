using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.App.Tasks;

/// <summary>
/// 任务状态改变事件委托
/// </summary>
/// <param name="state">当前状态</param>
/// <param name="message">状态消息</param>
public delegate void TaskStateEvent(TaskState state, string message);

/// <summary>
/// 响应式任务接口，所有任务必须实现此接口
/// </summary>
public interface ITask
{
    /// <summary>
    /// 任务标题
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// 执行任务
    /// </summary>
    /// <param name="cancelToken">取消令牌</param>
    /// <returns>任务执行结果</returns>
    public Task ExecuteAsync(CancellationToken cancelToken = default);

    /// <summary>
    /// 任务状态改变事件
    /// </summary>
    public event TaskStateEvent? StateChanged;
}