namespace PCL_CE.Neo.Core.Logging;

/// <summary>
/// Static log wrapper for easy logging.
/// </summary>
public static class LogWrapper
{
    /// <summary>
    /// Log event handler.
    /// </summary>
    public static event Action<LogLevel, string, string?, Exception?>? OnLog;

    private static void Log(LogLevel level, string message, string? module = null, Exception? ex = null)
    {
        OnLog?.Invoke(level, message, module, ex);
    }

    public static void Trace(string message, string? module = null) => Log(LogLevel.Trace, message, module);
    public static void Debug(string message, string? module = null) => Log(LogLevel.Debug, message, module);
    public static void Info(string message, string? module = null) => Log(LogLevel.Info, message, module);
    public static void Warn(string message, string? module = null) => Log(LogLevel.Warning, message, module);
    public static void Error(string message, string? module = null) => Log(LogLevel.Error, message, module);
    public static void Error(Exception ex, string? module = null, string? message = null) 
        => Log(LogLevel.Error, message ?? ex.Message, module, ex);
    public static void Fatal(string message, string? module = null) => Log(LogLevel.Fatal, message, module);
    public static void Fatal(Exception ex, string? module = null, string? message = null) 
        => Log(LogLevel.Fatal, message ?? ex.Message, module, ex);
}
