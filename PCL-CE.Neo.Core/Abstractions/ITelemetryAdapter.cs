namespace PCL_CE.Neo.Core.Abstractions;

public interface ITelemetryAdapter
{
    bool IsEnabled { get; }

    void Initialize();
    void SetUserId(string userId);

    void TrackEvent(string eventName, Dictionary<string, object?>? properties = null);
    void TrackPageView(string pageName, Dictionary<string, object?>? properties = null);
    void TrackError(Exception exception, string? message = null, Dictionary<string, object?>? properties = null);
    void TrackMetric(string name, double value, Dictionary<string, string>? tags = null);

    Task FlushAsync();
    void Shutdown();
}

public class TelemetryEvent
{
    public required string Name { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public Dictionary<string, object?> Properties { get; init; } = new();
}

public class TelemetryError
{
    public required Exception Exception { get; init; }
    public string? Message { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public Dictionary<string, object?> Properties { get; init; } = new();
}

public class TelemetryMetric
{
    public required string Name { get; init; }
    public required double Value { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public Dictionary<string, string> Tags { get; init; } = new();
}
