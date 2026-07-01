using System.Collections.Concurrent;

namespace PCL_CE.Neo.Core.Utils.Logging;

public class CompositeLogger : ILogger, IDisposable
{
    private readonly ConcurrentBag<ILogger> _loggers = new ConcurrentBag<ILogger>();

    public CompositeLogger(params ILogger[] loggers)
    {
        foreach (var logger in loggers)
            Add(logger);
    }

    public void Add(ILogger logger)
    {
        if (logger != null)
            _loggers.Add(logger);
    }

    public void Remove(ILogger logger)
    {
        // ConcurrentBag doesn't support Remove, so we create a new bag without the logger
        var newBag = new ConcurrentBag<ILogger>();
        while (_loggers.TryTake(out var existing))
        {
            if (!ReferenceEquals(existing, logger))
                newBag.Add(existing);
        }
        _loggers.Clear();
        while (newBag.TryTake(out var item))
            _loggers.Add(item);
    }

    public void Clear()
    {
        _loggers.Clear();
    }

    public void Trace(string message) => LogToAll(l => l.Trace(message));
    public void Trace(string message, Exception exception) => LogToAll(l => l.Trace(message, exception));
    public void Debug(string message) => LogToAll(l => l.Debug(message));
    public void Debug(string message, Exception exception) => LogToAll(l => l.Debug(message, exception));
    public void Info(string message) => LogToAll(l => l.Info(message));
    public void Info(string message, Exception exception) => LogToAll(l => l.Info(message, exception));
    public void Warning(string message) => LogToAll(l => l.Warning(message));
    public void Warning(string message, Exception exception) => LogToAll(l => l.Warning(message, exception));
    public void Error(string message) => LogToAll(l => l.Error(message));
    public void Error(string message, Exception exception) => LogToAll(l => l.Error(message, exception));
    public void Critical(string message) => LogToAll(l => l.Critical(message));
    public void Critical(string message, Exception exception) => LogToAll(l => l.Critical(message, exception));

    public bool IsEnabled(LogLevel level)
    {
        return _loggers.Any(l => l.IsEnabled(level));
    }

    private void LogToAll(Action<ILogger> action)
    {
        foreach (var logger in _loggers)
        {
            try
            {
                action(logger);
            }
            catch
            {
            }
        }
    }

    public void Dispose()
    {
        foreach (var logger in _loggers)
        {
            if (logger is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                }
                catch
                {
                }
            }
        }
        _loggers.Clear();
    }
}