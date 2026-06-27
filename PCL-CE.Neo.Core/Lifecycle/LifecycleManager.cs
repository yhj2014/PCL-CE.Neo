using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.Lifecycle;

/// <summary>
/// 启动器生命周期管理器
/// </summary>
public sealed class LifecycleManager : ILifecycleService, IDisposable
{
    private readonly ILogger<LifecycleManager> _logger;
    
    public string Identifier => "lifecycle";
    public string Name => "生命周期管理";
    public bool SupportAsync => false;

    private static LifecycleManager? _instance;
    private static LifecycleContext? _context;

    private readonly ConcurrentDictionary<string, LifecycleServiceInfo> _runningServices = new();
    private readonly ConcurrentStack<ILifecycleService> _startedServiceStack = new();
    private readonly Dictionary<string, ILifecycleService> _manualServices = new();
    private readonly HashSet<ILifecycleService> _declaredStoppedServices = new();
    private readonly ConcurrentDictionary<string, Exception> _serviceExceptions = new();
    private readonly ConcurrentBag<Task> _stoppingTasks = new();
    private readonly List<LifecycleLogItem> _pendingLogs = new();
    private readonly object _pendingLogLock = new();
    
    private ILifecycleLogService? _logService;
    private bool _hasRequestedRestart = false;
    private string? _requestRestartArguments;
    private ILifecycleService? _requestRestartService;
    private bool _hasRequestedStopLoading = false;
    private bool _isApplicationStarted = false;
    private bool _isLoadingStarted = false;
    private bool _isWindowCreated = false;
    private DateTime? _runningStartTime;

    public static LifecycleManager Instance => _instance ?? throw new InvalidOperationException("LifecycleManager not initialized");
    private static LifecycleContext Context => _context ?? SystemContext;
    
    private static readonly SystemLifecycleService _systemService = new();
    private static readonly LifecycleServiceInfo _systemServiceInfo = new(_systemService, LifecycleState.BeforeLoading);
    internal static readonly LifecycleContext SystemContext = CreateContext(_systemService);

    public LifecycleManager(ILogger<LifecycleManager> logger)
    {
        _logger = logger;
        _instance = this;
        _context = CreateContext(this);
        _runningServices["system"] = _systemServiceInfo;
    }

    public LifecycleState CurrentState { get; private set; } = LifecycleState.BeforeLoading;

    /// <summary>
    /// 生命周期状态改变时触发的事件
    /// </summary>
    public event Action<LifecycleState>? StateChanged;

    /// <summary>
    /// 服务项启动前触发的事件
    /// </summary>
    public event Action<string>? ServiceStarting;

    /// <summary>
    /// 服务项启动后触发的事件
    /// </summary>
    public event Action<string>? ServiceStarted;

    /// <summary>
    /// 服务项停止前触发的事件
    /// </summary>
    public event Action<string>? ServiceStopping;

    /// <summary>
    /// 服务项停止后触发的事件
    /// </summary>
    public event Action<string>? ServiceStopped;

    /// <summary>
    /// 所有正在运行的服务项标识符
    /// </summary>
    public ICollection<string> RunningServices => _runningServices.Keys;

    /// <summary>
    /// 是否正在关闭程序
    /// </summary>
    public bool HasShutdownStarted { get; private set; } = false;

    /// <summary>
    /// 正在进行的关闭程序流程是否是强制关闭
    /// </summary>
    public bool IsForceShutdown { get; private set; } = false;

    public Task StartAsync() => Task.CompletedTask;

    public Task StopAsync()
    {
        _context = null;
        return Task.CompletedTask;
    }

