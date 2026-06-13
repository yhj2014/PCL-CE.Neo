namespace PCL_CE.Neo.Core.Abstractions.Mock;

/// <summary>
/// 动画服务的 Mock 实现
/// </summary>
public class AnimationServiceMock : IAnimationService
{
    private readonly List<object> _animatingElements = [];

    public async Task AnimateAsync(object element, AnimationDescription description)
    {
        if (!_animatingElements.Contains(element))
        {
            _animatingElements.Add(element);
        }
        await Task.Delay(description.Duration);
        _animatingElements.Remove(element);
        description.OnCompleted?.Invoke();
    }

    public void CancelAnimation(object element)
    {
        _animatingElements.Remove(element);
    }

    public bool IsAnimating(object element)
    {
        return _animatingElements.Contains(element);
    }

    public async Task FadeInAsync(object element, double duration = 300)
    {
        if (!_animatingElements.Contains(element))
        {
            _animatingElements.Add(element);
        }
        await Task.Delay(TimeSpan.FromMilliseconds(duration));
        _animatingElements.Remove(element);
    }

    public async Task FadeOutAsync(object element, double duration = 300)
    {
        if (!_animatingElements.Contains(element))
        {
            _animatingElements.Add(element);
        }
        await Task.Delay(TimeSpan.FromMilliseconds(duration));
        _animatingElements.Remove(element);
    }

    public async Task ScaleAsync(object element, double scale, double duration = 300)
    {
        if (!_animatingElements.Contains(element))
        {
            _animatingElements.Add(element);
        }
        await Task.Delay(TimeSpan.FromMilliseconds(duration));
        _animatingElements.Remove(element);
    }

    public async Task MoveToAsync(object element, double x, double y, double duration = 300)
    {
        if (!_animatingElements.Contains(element))
        {
            _animatingElements.Add(element);
        }
        await Task.Delay(TimeSpan.FromMilliseconds(duration));
        _animatingElements.Remove(element);
    }
}
