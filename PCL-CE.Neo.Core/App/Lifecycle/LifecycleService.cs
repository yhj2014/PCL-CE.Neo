using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.App.Lifecycle;

/// <summary>
/// 应用生命周期阶段
/// </summary>
public enum LifecycleStage
{
    /// <summary>
    /// 未初始化
    /// </summary>
    Uninitialized = 0,
    
    /// <summary>
    /// 正在初始化
    /// </summary>
    Initializing = 1,
    
    /// <summary>
    /// 已初始化（准备就绪）
    /// </summary>
    Initialized = 2,
    
    /// <summary>
    /// 正在启动
    /// </summary>
    Starting = 3,
    
    /// <summary>
    /// 正在运行
    /// </summary>
    Running = 4,
    
    /// <summary>
    /// 正在停止
    /// </summary>
    Stopping = 5,
    
    /// <summary>
    /// 已停止
    /// </summary>
    Stopped = 6,
    
    /// <summary>
    /// 错误状态
    /// </summary>
    Error = 7
}

/// <summary>
/// 生命周期事件委托
/// </summary>
/// <param name="previousStage">前一个阶段</param>
/// <param name="newStage">新阶段</param>
public delegate void LifecycleStageChangedEvent(LifecycleStage previousStage, LifecycleStage newStage);

/// <summary>
/// 生命周期钩子委托
/// </summary>
/// <param name="cancellationToken">取消令牌</param>
public delegate Task LifecycleHook(CancellationToken cancellationToken);

/// <summary>
/// 生命周期服务，管理应用启动、停止等生命周期事件
/// </summary>
public class LifecycleService
{
    private readonly ILogger<LifecycleService>? _logger;
    private readonly SemaphoreSlim _transitionLock = new(1, 1);
    
    private LifecycleStage _currentStage = LifecycleStage.Uninitialized;
    private readonly List<LifecycleHook> _initHooks = new();
    private readonly List<LifecycleHook> _startHooks = new();
    private readonly List<LifecycleHook> _stopHooks = new();
    private readonly List<LifecycleHook> _shutdownHooks = new();

    /// <summary>
    /// 当前生命周期阶段
    /// </summary>
    public LifecycleStage CurrentStage => _currentStage;

    /// <summary>
    /// 生命周期阶段改变事件
    /// </summary>
    public event LifecycleStageChangedEvent? StageChanged;

    /// <summary>
    /// 应用错误事件
    /// </summary>
    public event Action<Exception>? ApplicationError;

    public LifecycleService() : this(null)
    {
    }

    public LifecycleService(ILogger<LifecycleService>? logger)
    {
        _logger = logger;
        _logger?.LogInformation("生命周期服务已创建");
    }

    /// <summary>
    /// 注册初始化钩子（在初始化阶段执行）
    /// </summary>
    /// <param name="hook">钩子函数</param>
    public void RegisterInitHook(LifecycleHook hook)
    {
        if (hook == null) throw new ArgumentNullException(nameof(hook));
        _initHooks.Add(hook);
        _logger?.LogDebug("注册初始化钩子: {Count}", _initHooks.Count);
    }

    /// <summary>
    /// 注册启动钩子（在启动阶段执行）
    /// </summary>
    /// <param name="hook">钩子函数</param>
    public void RegisterStartHook(LifecycleHook hook)
    {
        if (hook == null) throw new ArgumentNullException(nameof(hook));
        _startHooks.Add(hook);
        _logger?.LogDebug("注册启动钩子: {Count}", _startHooks.Count);
    }

    /// <summary>
    /// 注册停止钩子（在停止阶段执行）
    /// </summary>
    /// <param name="hook">钩子函数</param>
    public void RegisterStopHook(LifecycleHook hook)
    {
        if (hook == null) throw new ArgumentNullException(nameof(hook));
        _stopHooks.Add(hook);
        _logger?.LogDebug("注册停止钩子: {Count}", _stopHooks.Count);
    }

    /// <summary>
    /// 注册关闭钩子（在应用完全关闭时执行）
    /// </summary>
    /// <param name="hook">钩子函数</param>
    public void RegisterShutdownHook(LifecycleHook hook)
    {
        if (hook == null) throw new ArgumentNullException(nameof(hook));
        _shutdownHooks.Add(hook);
        _logger?.LogDebug("注册关闭钩子: {Count}", _shutdownHooks.Count);
    }

