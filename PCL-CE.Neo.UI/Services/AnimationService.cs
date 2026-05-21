namespace PCL_CE.Neo.UI.Services;

public class AnimationService : Core.Abstractions.IAnimationService
{
    private readonly Dictionary<object, CancellationTokenSource> _activeAnimations = new Dictionary<object, CancellationTokenSource>();

    public async Task AnimateAsync(object element, Core.Abstractions.AnimationDescription description)
    {
        var cts = new CancellationTokenSource();
        _activeAnimations[element] = cts;

        try
        {
            await AnimateElementAsync(element, description, cts.Token);
            description.OnCompleted?.Invoke();
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _activeAnimations.Remove(element);
        }
    }

    private async Task AnimateElementAsync(object element, Core.Abstractions.AnimationDescription description, CancellationToken cancellationToken)
    {
#if WINDOWS || MACCATALYST || LINUX
        await ExecuteAnimation(element, description, cancellationToken);
#else
        await ExecuteGenericAnimation(description, cancellationToken);
#endif
    }

    private async Task ExecuteAnimation(object element, Core.Abstractions.AnimationDescription description, CancellationToken cancellationToken)
    {
        var startTime = DateTime.Now;
        var duration = description.Duration;
        var easing = GetEasingFunction(description.EasingType);

        while (DateTime.Now - startTime < duration && !cancellationToken.IsCancellationRequested)
        {
            var elapsed = DateTime.Now - startTime;
            var progress = Math.Min(elapsed.TotalMilliseconds / duration.TotalMilliseconds, 1.0);
            var easedProgress = easing(progress);

            UpdateElementProperty(element, description, easedProgress);

            await Task.Delay(16, cancellationToken);
        }

        if (!cancellationToken.IsCancellationRequested)
        {
            UpdateElementProperty(element, description, 1.0);
        }
    }

    private async Task ExecuteGenericAnimation(Core.Abstractions.AnimationDescription description, CancellationToken cancellationToken)
    {
        var startTime = DateTime.Now;
        var duration = description.Duration;
        var easing = GetEasingFunction(description.EasingType);

        while (DateTime.Now - startTime < duration && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(16, cancellationToken);
        }
    }

    private void UpdateElementProperty(object element, Core.Abstractions.AnimationDescription description, double progress)
    {
#if WINDOWS
        UpdateWindowsElementProperty(element, description, progress);
#elif MACCATALYST
        UpdateMacOSElementProperty(element, description, progress);
#elif LINUX
        UpdateLinuxElementProperty(element, description, progress);
#endif
    }

#if WINDOWS
    private void UpdateWindowsElementProperty(object element, Core.Abstractions.AnimationDescription description, double progress)
    {
        try
        {
            if (element is System.Windows.UIElement uiElement)
            {
                switch (description.PropertyName)
                {
                    case "Opacity":
                        var fromOpacity = description.FromValue is double from ? from : 0.0;
                        var toOpacity = description.ToValue is double to ? to : 1.0;
                        uiElement.Opacity = fromOpacity + (toOpacity - fromOpacity) * progress;
                        break;

                    case "Scale":
                        var scaleTransform = uiElement.RenderTransform as System.Windows.Media.ScaleTransform
                            ?? new System.Windows.Media.ScaleTransform(1, 1);
                        var targetScale = description.ToValue is double scale ? scale : 1.0;
                        scaleTransform.ScaleX = targetScale * progress;
                        scaleTransform.ScaleY = targetScale * progress;
                        uiElement.RenderTransform = scaleTransform;
                        break;
                }
            }
        }
        catch
        {
        }
    }
#endif

#if MACCATALYST
    private void UpdateMacOSElementProperty(object element, Core.Abstractions.AnimationDescription description, double progress)
    {
    }
#endif

#if LINUX
    private void UpdateLinuxElementProperty(object element, Core.Abstractions.AnimationDescription description, double progress)
    {
    }
#endif

    public void CancelAnimation(object element)
    {
        if (_activeAnimations.TryGetValue(element, out var cts))
        {
            cts.Cancel();
            _activeAnimations.Remove(element);
        }
    }

    public bool IsAnimating(object element)
    {
        return _activeAnimations.ContainsKey(element);
    }

    public async Task FadeInAsync(object element, double duration = 300)
    {
        await AnimateAsync(element, new Core.Abstractions.AnimationDescription
        {
            PropertyName = "Opacity",
            FromValue = 0.0,
            ToValue = 1.0,
            Duration = TimeSpan.FromMilliseconds(duration),
            EasingType = Core.Abstractions.EasingType.CubicOut
        });
    }

    public async Task FadeOutAsync(object element, double duration = 300)
    {
        await AnimateAsync(element, new Core.Abstractions.AnimationDescription
        {
            PropertyName = "Opacity",
            FromValue = 1.0,
            ToValue = 0.0,
            Duration = TimeSpan.FromMilliseconds(duration),
            EasingType = Core.Abstractions.EasingType.CubicIn
        });
    }

    public async Task ScaleAsync(object element, double scale, double duration = 300)
    {
        await AnimateAsync(element, new Core.Abstractions.AnimationDescription
        {
            PropertyName = "Scale",
            ToValue = scale,
            Duration = TimeSpan.FromMilliseconds(duration),
            EasingType = Core.Abstractions.EasingType.ElasticOut
        });
    }

    public async Task MoveToAsync(object element, double x, double y, double duration = 300)
    {
        await AnimateAsync(element, new Core.Abstractions.AnimationDescription
        {
            PropertyName = "Position",
            ToValue = new { X = x, Y = y },
            Duration = TimeSpan.FromMilliseconds(duration),
            EasingType = Core.Abstractions.EasingType.CubicOut
        });
    }

    private Func<double, double> GetEasingFunction(Core.Abstractions.EasingType easingType)
    {
        return easingType switch
        {
            Core.Abstractions.EasingType.Linear => t => t,
            Core.Abstractions.EasingType.CubicIn => t => t * t * t,
            Core.Abstractions.EasingType.CubicOut => t => 1 - Math.Pow(1 - t, 3),
            Core.Abstractions.EasingType.CubicInOut => t => t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2,
            Core.Abstractions.EasingType.ElasticOut => ElasticOut,
            Core.Abstractions.EasingType.BounceOut => BounceOut,
            _ => t => t
        };
    }

    private double ElasticOut(double t)
    {
        if (t == 0 || t == 1) return t;
        var c4 = (2 * Math.PI) / 3;
        return Math.Pow(2, -10 * t) * Math.Sin((t * 10 - 0.75) * c4) + 1;
    }

    private double BounceOut(double t)
    {
        var n1 = 7.5625;
        var d1 = 2.75;
        if (t < 1 / d1)
            return n1 * t * t;
        if (t < 2 / d1)
            return n1 * (t -= 1.5 / d1) * t + 0.75;
        if (t < 2.5 / d1)
            return n1 * (t -= 2.25 / d1) * t + 0.9375;
        return n1 * (t -= 2.625 / d1) * t + 0.984375;
    }
}
