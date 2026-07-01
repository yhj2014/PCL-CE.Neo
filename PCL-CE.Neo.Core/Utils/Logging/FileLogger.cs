using System.IO;
using System.Text;
using PCL_CE.Neo.Core.Utils.FileSystem;

namespace PCL_CE.Neo.Core.Utils.Logging;

public class FileLogger : ILogger, IDisposable
{
    private readonly string _category;
    private readonly LogLevel _minLevel;
    private readonly string _logDirectory;
    private string _logFileName;
    private readonly object _lock = new object();
    private StreamWriter? _writer;
    private bool _disposed;

    public FileLogger(string category = "", LogLevel minLevel = LogLevel.Debug, string logDirectory = "")
    {
        _category = category;
        _minLevel = minLevel;
        _logDirectory = string.IsNullOrEmpty(logDirectory)
            ? Path.Combine(PathUtils.GetLocalApplicationDataPath(), "PCL-CE-NEO", "Logs")
            : logDirectory;

        _logFileName = $"PCL-CE-NEO_{DateTime.Now:yyyyMMdd}.log";
        FileUtils.EnsureDirectoryExists(_logDirectory);
        InitializeWriter();
    }

    private void InitializeWriter()
    {
        string logPath = Path.Combine(_logDirectory, _logFileName);
        _writer = new StreamWriter(logPath, true, Encoding.UTF8)
        {
            AutoFlush = true
        };
    }

    public void Trace(string message) => Log(LogLevel.Trace, message, null);
    public void Trace(string message, Exception exception) => Log(LogLevel.Trace, message, exception);
    public void Debug(string message) => Log(LogLevel.Debug, message, null);
    public void Debug(string message, Exception exception) => Log(LogLevel.Debug, message, exception);
    public void Info(string message) => Log(LogLevel.Info, message, null);
    public void Info(string message, Exception exception) => Log(LogLevel.Info, message, exception);
    public void Warning(string message) => Log(LogLevel.Warning, message, null);
    public void Warning(string message, Exception exception) => Log(LogLevel.Warning, message, exception);
    public void Error(string message) => Log(LogLevel.Error, message, null);
    public void Error(string message, Exception exception) => Log(LogLevel.Error, message, exception);
    public void Critical(string message) => Log(LogLevel.Critical, message, null);
    public void Critical(string message, Exception exception) => Log(LogLevel.Critical, message, exception);

    public bool IsEnabled(LogLevel level) => level >= _minLevel;

    private void Log(LogLevel level, string message, Exception? exception)
    {
        if (!IsEnabled(level))
            return;

        lock (_lock)
        {
            if (_disposed || _writer == null)
                return;

            string currentFileName = $"PCL-CE-NEO_{DateTime.Now:yyyyMMdd}.log";
            if (_logFileName != currentFileName)
            {
                _writer.Dispose();
                _logFileName = currentFileName;
                InitializeWriter();
            }

            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                Category = _category,
                Exception = exception
            };
            _writer.WriteLine(entry.ToString());
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            lock (_lock)
            {
                _writer?.Dispose();
                _writer = null;
            }
        }

        _disposed = true;
    }

    ~FileLogger()
    {
        Dispose(false);
    }
}