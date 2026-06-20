using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.TaskManager;

public delegate void TaskStateEvent(TaskState state, string message);

public interface ITask
{
    string Title { get; }
    Task ExecuteAsync(CancellationToken cancelToken = default);
    event TaskStateEvent StateChanged;
}