namespace PCL_CE.Neo.Core.Abstractions;

/// <summary>
/// 动画描述对象，包含动画的各种属性
/// </summary>
public class AnimationDescription
{
    /// <summary>
    /// 动画持续时间
    /// </summary>
    public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(0.3);

    /// <summary>
    /// 目标属性名称
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// 动画目标值
    /// </summary>
    public object? ToValue { get; set; }

    /// <summary>
    /// 动画起始值（可选，不设置时从当前值开始）
    /// </summary>
    public object? FromValue { get; set; }

    /// <summary>
    /// 动画缓动函数类型
    /// </summary>
    public EasingType EasingType { get; set; } = EasingType.Linear;

    /// <summary>
    /// 动画曲线（可选，自定义缓动函数）
    /// </summary>
    public List<double>? KeyFrames { get; set; }

    /// <summary>
    /// 动画完成回调
    /// </summary>
    public Action? OnCompleted { get; set; }
}

/// <summary>
/// 缓动函数类型枚举
/// </summary>
public enum EasingType
{
    /// <summary>
    /// 线性缓动
    /// </summary>
    Linear,
    /// <summary>
    /// 二次方缓入
    /// </summary>
    QuadraticIn,
    /// <summary>
    /// 二次方缓出
    /// </summary>
    QuadraticOut,
    /// <summary>
    /// 二次方缓入缓出
    /// </summary>
    QuadraticInOut,
    /// <summary>
    /// 三次方缓入
    /// </summary>
    CubicIn,
    /// <summary>
    /// 三次方缓出
    /// </summary>
    CubicOut,
    /// <summary>
    /// 三次方缓入缓出
    /// </summary>
    CubicInOut,
    /// <summary>
    /// 弹性缓出
    /// </summary>
    ElasticOut,
    /// <summary>
    /// 回弹缓出
    /// </summary>
    BounceOut
}

/// <summary>
/// 平台无关的动画服务
/// </summary>
public interface IAnimationService
{
    /// <summary>
    /// 对指定元素执行动画
    /// </summary>
    /// <param name="element">要执行动画的 UI 元素（object 以保持平台无关）</param>
    /// <param name="description">动画描述对象</param>
    /// <returns>动画任务</returns>
    Task AnimateAsync(object element, AnimationDescription description);

    /// <summary>
    /// 取消指定元素的动画
    /// </summary>
    /// <param name="element">要取消动画的 UI 元素</param>
    void CancelAnimation(object element);

    /// <summary>
    /// 检查指定元素是否正在动画
    /// </summary>
    /// <param name="element">要检查的 UI 元素</param>
    /// <returns>是否正在动画</returns>
    bool IsAnimating(object element);

    /// <summary>
    /// 快速应用淡入动画
    /// </summary>
    /// <param name="element">UI 元素</param>
    /// <param name="duration">持续时间（毫秒）</param>
    /// <returns>动画任务</returns>
    Task FadeInAsync(object element, double duration = 300);

    /// <summary>
    /// 快速应用淡出动画
    /// </summary>
    /// <param name="element">UI 元素</param>
    /// <param name="duration">持续时间（毫秒）</param>
    /// <returns>动画任务</returns>
    Task FadeOutAsync(object element, double duration = 300);

    /// <summary>
    /// 快速应用缩放动画
    /// </summary>
    /// <param name="element">UI 元素</param>
    /// <param name="scale">目标缩放比例</param>
    /// <param name="duration">持续时间（毫秒）</param>
    /// <returns>动画任务</returns>
    Task ScaleAsync(object element, double scale, double duration = 300);

    /// <summary>
    /// 快速应用位置动画
    /// </summary>
    /// <param name="element">UI 元素</param>
    /// <param name="x">目标 X 位置</param>
    /// <param name="y">目标 Y 位置</param>
    /// <param name="duration">持续时间（毫秒）</param>
    /// <returns>动画任务</returns>
    Task MoveToAsync(object element, double x, double y, double duration = 300);
}
