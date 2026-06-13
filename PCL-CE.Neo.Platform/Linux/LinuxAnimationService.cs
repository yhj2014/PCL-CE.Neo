using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Linux;

public class LinuxAnimationService : IAnimationService
{
    private readonly ILogger<LinuxAnimationService> _logger;
    private readonly Dictionary<object, CancellationTokenSource> _activeAnimations = new();

    public LinuxAnimationService() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<LinuxAnimationService>.Instance)
    {
    }

    public LinuxAnimationService(ILogger<LinuxAnimationService> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("LinuxAnimationService initializing");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during LinuxAnimationService initialization");
        }
    }

    private static double Ease(AnimationDescription description, double progress)
    {
        try
        {
            switch (description.EasingType)
            {
                case EasingType.Linear:
                    return progress;
                case EasingType.QuadraticIn:
                    return progress * progress;
                case EasingType.QuadraticOut:
                    return 1.0 - (1 - progress) * (1 - progress);
                case EasingType.QuadraticInOut:
                    return progress < 0.5 ? 2 * progress * progress : 1 - Math.Pow(-2 * progress + 2, 2) / 2;
                case EasingType.CubicIn:
                    return progress * progress * progress;
                case EasingType.CubicOut:
                    return 1 - Math.Pow(1 - progress, 3);
                case EasingType.CubicInOut:
                    return progress < 0.5 ? 4 * progress * progress * progress : 1 - Math.Pow(-2 * progress + 2, 3) / 2;
                case EasingType.ElasticOut:
                    {
                        const double c4 = (2 * Math.PI) / 3;
                        return progress == 0 ? 0 : progress == 1 ? 1 : Math.Pow(2, -10 * progress) * Math.Sin((progress * 10 - 0.75) * c4) + 1;
                    }
                case EasingType.BounceOut:
                    {
                        const double n1 = 7.5625;
                        const double d1 = 2.75;
                        if (progress < 1 / d1) return n1 * progress * progress;
                        if (progress < 2 / d1) return n1 * (progress -= 1.5 / d1) * progress + 0.75;
                        if (progress < 2.5 / d1) return n1 * (progress -= 2.25 / d1) * progress + 0.9375;
                        return n1 * (progress -= 2.625 / d1) * progress + 0.984375;
                    }
                default:
                    return progress;
            }
        }
        catch
        {
            return progress;
        }
    }

    private static object Interpolate(object? from, object? to, double progress)
    {
        try
        {
            if (from is double d1 && to is double d2)
            {
                return d1 + (d2 - d1) * progress;
            }
            if (from is float f1 && to is float f2)
            {
                return f1 + (f2 - f1) * (float)progress;
            }
            if (from is int i1 && to is int i2)
            {
                return (int)(i1 + (i2 - i1) * progress);
            }

            return to ?? from ?? 0;
        }
        catch
        {
            return to ?? from ?? 0;
        }
    }

    public async Task AnimateAsync(object element, AnimationDescription description)
    {
        if (element == null || description == null)
        {
            _logger.LogWarning("AnimateAsync called with null element or description");
            return;
        }

        try
        {
            var cts = new CancellationTokenSource();
            _activeAnimations[element] = cts;

            _logger.LogDebug("Starting animation for element of type {Type}", element.GetType().Name);

            var durationMs = (long)description.Duration.TotalMilliseconds;
            if (durationMs <= 0) durationMs = 300;

            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddMilliseconds(durationMs);

            while (DateTime.UtcNow < endTime && !cts.Token.IsCancellationRequested)
            {
                var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
                var progress = Math.Min(1.0, elapsed / durationMs);
                var easedProgress = Ease(description, progress);

                if (description.PropertyName != null)
                {
                    var current = Interpolate(description.FromValue, description.ToValue, easedProgress);
                    _logger.LogDebug("Animating {Property}: {Progress:F3}", description.PropertyName, easedProgress);
                }

                await Task.Delay(16, cts.Token);
            }

            description.OnCompleted?.Invoke();
            _logger.LogDebug("Animation completed for element");
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Animation cancelled for element");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Animation failed for element");
        }
        finally
        {
            _activeAnimations.Remove(element);
        }
    }

    public void CancelAnimation(object element)
    {
        try
        {
            if (element == null) return;

            if (_activeAnimations.TryGetValue(element, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
                _activeAnimations.Remove(element);
                _logger.LogDebug("Animation cancelled");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cancel animation");
        }
    }

    public bool IsAnimating(object element)
    {
        try
        {
            return element != null && _activeAnimations.ContainsKey(element);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check animation state");
            return false;
        }
    }

    public async Task FadeInAsync(object element, double duration = 300)
    {
        try
        {
            if (element == null) return;

            _logger.LogDebug("FadeIn async started");
            var desc = new AnimationDescription
            {
                Duration = TimeSpan.FromMilliseconds(duration),
                FromValue = 0.0,
                ToValue = 1.0,
                EasingType = EasingType.QuadraticOut
            };

            await AnimateAsync(element, desc);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FadeInAsync failed");
        }
    }

    public async Task FadeOutAsync(object element, double duration = 300)
    {
        try
        {
            if (element == null) return;

            _logger.LogDebug("FadeOut async started");
            var desc = new AnimationDescription
            {
                Duration = TimeSpan.FromMilliseconds(duration),
                FromValue = 1.0,
                ToValue = 0.0,
                EasingType = EasingType.QuadraticOut
            };

            await AnimateAsync(element, desc);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FadeOutAsync failed");
        }
    }

    public async Task ScaleAsync(object element, double scale, double duration = 300)
    {
        try
        {
            if (element == null) return;

            _logger.LogDebug("Scale async started, target: {Scale}", scale);
            var desc = new AnimationDescription
            {
                Duration = TimeSpan.FromMilliseconds(duration),
                FromValue = 1.0,
                ToValue = scale,
                EasingType = EasingType.CubicOut
            };

            await AnimateAsync(element, desc);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ScaleAsync failed");
        }
    }

    public async Task MoveToAsync(object element, double x, double y, double duration = 300)
    {
        try
        {
            if (element == null) return;

            _logger.LogDebug("MoveTo async started, target: {X}, {Y}", x, y);
            var desc = new AnimationDescription
            {
                Duration = TimeSpan.FromMilliseconds(duration),
                FromValue = new { X = 0.0, Y = 0.0 },
                ToValue = new { X = x, Y = y },
                EasingType = EasingType.CubicOut
            };

            await AnimateAsync(element, desc);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MoveToAsync failed");
        }
    }
}
