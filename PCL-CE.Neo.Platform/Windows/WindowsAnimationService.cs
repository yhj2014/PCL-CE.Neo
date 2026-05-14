using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

/// <summary>
/// Windows 平台动画服务实现
/// </summary>
public class WindowsAnimationService : IAnimationService
{
    private readonly List<object> _animatingElements = [];

    public async Task AnimateAsync(object element, AnimationDescription description)
    {
        if (!_animatingElements.Contains(element))
        {
            _animatingElements.Add(element);
        }

        // 在 Uno Platform/WinUI 中，可以通过反射或直接处理 UIElement 来应用动画
        // 为了兼容性，这里使用 Task.Delay 模拟动画执行
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
        var description = new AnimationDescription
        {
            PropertyName = "Opacity",
            FromValue = 0.0,
            ToValue = 1.0,
            Duration = TimeSpan.FromMilliseconds(duration),
            EasingType = EasingType.CubicOut
        };
        await AnimateAsync(element, description);
    }

    public async Task FadeOutAsync(object element, double duration = 300)
    {
        var description = new AnimationDescription
        {
            PropertyName = "Opacity",
            FromValue = 1.0,
            ToValue = 0.0,
            Duration = TimeSpan.FromMilliseconds(duration),
            EasingType = EasingType.CubicIn
        };
        await AnimateAsync(element, description);
    }

    public async Task ScaleAsync(object element, double scale, double duration = 300)
    {
        var description = new AnimationDescription
        {
            PropertyName = "Scale",
            ToValue = scale,
            Duration = TimeSpan.FromMilliseconds(duration),
            EasingType = EasingType.ElasticOut
        };
        await AnimateAsync(element, description);
    }

    public async Task MoveToAsync(object element, double x, double y, double duration = 300)
    {
        var description = new AnimationDescription
        {
            PropertyName = "Translation",
            ToValue = new { X = x, Y = y },
            Duration = TimeSpan.FromMilliseconds(duration),
            EasingType = EasingType.CubicOut
        };
        await AnimateAsync(element, description);
    }
}
