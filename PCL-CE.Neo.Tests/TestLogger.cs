using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PCL_CE.Neo.Tests;

public class TestLogger<T> : ILogger<T>
{
    private readonly List<string> _logs = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullLogger.Instance.BeginScope(state);
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _logs.Add($"[{logLevel}] {formatter(state, exception)}");
    }

    public IReadOnlyList<string> Logs => _logs;
}