    /// <summary>
    /// 创建指定服务项的上下文实例
    /// </summary>
    public static LifecycleContext CreateContext(ILifecycleService service)
    {
        return new LifecycleContext(
            service,
            item =>
            {
                lock (_instance!._pendingLogLock)
                {
                    if (_instance._logService == null)
                        _instance._pendingLogs.Add(item);
                    else
                        _instance._logService.OnLog(item);
                }
                if (item.ActionLevel == ActionLevel.FatalExit)
                    _instance!._FatalExit();
            },
            statusCode =>
            {
                if (_instance!.CurrentState != LifecycleState.BeforeLoading)
                    throw new InvalidOperationException("只能在 BeforeLoading 时请求退出");
                Context.Info($"{service.Name} 已请求退出程序");
                _instance._Exit(statusCode);
            },
            args =>
            {
                _instance!._hasRequestedRestart = true;
                _instance._requestRestartService = service;
                _instance._requestRestartArguments = args;
            },
            () =>
            {
                var identifier = service.Identifier;
                if (_instance!.GetServiceInfo(identifier)?.Identifier == identifier)
                    throw new InvalidOperationException("只能在服务启动阶段调用");
                _instance._declaredStoppedServices.Add(service);
            },
            () =>
            {
                if (_instance!.CurrentState != LifecycleState.BeforeLoading)
                    throw new InvalidOperationException("只能在 BeforeLoading 时请求停止加载");
                Context.Info($"{service.Name} 已请求停止继续加载");
                _instance._hasRequestedStopLoading = true;
            }
        );
    }

    /// <summary>
    /// 检查指定标识符的服务项是否正在运行
    /// </summary>
    public bool IsServiceRunning(string identifier) => _runningServices.ContainsKey(identifier);

    /// <summary>
    /// 根据标识符获取正在运行的服务项的相关信息
    /// </summary>
    public LifecycleServiceInfo? GetServiceInfo(string? identifier)
    {
        if (identifier == null) return null;
        _runningServices.TryGetValue(identifier, out var info);
        return info;
    }

    /// <summary>
    /// 获取服务项初始化或结束逻辑的最后一次异常
    /// </summary>
    public Exception? GetServiceLastException(string identifier)
    {
        return _serviceExceptions.TryGetValue(identifier, out var ex) ? ex : null;
    }

    /// <summary>
    /// 注册生命周期服务
    /// </summary>
    public void RegisterService(ILifecycleService service, LifecycleState state = LifecycleState.Manual)
    {
        if (IsServiceRunning(service.Identifier))
        {
            Context.Warn($"{service.Name} ({service.Identifier}) 标识符重复，已跳过");
            return;
        }
        
        lock (_manualServices)
        {
            _manualServices[service.Identifier] = service;
        }
        
        _logger.LogInformation("已注册服务: {Name} ({Identifier})", service.Name, service.Identifier);
    }

    /// <summary>
    /// 手动请求启动一个周期为 Manual 的服务项
    /// </summary>
    public bool StartService(string identifier, bool? async = null)
    {
        lock (_manualServices)
        {
            if (!_manualServices.TryGetValue(identifier, out var service))
                return false;
        }
        
        if (IsServiceRunning(identifier)) return false;
        
        var serviceInfo = GetServiceInfo(identifier);
        if (serviceInfo == null) return false;
        
        async ??= serviceInfo.CanStartAsync;
        
        if (async == true)
            Task.Run(() => _StartServiceTask(serviceInfo, true));
        else
            _StartServiceTask(serviceInfo, true);
        
        return true;
    }

    /// <summary>
    /// 手动请求停止一个周期为 Manual 的服务项
    /// </summary>
    public bool StopService(string identifier, bool async = true)
    {
        lock (_manualServices)
        {
            if (!_manualServices.TryGetValue(identifier, out var service))
                return false;
        }
        
        if (!IsServiceRunning(identifier)) return false;
        
        var serviceInfo = GetServiceInfo(identifier);
        if (serviceInfo == null) return false;
        
        _StopService(serviceInfo, async, true);
        return true;
    }

    /// <summary>
    /// 阻塞当前线程并等待到达指定生命周期状态
    /// </summary>
    public bool WaitForState(LifecycleState state)
    {
        if (CurrentState >= state) return false;
        
        using var mre = new ManualResetEventSlim(false);
        StateChanged += handler;
        try { mre.Wait(); }
        finally { StateChanged -= handler; }
        return true;

        void handler(LifecycleState s)
        {
            if (s == state) mre.Set();
        }
    }

    /// <summary>
    /// 异步等待到达指定生命周期状态
    /// </summary>
    public async Task<bool> WaitForStateAsync(LifecycleState state)
    {
        if (CurrentState >= state) return false;
        
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        StateChanged += handler;
        return await tcs.Task;

        void handler(LifecycleState s)
        {
            if (s != state) return;
            StateChanged -= handler;
            tcs.TrySetResult(true);
        }
    }

