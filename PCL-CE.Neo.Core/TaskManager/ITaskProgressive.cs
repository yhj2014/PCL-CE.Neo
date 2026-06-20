namespace PCL_CE.Neo.Core.TaskManager;

public delegate void TaskProgressEvent(double progress);

public interface ITaskProgressive
{
    event TaskProgressEvent ProgressChanged;
}