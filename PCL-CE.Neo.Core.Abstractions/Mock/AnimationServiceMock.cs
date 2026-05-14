namespace PCL_CE.Neo.Core.Abstractions.Mock;

/// <summary>
/// 动画服务的 Mock 实现
/// </summary>
public class AnimationServiceMock : IAnimationService
{
    private readonly List<object> _animatingElements = [];

    public Task AnimateAsync(object element, AnimationDescription description)
    {
        if (!_animatingElements.Contains(element))
        {
            _animatingElements.Add(element);
        }
        return Task.Delay(description.Duration);
    }

    public void CancelAnimation(object element)
    {
        _animatingElements.Remove(element);
    }

    public bool IsAnimating(object element)
    {
        return _animatingElements.Contains(element);
    }

    public Task FadeInAsync(object element, double duration = 300)
    {
        return Task.Delay(TimeSpan.FromMilliseconds(duration));
    }

    public Task FadeOutAsync(object element, double duration = 300)
    {
        return Task.Delay(TimeSpan.FromMilliseconds(duration));
    }

    public Task ScaleAsync(object element, double scale, double duration = 300)
    {
        return Task.Delay(TimeSpan.FromMilliseconds(duration));
    }

    public Task MoveToAsync(object element, double x, double y, double duration = 300)
    {
        return Task.Delay(TimeSpan.FromMilliseconds(duration));
    }
}
