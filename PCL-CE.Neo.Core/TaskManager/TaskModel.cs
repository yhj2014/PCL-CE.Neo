using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PCL_CE.Neo.Core.TaskManager;

public class TaskModel : INotifyPropertyChanged
{
    private TaskState _state = TaskState.Waiting;
    private string _stateMessage = string.Empty;
    private double _progress = 0.0;
    private bool _isGroup;

    public required string Title { get; init; }
    public required bool SupportProgress { get; init; }

    public TaskState State
    {
        get => _state;
        set
        {
            _state = value;
            OnPropertyChanged();
        }
    }

    public string StateMessage
    {
        get => _stateMessage;
        set
        {
            _stateMessage = value;
            OnPropertyChanged();
        }
    }

    public double Progress
    {
        get => _progress;
        set
        {
            _progress = value;
            OnPropertyChanged();
        }
    }

    public required Action? OnCancel { private get; init; }
    public required Action? OnPause { private get; init; }

    public bool IsGroup
    {
        get => _isGroup;
        set
        {
            _isGroup = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<TaskModel> Children { get; } = [];

    public TaskModel()
    {
        Children.CollectionChanged += (sender, _) =>
        {
            if (sender is ObservableCollection<TaskModel> c) IsGroup = c.Count > 0;
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}