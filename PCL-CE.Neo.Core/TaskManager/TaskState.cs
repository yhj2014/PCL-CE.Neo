namespace PCL_CE.Neo.Core.TaskManager;

public enum TaskState
{
    Waiting,
    Running,
    Paused,
    Completed,
    Failed,
    Cancelled
}