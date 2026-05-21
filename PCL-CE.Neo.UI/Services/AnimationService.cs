namespace PCL_CE.Neo.UI.Services;

public class AnimationService : Core.Abstractions.IAnimationService
{
    public Task AnimateAsync(object element, Core.Abstractions.AnimationDescription description)
    {
#if WINDOWS || MACCATALYST || LINUX
        return AnimateElementAsync(element, description);
#else
        throw new PlatformNotSupportedException("AnimationService requires Uno Platform");
#endif
    }

#if WINDOWS || MACCATALYST || LINUX
    private async Task AnimateElementAsync(object element, Core.Abstractions.AnimationDescription description)
    {
        await Task.Delay(description.Duration);
        description.OnCompleted?.Invoke();
    }
#endif

    public void CancelAnimation(object element)
    {
#if WINDOWS || MACCATALYST || LINUX
        // Implementation depends on Uno Platform animation system
#endif
    }

    public bool IsAnimating(object element)
    {
        return false;
    }

    public Task FadeInAsync(object element, double duration = 300)
    {
#if WINDOWS || MACCATALYST || LINUX
        return AnimateAsync(element, new Core.Abstractions.AnimationDescription
        {
            PropertyName = "Opacity",
            FromValue = 0.0,
            ToValue = 1.0,
            Duration = TimeSpan.FromMilliseconds(duration),
            EasingType = Core.Abstractions.EasingType.CubicOut
        });
#else
        throw new PlatformNotSupportedException("AnimationService requires Uno Platform");
#endif
    }

    public Task FadeOutAsync(object element, double duration = 300)
    {
#if WINDOWS || MACCATALYST || LINUX
        return AnimateAsync(element, new Core.Abstractions.AnimationDescription
        {
            PropertyName = "Opacity",
            FromValue = 1.0,
            ToValue = 0.0,
            Duration = TimeSpan.FromMilliseconds(duration),
            EasingType = Core.Abstractions.EasingType.CubicIn
        });
#else
        throw new PlatformNotSupportedException("AnimationService requires Uno Platform");
#endif
    }

    public Task ScaleAsync(object element, double scale, double duration = 300)
    {
#if WINDOWS || MACCATALYST || LINUX
        return AnimateAsync(element, new Core.Abstractions.AnimationDescription
        {
            PropertyName = "Scale",
            ToValue = scale,
            Duration = TimeSpan.FromMilliseconds(duration),
            EasingType = Core.Abstractions.EasingType.ElasticOut
        });
#else
        throw new PlatformNotSupportedException("AnimationService requires Uno Platform");
#endif
    }

    public Task MoveToAsync(object element, double x, double y, double duration = 300)
    {
#if WINDOWS || MACCATALYST || LINUX
        return AnimateAsync(element, new Core.Abstractions.AnimationDescription
        {
            PropertyName = "Position",
            ToValue = new { X = x, Y = y },
            Duration = TimeSpan.FromMilliseconds(duration),
            EasingType = Core.Abstractions.EasingType.CubicOut
        });
#else
        throw new PlatformNotSupportedException("AnimationService requires Uno Platform");
#endif
    }
}
