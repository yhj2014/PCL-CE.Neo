using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class LoggerAdapter : ILoggerAdapter
{
    private readonly string _categoryName;
    private readonly ILogService? _logService;
    private readonly ConcurrentDictionary<string, object> _scopes = new ConcurrentDictionary<string, object>();
    private Abstractions.LogLevel _minLevel = Abstractions.LogLevel.Information;

    public LoggerAdapter(string categoryName, ILogService? logService = null)
    {
        _categoryName = categoryName;
        _logService = logService;
    }

    public void Log(Abstractions.LogLevel level, string message, Exception? exception = null)
    {
        var logEntry = new LogEntry
        {
            Level = level,
            Category = _categoryName,
            Message = message,
            Exception = exception,
            Timestamp = DateTime.Now
        };

        _logService?.Log(logEntry);
    }

    public void Debug(string message)
    {
        Log(Abstractions.LogLevel.Debug, message);
    }

    public void Info(string message)
    {
        Log(Abstractions.LogLevel.Information, message);
    }

    public void Warning(string message)
    {
        Log(Abstractions.LogLevel.Warning, message);
    }

    public void Error(string message, Exception? exception = null)
    {
        Log(Abstractions.LogLevel.Error, message, exception);
    }

    public void Fatal(string message, Exception? exception = null)
    {
        Log(Abstractions.LogLevel.Critical, message, exception);
    }

    public IDisposable? BeginScope(string scope)
    {
        var scopeId = Guid.NewGuid().ToString();
        var scopeObject = new ScopeObject(scopeId, scope, _categoryName);
        _scopes[scopeId] = scopeObject;
        return new ScopeDisposer(scopeId, _scopes);
    }

    public bool IsEnabled(Abstractions.LogLevel level)
    {
        return level >= _minLevel;
    }

    public void SetLevel(Abstractions.LogLevel level)
    {
        _minLevel = level;
    }

    private class ScopeObject
    {
        public string ScopeId { get; }
        public string ScopeName { get; }
        public string Category { get; }
        public DateTime StartTime { get; }

        public ScopeObject(string scopeId, string scopeName, string category)
        {
            ScopeId = scopeId;
            ScopeName = scopeName;
            Category = category;
            StartTime = DateTime.Now;
        }
    }

    private class ScopeDisposer : IDisposable
    {
        private readonly string _scopeId;
        private readonly ConcurrentDictionary<string, object> _scopes;

        public ScopeDisposer(string scopeId, ConcurrentDictionary<string, object> scopes)
        {
            _scopeId = scopeId;
            _scopes = scopes;
        }

        public void Dispose()
        {
            _scopes.TryRemove(_scopeId, out _);
        }
    }
}
