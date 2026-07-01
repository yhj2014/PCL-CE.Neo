using System.Text;

namespace PCL_CE.Neo.Core.Utils.Logging;

public class ConsoleLogger : ILogger
{
    private readonly string _category;
    private readonly LogLevel _minLevel;
    private readonly object _lock = new object();

    public ConsoleLogger(string category = "", LogLevel minLevel = LogLevel.Debug)
    {
        _category = category;
        _minLevel = minLevel;
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
            Console.ForegroundColor = GetLevelColor(level);
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                Category = _category,
                Exception = exception
            };
            Console.WriteLine(entry.ToString());
            Console.ResetColor();
        }
    }

    private ConsoleColor GetLevelColor(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => ConsoleColor.Gray,
            LogLevel.Debug => ConsoleColor.Cyan,
            LogLevel.Info => ConsoleColor.White,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Critical => ConsoleColor.DarkRed,
            _ => ConsoleColor.White
        };
    }
}