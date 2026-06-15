using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsAnimationService : IAnimationService
{
    private readonly ILogger<WindowsAnimationService> _logger;
    private readonly Dictionary<object, CancellationTokenSource> _activeAnimations;
    private readonly object _lock = new();

    public WindowsAnimationService() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<WindowsAnimationService>.Instance)
    {
    }

    public WindowsAnimationService(ILogger<WindowsAnimationService> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("正在初始化 Windows 动画服务");
            _activeAnimations = new Dictionary<object, CancellationTokenSource>();
            _logger.LogInformation("Windows 动画服务初始化完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化 Windows 动画服务时发生错误");
        }
    }

    public Task AnimateAsync(object element, AnimationDescription description)
    {
        try
        {
            if (element == null)
            {
                _logger.LogWarning("尝试对空元素执行动画，已忽略");
                return Task.CompletedTask;
            }

            if (description == null)
            {
                _logger.LogWarning("动画描述为空，已忽略");
                return Task.CompletedTask;
            }

            _logger.LogDebug("开始执行动画，元素: {Element}, 属性: {Prop}, 时长: {Duration}ms",
                element.GetHashCode(), description.PropertyName, description.Duration.TotalMilliseconds);

            CancellationTokenSource? cts;
            lock (_lock)
            {
                if (_activeAnimations.ContainsKey(element))
                {
                    _logger.LogDebug("元素上已有运行中的动画，先取消");
                    _activeAnimations[element].Cancel();
                    _activeAnimations[element].Dispose();
                    _activeAnimations.Remove(element);
                }

                cts = new CancellationTokenSource();
                _activeAnimations[element] = cts;
            }

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
                    double frameStart = 0;
                    double frameTarget = 1.0;
                    double fromValue = 0;
                    double toValue = 1.0;

                    if (description.FromValue != null)
                    {
                        double.TryParse(description.FromValue.ToString(), out fromValue);
                        frameStart = fromValue;
                    }

                    if (description.ToValue != null)
                    {
                        double.TryParse(description.ToValue.ToString(), out toValue);
                        frameTarget = toValue;
                    }

                    _logger.LogDebug("动画详情: From={From}, To={To}, Frames={Frames}, 缓动: {Easing}",
                        fromValue, toValue, totalFrames, description.EasingType);

                    for (var frame = 0; frame < totalFrames; frame++)
                    {
                        if (cts.Token.IsCancellationRequested)
                        {
                            _logger.LogDebug("动画被取消，元素: {Element}", element.GetHashCode());
                            return;
                        }

                        var progress = (double)frame / totalFrames;
                        var easedProgress = ApplyEasing(progress, description.EasingType);
                        var currentValue = fromValue + (toValue - fromValue) * easedProgress;

                        await Task.Delay(frameInterval, cts.Token).ContinueWith(t => { }, cts.Token);
                    }

                    _logger.LogInformation("动画完成，元素: {Element}, 属性: {Prop}",
                        element.GetHashCode(), description.PropertyName);

                    description.OnCompleted?.Invoke();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "执行动画时发生错误，元素: {Element}", element.GetHashCode());
                }
                finally
                {
                    lock (_lock)
                    {
                        if (_activeAnimations.TryGetValue(element, out var existing) && existing == cts)
                        {
                            _activeAnimations.Remove(element);
                        }
                    }
                    cts.Dispose();
                }
            }, cts.Token);

            return task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建动画任务时发生错误");
            return Task.CompletedTask;
        }
    }

    public void CancelAnimation(object element)
    {
        try
        {
            if (element == null)
            {
                _logger.LogWarning("尝试取消空元素的动画，已忽略");
                return;
            }

            lock (_lock)
            {
                if (_activeAnimations.TryGetValue(element, out var cts))
                {
                    cts.Cancel();
                    cts.Dispose();
                    _activeAnimations.Remove(element);
                    _logger.LogInformation("已取消元素 {Element} 的动画", element.GetHashCode());
                }
                else
                {
                    _logger.LogDebug("元素 {Element} 上没有活动的动画", element.GetHashCode());
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取消动画时发生错误");
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

            lock (_lock)
            {
                var isAnimating = _activeAnimations.ContainsKey(element);
                _logger.LogDebug("元素 {Element} 的动画状态: {State}", element.GetHashCode(), isAnimating);
                return isAnimating;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查动画状态时发生错误");
            return false;
        }
    }

    public Task FadeInAsync(object element, double duration = 300)
    {
        try
        {
            _logger.LogDebug("淡入动画，元素: {Element}, 时长: {Duration}ms", element?.GetHashCode(), duration);
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
            _logger.LogError(ex, "创建淡入动画时发生错误");
            return Task.CompletedTask;
        }
    }

    public Task FadeOutAsync(object element, double duration = 300)
    {
        try
        {
            _logger.LogDebug("淡出动画，元素: {Element}, 时长: {Duration}ms", element?.GetHashCode(), duration);
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
            _logger.LogError(ex, "创建淡出动画时发生错误");
            return Task.CompletedTask;
        }
    }

    public Task ScaleAsync(object element, double scale, double duration = 300)
    {
        try
        {
            _logger.LogDebug("缩放动画，元素: {Element}, 目标: {Scale}, 时长: {Duration}ms",
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
            _logger.LogError(ex, "创建缩放动画时发生错误");
            return Task.CompletedTask;
        }
    }

    public Task MoveToAsync(object element, double x, double y, double duration = 300)
    {
        try
        {
            _logger.LogDebug("位置动画，元素: {Element}, 目标: ({X}, {Y}), 时长: {Duration}ms",
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
            _logger.LogError(ex, "创建位置动画时发生错误");
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