    /// <summary>
    /// 初始化应用
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await TransitionToStageAsync(
            LifecycleStage.Initializing,
            LifecycleStage.Initialized,
            _initHooks,
            cancellationToken
        );
    }

    /// <summary>
    /// 启动应用
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_currentStage < LifecycleStage.Initialized)
        {
            await InitializeAsync(cancellationToken);
        }

        await TransitionToStageAsync(
            LifecycleStage.Starting,
            LifecycleStage.Running,
            _startHooks,
            cancellationToken
        );
    }

    /// <summary>
    /// 停止应用
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await TransitionToStageAsync(
            LifecycleStage.Stopping,
            LifecycleStage.Stopped,
            _stopHooks,
            cancellationToken
        );
    }

    /// <summary>
    /// 完全关闭应用（停止并执行关闭钩子）
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentStage == LifecycleStage.Running)
            {
                await StopAsync(cancellationToken);
            }

            _logger?.LogInformation("执行关闭钩子");
            foreach (var hook in _shutdownHooks)
            {
                try
                {
                    await hook(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "关闭钩子执行失败");
                }
            }

            _logger?.LogInformation("应用已完全关闭");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "关闭应用失败");
            ApplicationError?.Invoke(ex);
        }
    }

    /// <summary>
    /// 执行生命周期阶段转换
    /// </summary>
    private async Task TransitionToStageAsync(
        LifecycleStage transitionStage,
        LifecycleStage targetStage,
        List<LifecycleHook> hooks,
        CancellationToken cancellationToken)
    {
        if (!await _transitionLock.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken))
        {
            _logger?.LogWarning("生命周期转换锁等待超时");
            throw new TimeoutException("生命周期转换锁等待超时");
        }

        try
        {
            var previousStage = _currentStage;

            // 验证转换是否允许
            if (!CanTransition(previousStage, transitionStage))
            {
                _logger?.LogWarning("不允许的生命周期转换: {Previous} -> {Transition}", 
                    previousStage, transitionStage);
                return;
            }

            // 设置转换阶段
            SetStage(transitionStage);
            _logger?.LogInformation("生命周期转换: {Previous} -> {Transition}", 
                previousStage, transitionStage);

            // 执行钩子
            var hookErrors = new List<Exception>();
            foreach (var hook in hooks)
            {
                try
                {
                    _logger?.LogDebug("执行生命周期钩子");
                    await hook(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    _logger?.LogWarning("生命周期钩子被取消");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "生命周期钩子执行失败");
                    hookErrors.Add(ex);
                }
            }

            // 如果有错误，转换为错误状态
            if (hookErrors.Count > 0)
            {
                SetStage(LifecycleStage.Error);
                ApplicationError?.Invoke(new AggregateException(hookErrors));
                throw new AggregateException("生命周期钩子执行失败", hookErrors);
            }

            // 设置目标阶段
            SetStage(targetStage);
            _logger?.LogInformation("生命周期转换完成: {Transition} -> {Target}", 
                transitionStage, targetStage);
        }
        finally
        {
            _transitionLock.Release();
        }
    }

    /// <summary>
    /// 判断是否允许生命周期转换
    /// </summary>
    private static bool CanTransition(LifecycleStage current, LifecycleStage target)
    {
        return current switch
        {
            LifecycleStage.Uninitialized => target == LifecycleStage.Initializing,
            LifecycleStage.Initialized => target == LifecycleStage.Starting || 
                                          target == LifecycleStage.Stopping,
            LifecycleStage.Running => target == LifecycleStage.Stopping,
            LifecycleStage.Stopped => target == LifecycleStage.Starting || 
                                      target == LifecycleStage.Initializing,
            LifecycleStage.Error => target == LifecycleStage.Initializing || 
                                    target == LifecycleStage.Stopping,
            _ => false
        };
    }

    /// <summary>
    /// 设置当前阶段并触发事件
    /// </summary>
    private void SetStage(LifecycleStage newStage)
    {
        var previousStage = _currentStage;
        _currentStage = newStage;
        StageChanged?.Invoke(previousStage, newStage);
    }

    /// <summary>
    /// 获取生命周期状态摘要
    /// </summary>
    public string GetStatusSummary()
    {
        return $"当前阶段: {_currentStage}, " +
               $"初始化钩子: {_initHooks.Count}, " +
               $"启动钩子: {_startHooks.Count}, " +
               $"停止钩子: {_stopHooks.Count}, " +
               $"关闭钩子: {_shutdownHooks.Count}";
    }
}