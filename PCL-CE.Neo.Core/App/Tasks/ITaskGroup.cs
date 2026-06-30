namespace PCL_CE.Neo.Core.App.Tasks;

public interface ITaskGroup : ITask
{
    event TaskAddedEventHandler AddTask;
    event TaskRemovedEventHandler RemoveTask;
}

public delegate void TaskAddedEventHandler(ITask task);
public delegate void TaskRemovedEventHandler(ITask task);