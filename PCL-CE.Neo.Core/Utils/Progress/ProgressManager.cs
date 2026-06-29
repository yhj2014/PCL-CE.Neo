using System;
using System.Collections.Concurrent;

namespace PCL_CE.Neo.Core.Utils.Progress;

public interface IProgressManager
{
    void Register(string id, string title, int totalSteps = 100);
    void Update(string id, int progress, string? message = null);
    void Complete(string id, string? message = null);
    void Fail(string id, string? errorMessage = null);
    ProgressState? GetProgress(string id);
    void Remove(string id);
    void Clear();
}

public class ProgressManager : IProgressManager
{
    private readonly ConcurrentDictionary<string, ProgressState> _progressStates = new();

    public void Register(string id, string title, int totalSteps = 100)
    {
        try
        {
            _progressStates[id] = new ProgressState
            {
                Id = id,
                Title = title,
                TotalSteps = totalSteps,
                CurrentProgress = 0,
                Status = ProgressStatus.Running,
                Message = "Initializing..."
            };
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to register progress: {id}");
        }
    }

    public void Update(string id, int progress, string? message = null)
    {
        try
        {
            if (_progressStates.TryGetValue(id, out var state))
            {
                state.CurrentProgress = Math.Clamp(progress, 0, state.TotalSteps);
                state.Percentage = (int)((state.CurrentProgress / (double)state.TotalSteps) * 100);
                
                if (!string.IsNullOrEmpty(message))
                    state.Message = message;
                
                state.LastUpdated = DateTime.Now;
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to update progress: {id}");
        }
    }

    public void Complete(string id, string? message = null)
    {
        try
        {
            if (_progressStates.TryGetValue(id, out var state))
            {
                state.CurrentProgress = state.TotalSteps;
                state.Percentage = 100;
                state.Status = ProgressStatus.Completed;
                
                if (!string.IsNullOrEmpty(message))
                    state.Message = message;
                else
                    state.Message = "Completed";
                
                state.LastUpdated = DateTime.Now;
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to mark progress as complete: {id}");
        }
    }

    public void Fail(string id, string? errorMessage = null)
    {
        try
        {
            if (_progressStates.TryGetValue(id, out var state))
            {
                state.Status = ProgressStatus.Failed;
                
                if (!string.IsNullOrEmpty(errorMessage))
                    state.Message = errorMessage;
                else
                    state.Message = "Failed";
                
                state.LastUpdated = DateTime.Now;
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to mark progress as failed: {id}");
        }
    }

    public ProgressState? GetProgress(string id)
    {
        try
        {
            return _progressStates.TryGetValue(id, out var state) ? state : null;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to get progress: {id}");
            return null;
        }
    }

    public void Remove(string id)
    {
        try
        {
            _progressStates.TryRemove(id, out _);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to remove progress: {id}");
        }
    }

    public void Clear()
    {
        try
        {
            _progressStates.Clear();
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to clear progress states");
        }
    }
}

public enum ProgressStatus
{
    Running,
    Completed,
    Failed,
    Paused
}

public class ProgressState
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int TotalSteps { get; set; } = 100;
    public int CurrentProgress { get; set; } = 0;
    public int Percentage { get; set; } = 0;
    public ProgressStatus Status { get; set; } = ProgressStatus.Running;
    public string Message { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}