    /// <summary>
    /// 快速注册改变到目标生命周期状态的事件
    /// </summary>
    public void When(LifecycleState targetState, Action action)
    {
        if (CurrentState >= targetState) return;
        StateChanged += handler;
        return;

        void handler(LifecycleState state)
        {
            if (state != targetState) return;
            action();
            StateChanged -= handler;
        }
    }

    private void _NextState(LifecycleState? enforce = null)
    {
        if (enforce is { } state)
            CurrentState = state;
        else
            CurrentState++;
        
        _logger.LogDebug("状态改变: {State}", CurrentState);
        try { StateChanged?.Invoke(CurrentState); }
        catch (Exception ex) { Context.Warn("状态更改事件出错", ex); }
    }

    private string _ServiceName(ILifecycleService service, LifecycleState? state = null)
    {
        var info = GetServiceInfo(service.Identifier);
        if (info != null) state = info.StartState;
        var stateText = (state == null) ? "" : $"{state}/";
        return $"{service.Name} ({stateText}{service.Identifier})";
    }

    private Task _StartServiceTask(ILifecycleService service, LifecycleState state, bool manual = false)
    {
        ILifecycleLogService? logService = null;
        
        if (service is ILifecycleLogService ls)
        {
            if (_logService != null)
                throw new InvalidOperationException("日志服务只能有一个");
            logService = ls;
        }
        
        var serviceName = _ServiceName(service, state);
        
        lock (_manualServices)
        {
            if (_manualServices.ContainsKey(service.Identifier) && IsServiceRunning(service.Identifier))
            {
                Context.Warn($"{serviceName} 标识符重复，已跳过");
                return Task.CompletedTask;
            }
            _runningServices[service.Identifier] = _systemServiceInfo;
        }
        
        return service.SupportAsync ? Task.Run(AsyncCall) : AsyncCall();

        async Task AsyncCall()
        {
            try
            {
                Context.Trace($"正在启动 {serviceName}");
                ServiceStarting?.Invoke(service.Identifier);
                await service.StartAsync().ConfigureAwait(!service.SupportAsync);
                
                var serviceInfo = new LifecycleServiceInfo(service, state);
                Context.Debug($"{serviceName} 启动成功");
                
                if (_declaredStoppedServices.Contains(service))
                {
                    _declaredStoppedServices.Remove(service);
                    _logger.LogInformation("{Service} 已中止", serviceName);
                }
                else
                {
                    _startedServiceStack.Push(service);
                    _runningServices[service.Identifier] = serviceInfo;
                    ServiceStarted?.Invoke(service.Identifier);
                }
            }
            catch (Exception ex)
            {
                Context.Warn($"{serviceName} 启动失败，尝试停止", ex);
                _serviceExceptions[service.Identifier] = ex;
                _StopService(service, false, manual);
            }
            
            if (logService != null)
            {
                lock (_pendingLogLock)
                {
                    foreach (var item in _pendingLogs)
                        logService.OnLog(item);
                    _pendingLogs.Clear();
                    _logService = logService;
                }
            }
        }
    }

    private void _StopService(ILifecycleService service, bool async, bool manual = false)
    {
        var serviceName = _ServiceName(service, manual ? LifecycleState.Manual : CurrentState);
        
        if (async)
            _stoppingTasks.Add(Task.Run(Stop));
        else
            Stop().ConfigureAwait(false).GetAwaiter().GetResult();
        
        return;

        async Task Stop()
        {
            try
            {
                Context.Trace($"正在停止 {serviceName}");
                ServiceStopping?.Invoke(service.Identifier);
                await service.StopAsync().ConfigureAwait(!async);
                ServiceStopped?.Invoke(service.Identifier);
                Context.Debug($"{serviceName} 已停止");
            }
            catch (Exception ex)
            {
                Context.Warn($"停止 {serviceName} 时出错，已跳过", ex);
                _serviceExceptions[service.Identifier] = ex;
            }
            
            _runningServices.TryRemove(service.Identifier, out var removed);
            removed?.MarkAsStopped();
        }
    }

