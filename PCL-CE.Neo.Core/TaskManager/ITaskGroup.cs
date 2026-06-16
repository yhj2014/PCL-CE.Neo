namespace PCL_CE.Neo.Core.TaskManager;

public delegate void TaskGroupEvent(ITask task);

public interface ITaskGroup : ITask
{
    event TaskGroupEvent AddTask;
    event TaskGroupEvent RemoveTask;
}