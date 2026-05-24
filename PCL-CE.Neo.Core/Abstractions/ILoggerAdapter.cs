namespace PCL_CE.Neo.Core.Abstractions;

public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5,
    None = 6
}

public interface ILoggerAdapter
{
    void Trace(string message, params object[] args);
    void Debug(string message, params object[] args);
    void Information(string message, params object[] args);
    void Warning(string message, params object[] args);
    void Warning(Exception? ex, string message, params object[] args);
    void Error(string message, params object[] args);
    void Error(Exception? ex, string message, params object[] args);
    void Fatal(string message, params object[] args);
    void Fatal(Exception? ex, string message, params object[] args);

    IDisposable? BeginScope(string scope);
    bool IsEnabled(LogLevel level);
    void SetLevel(LogLevel level);
}

public interface ILogWriter
{
    Task WriteLogAsync(LogEntry entry);
    Task FlushAsync();
}

public record LogEntry
{
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public required string Level { get; init; }
    public required string Message { get; init; }
    public string? Source { get; init; }
    public string? Exception { get; init; }
    public string? StackTrace { get; init; }
    public Dictionary<string, string> Properties { get; init; } = new();
}