    private void _StartStateFlow(LifecycleState start, LifecycleState? end = null)
    {
        var index = (int)start;
        var endIndex = end == null ? index : (int)end;
        
        while (index <= endIndex)
        {
            var state = (LifecycleState)index;
            _NextState(state);
            
            var countStart = DateTime.Now;
            
            lock (_manualServices)
            {
                var services = _manualServices.Values
                    .Where(s => GetServiceInfo(s.Identifier)?.StartState == state)
                    .ToList();
                
                var asyncServices = new List<ILifecycleService>();
                
                foreach (var service in services)
                {
                    if (service.SupportAsync)
                        asyncServices.Add(service);
                    else
                        _StartServiceTask(service, state).ConfigureAwait(false).GetAwaiter().GetResult();
                    
                    if (_hasRequestedStopLoading) return;
                }
                
                if (asyncServices.Count > 0)
                    Task.WaitAll(asyncServices.Select(s => _StartServiceTask(s, state)).ToArray());
            }
            
            var countSpan = DateTime.Now - countStart;
            _logger.LogDebug("状态 {State} 共用时 {Ms} ms", state, countSpan.TotalMilliseconds);
            
            index++;
        }
    }

    private void _FatalExit()
    {
        ForceShutdown(-1);
    }

    private void _Exit(int statusCode = 0)
    {
        if (HasShutdownStarted) return;
        HasShutdownStarted = true;
        
        if (_runningStartTime is { } start)
        {
            var countSpan = DateTime.Now - start;
            _logger.LogDebug("Running 状态共用时 {Ms} ms", countSpan.TotalMilliseconds);
        }
        
        _StartStateFlow(LifecycleState.Exiting);
        
        Context.Debug("正在停止运行中的服务");
        
        ILifecycleLogService? logService = null;
        
        while (_startedServiceStack.TryPop(out var service))
        {
            if (_runningServices.TryGetValue(service.Identifier, out var info) && info.IsStopped)
                continue;
            
            if (service is ILifecycleLogService ls)
            {
                Context.Trace($"已跳过日志服务: {_ServiceName(ls)}");
                logService = ls;
                continue;
            }
            
            _StopService(service, service.SupportAsync);
        }
        
        Task.WaitAll(_stoppingTasks.ToArray());
        
        if (logService != null)
        {
            Context.Trace("退出过程已结束，正在停止日志服务");
            logService.StopAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        
        _SavePendingLogs();
        
        if (_hasRequestedRestart && _requestRestartService != null)
        {
            _logger.LogInformation("请求重启程序，来源: {Identifier}", _requestRestartService.Identifier);
        }
        
        _logger.LogInformation("程序退出，状态码: {StatusCode}", statusCode);
        
        if (statusCode == -1)
            Environment.Exit(-1);
        else
            Environment.Exit(statusCode);
    }

    /// <summary>
    /// 发起关闭程序流程
    /// </summary>
    public void Shutdown(int statusCode = 0, bool force = false)
    {
        if (CurrentState == LifecycleState.BeforeLoading)
            throw new InvalidOperationException("Cannot shutdown in BeforeLoading state");
        
        if (HasShutdownStarted) return;
        
        Context.Info(force ? "开始强制关闭程序" : "正在关闭程序");
        IsForceShutdown = force;
        
        if (force)
            _Exit(statusCode);
        else
            new Thread(() => _Exit(statusCode)) { Name = "Lifecycle/Shutdown" }.Start();
    }

    /// <summary>
    /// 强制关闭程序
    /// </summary>
    public void ForceShutdown(int statusCode = 0) => Shutdown(statusCode, true);

    private void _SavePendingLogs()
    {
        if (_pendingLogs.Count == 0) return;
        
        try
        {
            foreach (var item in _pendingLogs)
            {
                _logger.LogDebug("Pending log: {Message}", item.ComposeMessage());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存待处理日志时出错");
        }
    }

    public void Dispose()
    {
        if (!HasShutdownStarted)
            Shutdown(0);
    }

    private class SystemLifecycleService : ILifecycleService
    {
        public string Name => "系统";
        public string Identifier => "system";
        public bool SupportAsync => false;
        public Task StartAsync() => Task.CompletedTask;
        public Task StopAsync() => Task.CompletedTask;
    }
}