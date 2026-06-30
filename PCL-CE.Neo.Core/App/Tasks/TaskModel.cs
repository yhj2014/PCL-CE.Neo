using System.Collections.ObjectModel;

namespace PCL_CE.Neo.Core.App.Tasks;

public class TaskModel
{
    public string Title { get; set; } = "";
    public TaskState State { get; set; } = TaskState.Ready;
    public string? StateMessage { get; set; }
    public double Progress { get; set; }
    public bool SupportProgress { get; set; }
    public Action? OnCancel { get; set; }
    public Action? OnPause { get; set; }
    public ObservableCollection<TaskModel> Children { get; } = [];
}