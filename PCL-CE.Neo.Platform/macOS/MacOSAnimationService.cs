using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.macOS;

public class MacOSAnimationService : IAnimationService
{
    private readonly ILogger<MacOSAnimationService> _logger;
    private readonly ConcurrentDictionary<object, CancellationTokenSource> _activeAnimations;

    public MacOSAnimationService() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<MacOSAnimationService>.Instance) { }

    public MacOSAnimationService(ILogger<MacOSAnimationService> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("Initializing macOS animation service");
            _activeAnimations = new ConcurrentDictionary<object, CancellationTokenSource>();
            _logger.LogInformation("macOS animation service initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing macOS animation service");
        }
    }

    public Task AnimateAsync(object element, AnimationDescription description)
    {
        try
        {
            if (element == null)
            {
                _logger.LogWarning("Attempted to animate null element, ignored");
                return Task.CompletedTask;
            }

            if (description == null)
            {
                _logger.LogWarning("Animation description is null, ignored");
                return Task.CompletedTask;
            }

            _logger.LogDebug("Starting animation, element: {Element}, property: {Prop}, duration: {Duration}ms",
                element.GetHashCode(), description.PropertyName, description.Duration.TotalMilliseconds);

            if (_activeAnimations.TryGetValue(element, out var existingCts))
            {
                _logger.LogDebug("Existing animation found on element, cancelling first");
                try
                {
                    existingCts.Cancel();
                    existingCts.Dispose();
                }
                catch { }
                _activeAnimations.TryRemove(element, out _);
            }

            var cts = new CancellationTokenSource();
            _activeAnimations[element] = cts;

            var task = Task.Run(async () =>
            {
                try
                {
                    var durationMs = description.Duration.TotalMilliseconds;
                    if (durationMs <= 0)
                    {
                        durationMs = 300;
                    }

                    var frameInterval = 16;
                    var totalFrames = (int)Math.Ceiling(durationMs / frameInterval);
                    double fromValue = 0;
                    double toValue = 1.0;

                    if (description.FromValue != null)
                    {
                        double.TryParse(description.FromValue.ToString(), out fromValue);
                    }

                    if (description.ToValue != null)
                    {
                        double.TryParse(description.ToValue.ToString(), out toValue);
                    }

                    _logger.LogDebug("Animation details: From={From}, To={To}, Frames={Frames}, easing={Easing}",
                        fromValue, toValue, totalFrames, description.EasingType);

                    for (var frame = 0; frame < totalFrames; frame++)
                    {
                        if (cts.Token.IsCancellationRequested)
                        {
                            _logger.LogDebug("Animation cancelled, element: {Element}", element.GetHashCode());
                            return;
                        }

                        var progress = (double)frame / totalFrames;
                        var easedProgress = ApplyEasing(progress, description.EasingType);
                        var currentValue = fromValue + (toValue - fromValue) * easedProgress;

                        try
                        {
                            await Task.Delay(frameInterval, cts.Token).ContinueWith(t => { }, cts.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogDebug("Animation delay cancelled, element: {Element}", element.GetHashCode());
                            return;
                        }
                    }

                    _logger.LogInformation("Animation completed, element: {Element}, property: {Prop}",
                        element.GetHashCode(), description.PropertyName);

                    description.OnCompleted?.Invoke();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during animation execution, element: {Element}", element.GetHashCode());
                }
                finally
                {
                    if (_activeAnimations.TryRemove(element, out var removedCts) && removedCts == cts)
                    {
                        cts.Dispose();
                    }
                }
            }, cts.Token);

            return task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating animation task");
            return Task.CompletedTask;
        }
    }

    public void CancelAnimation(object element)
    {
        try
        {
            if (element == null)
            {
                _logger.LogWarning("Attempted to cancel animation on null element, ignored");
                return;
            }

            if (_activeAnimations.TryRemove(element, out var cts))
            {
                try
                {
                    cts.Cancel();
                    cts.Dispose();
                    _logger.LogInformation("Cancelled animation on element: {Element}", element.GetHashCode());
                }
                catch { }
            }
            else
            {
                _logger.LogDebug("No active animation on element: {Element}", element.GetHashCode());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling animation");
        }
    }

    public bool IsAnimating(object element)
    {
        try
        {
            if (element == null)
            {
                return false;
            }

            var isAnimating = _activeAnimations.ContainsKey(element);
            _logger.LogDebug("Animation status for element {Element}: {State}", element.GetHashCode(), isAnimating);
            return isAnimating;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking animation status");
            return false;
        }
    }

    public Task FadeInAsync(object element, double duration = 300)
    {
        try
        {
            _logger.LogDebug("Fade-in animation, element: {Element}, duration: {Duration}ms", element?.GetHashCode(), duration);
            var description = new AnimationDescription
            {
                PropertyName = "Opacity",
                FromValue = 0.0,
                ToValue = 1.0,
                Duration = TimeSpan.FromMilliseconds(duration),
                EasingType = EasingType.QuadraticInOut
            };
            return AnimateAsync(element, description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating fade-in animation");
            return Task.CompletedTask;
        }
    }

    public Task FadeOutAsync(object element, double duration = 300)
    {
        try
        {
            _logger.LogDebug("Fade-out animation, element: {Element}, duration: {Duration}ms", element?.GetHashCode(), duration);
            var description = new AnimationDescription
            {
                PropertyName = "Opacity",
                FromValue = 1.0,
                ToValue = 0.0,
                Duration = TimeSpan.FromMilliseconds(duration),
                EasingType = EasingType.QuadraticInOut
            };
            return AnimateAsync(element, description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating fade-out animation");
            return Task.CompletedTask;
        }
    }

    public Task ScaleAsync(object element, double scale, double duration = 300)
    {
        try
        {
            _logger.LogDebug("Scale animation, element: {Element}, target: {Scale}, duration: {Duration}ms",
                element?.GetHashCode(), scale, duration);
            var description = new AnimationDescription
            {
                PropertyName = "Scale",
                FromValue = 1.0,
                ToValue = scale,
                Duration = TimeSpan.FromMilliseconds(duration),
                EasingType = EasingType.CubicOut
            };
            return AnimateAsync(element, description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating scale animation");
            return Task.CompletedTask;
        }
    }

    public Task MoveToAsync(object element, double x, double y, double duration = 300)
    {
        try
        {
            _logger.LogDebug("Position animation, element: {Element}, target: ({X}, {Y}), duration: {Duration}ms",
                element?.GetHashCode(), x, y, duration);
            var description = new AnimationDescription
            {
                PropertyName = "Position",
                FromValue = 0.0,
                ToValue = 1.0,
                Duration = TimeSpan.FromMilliseconds(duration),
                EasingType = EasingType.CubicInOut
            };
            return AnimateAsync(element, description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating position animation");
            return Task.CompletedTask;
        }
    }

    private static double ApplyEasing(double progress, EasingType easingType)
    {
        return easingType switch
        {
            EasingType.Linear => progress,
            EasingType.QuadraticIn => progress * progress,
            EasingType.QuadraticOut => 1.0 - (1.0 - progress) * (1.0 - progress),
            EasingType.QuadraticInOut => progress < 0.5 ? 2 * progress * progress : 1.0 - Math.Pow(-2 * progress + 2, 2) / 2,
            EasingType.CubicIn => progress * progress * progress,
            EasingType.CubicOut => 1.0 - Math.Pow(1.0 - progress, 3),
            EasingType.CubicInOut => progress < 0.5 ? 4 * progress * progress * progress : 1.0 - Math.Pow(-2 * progress + 2, 3) / 2,
            EasingType.ElasticOut => Math.Pow(2, -10 * progress) * Math.Sin((progress - 0.1) * 2 * Math.PI / 0.4) + 1.0,
            EasingType.BounceOut => CalculateBounce(progress),
            _ => progress
        };
    }

    private static double CalculateBounce(double progress)
    {
        const double n1 = 7.5625;
        const double d1 = 2.75;

        if (progress < 1 / d1)
        {
            return n1 * progress * progress;
        }
        else if (progress < 2 / d1)
        {
            progress -= 1.5 / d1;
            return n1 * progress * progress + 0.75;
        }
        else if (progress < 2.5 / d1)
        {
            progress -= 2.25 / d1;
            return n1 * progress * progress + 0.9375;
        }
        else
        {
            progress -= 2.625 / d1;
            return n1 * progress * progress + 0.984375;
        }
    }
}
