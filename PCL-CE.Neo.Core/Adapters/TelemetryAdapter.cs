using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class TelemetryAdapter : ITelemetryAdapter
{
    private readonly ILogger<TelemetryAdapter> _logger;
    private readonly IConfigAdapter _config;
    private bool _isEnabled;
    private string? _userId;
    private readonly ConcurrentQueue<TelemetryEvent> _eventQueue = new();
    private readonly ConcurrentQueue<TelemetryError> _errorQueue = new();
    private readonly ConcurrentQueue<TelemetryMetric> _metricQueue = new();
    private Timer? _flushTimer;

    public bool IsEnabled => _isEnabled;

    public TelemetryAdapter(ILogger<TelemetryAdapter> logger, IConfigAdapter config)
    {
        _logger = logger;
        _config = config;
    }

    public void Initialize()
    {
        _isEnabled = _config.GetConfig("SystemTelemetry", false);
        if (_isEnabled)
        {
            _flushTimer = new Timer(async _ => await FlushAsync(), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            _logger.LogInformation("遥测系统已初始化");
        }
        else
        {
            _logger.LogInformation("遥测系统已禁用");
        }
    }

    public void SetUserId(string userId)
    {
        _userId = userId;
    }

    public void TrackEvent(string eventName, Dictionary<string, object?>? properties = null)
    {
        if (!_isEnabled) return;

        var telemetryEvent = new TelemetryEvent
        {
            Name = eventName,
            Properties = properties ?? new Dictionary<string, object?>()
        };

        _eventQueue.Enqueue(telemetryEvent);
        _logger.LogDebug("遥测事件: {Event}", eventName);
    }

    public void TrackPageView(string pageName, Dictionary<string, object?>? properties = null)
    {
        if (!_isEnabled) return;

        var data = new Dictionary<string, object?>(properties ?? new Dictionary<string, object?>())
        {
            ["PageName"] = pageName
        };

        TrackEvent("PageView", data);
    }

    public void TrackError(Exception exception, string? message = null, Dictionary<string, object?>? properties = null)
    {
        if (!_isEnabled) return;

        var error = new TelemetryError
        {
            Exception = exception,
            Message = message,
            Properties = properties ?? new Dictionary<string, object?>()
        };

        _errorQueue.Enqueue(error);
        _logger.LogError(exception, "遥测错误: {Message}", message ?? exception.Message);
    }

    public void TrackMetric(string name, double value, Dictionary<string, string>? tags = null)
    {
        if (!_isEnabled) return;

        var metric = new TelemetryMetric
        {
            Name = name,
            Value = value,
            Tags = tags ?? new Dictionary<string, string>()
        };

        _metricQueue.Enqueue(metric);
        _logger.LogDebug("遥测指标: {Name} = {Value}", name, value);
    }

    public async Task FlushAsync()
    {
        if (!_isEnabled) return;

        var events = new List<TelemetryEvent>();
        while (_eventQueue.TryDequeue(out var evt))
        {
            events.Add(evt);
        }

        var errors = new List<TelemetryError>();
        while (_errorQueue.TryDequeue(out var err))
        {
            errors.Add(err);
        }

        var metrics = new List<TelemetryMetric>();
        while (_metricQueue.TryDequeue(out var metric))
        {
            metrics.Add(metric);
        }

        if (events.Count > 0 || errors.Count > 0 || metrics.Count > 0)
        {
            _logger.LogDebug("刷新遥测数据: {Events} 事件, {Errors} 错误, {Metrics} 指标",
                events.Count, errors.Count, metrics.Count);

            await SendToServerAsync(events, errors, metrics);
        }
    }

    public void Shutdown()
    {
        _flushTimer?.Dispose();
        FlushAsync().GetAwaiter().GetResult();
        _logger.LogInformation("遥测系统已关闭");
    }

    private Task SendToServerAsync(
        List<TelemetryEvent> events,
        List<TelemetryError> errors,
        List<TelemetryMetric> metrics)
    {
        return Task.CompletedTask;
    }
}
