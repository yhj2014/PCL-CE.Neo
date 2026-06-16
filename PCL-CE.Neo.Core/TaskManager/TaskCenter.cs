using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.TaskManager;

public static class TaskCenter
{
    public static ObservableCollection<TaskModel> Tasks { get; } = [];

    private static readonly ConditionalWeakTable<ITask, TaskModel> ModelMap = [];

    private static TaskModel InitModel(ITask instance)
    {
        var cancelable = instance as ITaskCancelable;
        var pausable = instance as ITaskPausable;
        var progressive = instance as ITaskProgressive;

        var model = new TaskModel
        {
            Title = instance.Title,
            SupportProgress = progressive != null,
            OnCancel = cancelable == null ? null : (() => cancelable.Cancel()),
            OnPause = pausable == null ? null : (() => pausable.Pause()),
        };

        instance.StateChanged += (state, message) =>
        {
            LogWrapper.Trace("TaskCenter", $"{instance.Title}: state changed ({state}): {message}");
            model.State = state;
            model.StateMessage = message;
        };

        if (progressive != null)
        {
            progressive.ProgressChanged += progress =>
            {
                model.Progress = Math.Clamp(progress, 0.0, 1.0);
            };
        }

        if (instance is ITaskGroup group)
        {
            group.AddTask += task =>
            {
                var taskModel = InitModel(task);
                ModelMap.Add(task, taskModel);
                model.Children.Add(taskModel);
            };
            group.RemoveTask += task =>
            {
                if (ModelMap.TryGetValue(task, out var taskModel)) model.Children.Remove(taskModel);
            };
        }

        return model;
    }

    public static void Register(ITask instance, bool start = true)
    {
        var model = InitModel(instance);
        Tasks.Add(model);

        if (start)
        {
            _ = Task.Run(async () =>
            {
                try { await instance.ExecuteAsync(); }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    LogWrapper.Warn(ex, "TaskCenter", $"{instance.Title}: exception thrown");
                    model.State = TaskState.Failed;
                    model.StateMessage = ex.Message;
                }
            });
        }
    }

    public static void RemoveFinished()
    {
        foreach (var model in Tasks.Where(x => x.State > TaskState.Running).ToList())
            Tasks.Remove(model);
    }
}