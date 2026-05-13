using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class LifecycleBridge : ILifecycleBridge
{
    private readonly ILogger<LifecycleBridge> _logger;
    private bool _isInitialized;
    private bool _isRunning;
    private bool _shutdownRequested;

    public event Action<string>? ServiceStarting;
    public event Action<string>? ServiceStarted;
    public event Action<string>? ServiceStopping;
    public event Action<string>? ServiceStopped;
    public event Action<string, Exception>? ServiceException;

    public LifecycleBridge(ILogger<LifecycleBridge> logger)
    {
        _logger = logger;
    }

    public void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        _logger.LogInformation("生命周期桥接已初始化");
    }

    public async Task StartAsync()
    {
        if (!_isInitialized)
            throw new InvalidOperationException("生命周期桥接尚未初始化");
        if (_isRunning)
            throw new InvalidOperationException("生命周期已经在运行中");

        _isRunning = true;
        _logger.LogInformation("生命周期开始执行");
        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (!_isRunning)
        {
            _logger.LogWarning("生命周期尚未运行");
            return;
        }

        _isRunning = false;
        _logger.LogInformation("生命周期停止执行");
        await Task.CompletedTask;
    }

    public void RequestShutdown()
    {
        _shutdownRequested = true;
        _logger.LogInformation("已请求关闭");
    }

    public void OnException(Exception ex)
    {
        _logger.LogError(ex, "生命周期异常");
        ServiceException?.Invoke("lifecycle", ex);
    }
}
