using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.App.Tasks;

public interface ITask
{
    string Title { get; }
    event TaskStateChangedEventHandler StateChanged;
    Task ExecuteAsync();
}

public delegate void TaskStateChangedEventHandler(TaskState state, string? message);