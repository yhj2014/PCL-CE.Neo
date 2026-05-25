namespace PCL_CE.Neo.Platform.Windows;

public class WindowsAnimationService : Core.Abstractions.IAnimationService
{
    public Task AnimateAsync(object element, Core.Abstractions.AnimationDescription description)
    {
        description.OnCompleted?.Invoke();
        return Task.CompletedTask;
    }

    public void CancelAnimation(object element)
    {
    }

    public bool IsAnimating(object element)
    {
        return false;
    }

    public Task FadeInAsync(object element, double duration = 300)
    {
        return Task.CompletedTask;
    }

    public Task FadeOutAsync(object element, double duration = 300)
    {
        return Task.CompletedTask;
    }

    public Task ScaleAsync(object element, double scale, double duration = 300)
    {
        return Task.CompletedTask;
    }

    public Task MoveToAsync(object element, double x, double y, double duration = 300)
    {
        return Task.CompletedTask;
    }
}
