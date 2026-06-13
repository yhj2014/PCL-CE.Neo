using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class TelemetryAdapter : ITelemetryAdapter
{
    private readonly ILogger<TelemetryAdapter> _logger;
    private readonly IConfigAdapter _config;
    private readonly INetworkAdapter _network;
    private bool _isEnabled;
    private string? _userId;
    private string _telemetryEndpoint = "https://telemetry.pcl.example.com/api/v1/track";
    private readonly ConcurrentQueue<TelemetryEvent> _eventQueue = new();
    private readonly ConcurrentQueue<TelemetryError> _errorQueue = new();
    private readonly ConcurrentQueue<TelemetryMetric> _metricQueue = new();
    private Timer? _flushTimer;

    public bool IsEnabled => _isEnabled;

    public TelemetryAdapter() : this(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<TelemetryAdapter>.Instance,
        new ConfigAdapter(),
        new NetworkAdapter())
    {
    }

    public TelemetryAdapter(
        ILogger<TelemetryAdapter> logger,
        IConfigAdapter config,
        INetworkAdapter network)
    {
        _logger = logger;
        _config = config;
        _network = network;
    }

    public void TrackException(Exception ex, string? message = null)
    {
        _errorQueue.Enqueue(new TelemetryError { Name = ex.GetType().Name, Message = message ?? ex.Message, Timestamp = DateTime.UtcNow });
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

    private async Task SendToServerAsync(
        List<TelemetryEvent> events,
        List<TelemetryError> errors,
        List<TelemetryMetric> metrics)
    {
        try
        {
            var payload = new TelemetryPayload
            {
                UserId = _userId,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Events = events.Select(e => new TelemetryPayloadEvent
                {
                    Name = e.Name,
                    Properties = e.Properties,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }).ToList(),
                Errors = errors.Select(err => new TelemetryPayloadError
                {
                    Message = err.Message ?? err.Exception?.Message ?? "Unknown",
                    StackTrace = err.Exception?.StackTrace,
                    Properties = err.Properties,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }).ToList(),
                Metrics = metrics.Select(m => new TelemetryPayloadMetric
                {
                    Name = m.Name,
                    Value = m.Value,
                    Tags = m.Tags,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }).ToList()
            };

            var json = JsonSerializer.Serialize(payload);
            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "X-Telemetry-Version", "1.0" }
            };

            var response = await _network.PostAsync(_telemetryEndpoint, json, headers);

            if (response.IsSuccess)
            {
                _logger.LogDebug("遥测数据发送成功");
            }
            else
            {
                foreach (var evt in events) _eventQueue.Enqueue(evt);
                foreach (var err in errors) _errorQueue.Enqueue(err);
                foreach (var m in metrics) _metricQueue.Enqueue(m);
                _logger.LogWarning("遥测数据发送失败，状态码: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            foreach (var evt in events) _eventQueue.Enqueue(evt);
            foreach (var err in errors) _errorQueue.Enqueue(err);
            foreach (var m in metrics) _metricQueue.Enqueue(m);
            _logger.LogWarning(ex, "遥测数据发送异常");
        }
    }

    private class TelemetryPayload
    {
        public string? UserId { get; set; }
        public long Timestamp { get; set; }
        public List<TelemetryPayloadEvent> Events { get; set; } = new();
        public List<TelemetryPayloadError> Errors { get; set; } = new();
        public List<TelemetryPayloadMetric> Metrics { get; set; } = new();
    }

    private class TelemetryPayloadEvent
    {
        public string Name { get; set; } = "";
        public Dictionary<string, object?> Properties { get; set; } = new();
        public long Timestamp { get; set; }
    }

    private class TelemetryPayloadError
    {
        public string Message { get; set; } = "";
        public string? StackTrace { get; set; }
        public Dictionary<string, object?> Properties { get; set; } = new();
        public long Timestamp { get; set; }
    }

    private class TelemetryPayloadMetric
    {
        public string Name { get; set; } = "";
        public double Value { get; set; }
        public Dictionary<string, string> Tags { get; set; } = new();
        public long Timestamp { get; set; }
    }
}

public class TelemetryEvent
{
    public required string Name { get; init; }
    public Dictionary<string, object?> Properties { get; init; } = new();
}

public class TelemetryError
{
    public string Name { get; init; } = "";
    public DateTime Timestamp { get; init; }
    public Exception? Exception { get; init; }
    public string? Message { get; init; }
    public Dictionary<string, object?> Properties { get; init; } = new();
}

public class TelemetryMetric
{
    public required string Name { get; init; }
    public required double Value { get; init; }
    public Dictionary<string, string> Tags { get; init; } = new();
}
