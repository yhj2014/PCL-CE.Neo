using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class LoggerAdapter : ILoggerAdapter
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private readonly ILogWriter? _logWriter;
    private readonly ConcurrentDictionary<string, Scope> _scopes = new();
    private LogLevel _minLevel = LogLevel.Debug;

    public LoggerAdapter(ILoggerFactory loggerFactory, ILogWriter? logWriter = null)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger("PCL");
        _logWriter = logWriter;
    }

    public void Trace(string message, params object[] args)
    {
        Log(LogLevel.Trace, null, message, args);
    }

    public void Debug(string message, params object[] args)
    {
        Log(LogLevel.Debug, null, message, args);
    }

    public void Information(string message, params object[] args)
    {
        Log(LogLevel.Information, null, message, args);
    }

    public void Warning(string message, params object[] args)
    {
        Log(LogLevel.Warning, null, message, args);
    }

    public void Warning(Exception? ex, string message, params object[] args)
    {
        Log(LogLevel.Warning, ex, message, args);
    }

    public void Error(string message, params object[] args)
    {
        Log(LogLevel.Error, null, message, args);
    }

    public void Error(Exception? ex, string message, params object[] args)
    {
        Log(LogLevel.Error, ex, message, args);
    }

    public void Fatal(string message, params object[] args)
    {
        Log(LogLevel.Critical, null, message, args);
    }

    public void Fatal(Exception? ex, string message, params object[] args)
    {
        Log(LogLevel.Critical, ex, message, args);
    }

    public IDisposable? BeginScope(string scope)
    {
        var id = Guid.NewGuid().ToString();
        var s = new Scope(id, scope, this);
        _scopes[id] = s;
        return s;
    }

    public bool IsEnabled(LogLevel level) => level >= _minLevel;

    public void SetLevel(LogLevel level)
    {
        _minLevel = level;
    }

    private void Log(LogLevel level, Exception? ex, string message, params object[] args)
    {
        if (!IsEnabled(level)) return;

        var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;

        var entry = new LogEntry
        {
            Level = level.ToString(),
            Message = formattedMessage,
            Exception = ex?.ToString(),
            StackTrace = ex?.StackTrace
        };

        Task.Run(async () =>
        {
            if (_logWriter != null)
            {
                await _logWriter.WriteLogAsync(entry);
            }
        });

        var scopes = _scopes.Values.Select(s => s.Name).ToList();
        var scopeText = scopes.Count > 0 ? $"[{string.Join("][", scopes)}] " : "";

        switch (level)
        {
            case LogLevel.Trace:
            case LogLevel.Debug:
                _logger.LogDebug(ex, scopeText + formattedMessage);
                break;
            case LogLevel.Information:
                _logger.LogInformation(ex, scopeText + formattedMessage);
                break;
            case LogLevel.Warning:
                _logger.LogWarning(ex, scopeText + formattedMessage);
                break;
            case LogLevel.Error:
                _logger.LogError(ex, scopeText + formattedMessage);
                break;
            case LogLevel.Critical:
                _logger.LogCritical(ex, scopeText + formattedMessage);
                break;
        }
    }

    internal void RemoveScope(string id)
    {
        _scopes.TryRemove(id, out _);
    }

    private class Scope : IDisposable
    {
        private readonly LoggerAdapter _parent;
        public string Id { get; }
        public string Name { get; }

        public Scope(string id, string name, LoggerAdapter parent)
        {
            Id = id;
            Name = name;
            _parent = parent;
        }

        public void Dispose()
        {
            _parent.RemoveScope(Id);
        }
    }
}

public class FileLogWriter : ILogWriter
{
    private readonly string _logFilePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly StreamWriter _writer;

    public FileLogWriter(string logDirectory)
    {
        Directory.CreateDirectory(logDirectory);
        var fileName = $"pcl_{DateTime.Now:yyyyMMdd_HHmmss}.log";
        _logFilePath = Path.Combine(logDirectory, fileName);
        _writer = new StreamWriter(_logFilePath, append: true) { AutoFlush = false };
    }

    public async Task WriteLogAsync(LogEntry entry)
    {
        await _lock.WaitAsync();
        try
        {
            var line = $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{entry.Level}] {entry.Message}";
            if (!string.IsNullOrEmpty(entry.Source))
            {
                line = $"[{entry.Source}] {line}";
            }
            if (!string.IsNullOrEmpty(entry.Exception))
            {
                line += $"\n  Exception: {entry.Exception}";
            }
            await _writer.WriteLineAsync(line);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task FlushAsync()
    {
        await _lock.WaitAsync();
        try
        {
            await _writer.FlushAsync();
        }
        finally
        {
            _lock.Release();
        }
    }
}
