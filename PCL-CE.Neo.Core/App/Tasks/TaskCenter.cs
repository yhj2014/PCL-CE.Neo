using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.App.Tasks;

public static class TaskCenter
{
    private static readonly ILogger<TaskCenter> _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<TaskCenter>.Instance;

    public static ObservableCollection<TaskModel> Tasks { get; } = [];

    private static readonly ConditionalWeakTable<ITask, TaskModel> _ModelMap = [];

    private static TaskModel _InitModel(ITask instance)
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
            _logger.LogTrace("{Title}: state changed ({State}): {Message}", instance.Title, state, message);
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
                var taskModel = _InitModel(task);
                _ModelMap.Add(task, taskModel);
                model.Children.Add(taskModel);
            };
            group.RemoveTask += task =>
            {
                if (_ModelMap.TryGetValue(task, out var taskModel)) model.Children.Remove(taskModel);
            };
        }

        return model;
    }

    public static void Register(ITask instance, bool start = true)
    {
        var model = _InitModel(instance);
        Tasks.Add(model);

        if (start)
        {
            _ = Task.Run(async () =>
            {
                try { await instance.ExecuteAsync(); }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "{Title}: exception thrown", instance.Title);
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