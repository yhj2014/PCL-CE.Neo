using System.Collections.Concurrent;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.Abstractions;

public class LoggerAdapter : ILoggerAdapter
{
    private readonly string _categoryName;
    private readonly ConcurrentDictionary<string, object> _scopes = new ConcurrentDictionary<string, object>();
    private Abstractions.LogLevel _minLevel = Abstractions.LogLevel.Information;

    public LoggerAdapter(string categoryName)
    {
        _categoryName = categoryName;
    }

    private static Logging.LogLevel ToCoreLogLevel(Abstractions.LogLevel level)
    {
        return level switch
        {
            Abstractions.LogLevel.Trace => Logging.LogLevel.Trace,
            Abstractions.LogLevel.Debug => Logging.LogLevel.Debug,
            Abstractions.LogLevel.Information => Logging.LogLevel.Info,
            Abstractions.LogLevel.Warning => Logging.LogLevel.Warning,
            Abstractions.LogLevel.Error => Logging.LogLevel.Error,
            Abstractions.LogLevel.Critical => Logging.LogLevel.Fatal,
            _ => Logging.LogLevel.Info
        };
    }

    public void Trace(string message, params object[] args)
    {
        if (!IsEnabled(Abstractions.LogLevel.Trace)) return;
        var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
        LogWrapper.Trace(formattedMessage, _categoryName);
    }

    public void Debug(string message, params object[] args)
    {
        if (!IsEnabled(Abstractions.LogLevel.Debug)) return;
        var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
        LogWrapper.Debug(formattedMessage, _categoryName);
    }

    public void Information(string message, params object[] args)
    {
        if (!IsEnabled(Abstractions.LogLevel.Information)) return;
        var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
        LogWrapper.Info(formattedMessage, _categoryName);
    }

    public void Warning(string message, params object[] args)
    {
        if (!IsEnabled(Abstractions.LogLevel.Warning)) return;
        var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
        LogWrapper.Warn(formattedMessage, _categoryName);
    }

    public void Warning(Exception? ex, string message, params object[] args)
    {
        if (!IsEnabled(Abstractions.LogLevel.Warning)) return;
        var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
        LogWrapper.Error(ex, _categoryName, formattedMessage);
    }

    public void Error(string message, params object[] args)
    {
        if (!IsEnabled(Abstractions.LogLevel.Error)) return;
        var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
        LogWrapper.Error(formattedMessage, _categoryName);
    }

    public void Error(Exception? ex, string message, params object[] args)
    {
        if (!IsEnabled(Abstractions.LogLevel.Error)) return;
        var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
        LogWrapper.Error(ex, _categoryName, formattedMessage);
    }

    public void Fatal(string message, params object[] args)
    {
        if (!IsEnabled(Abstractions.LogLevel.Critical)) return;
        var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
        LogWrapper.Fatal(formattedMessage, _categoryName);
    }

    public void Fatal(Exception? ex, string message, params object[] args)
    {
        if (!IsEnabled(Abstractions.LogLevel.Critical)) return;
        var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
        LogWrapper.Fatal(ex, _categoryName, formattedMessage);
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
