using System;
using System.Collections.Concurrent;

namespace PCL_CE.Neo.Core.Logging;

public static class LogManager
{
    private static readonly ConcurrentDictionary<string, LogWrapper> _loggers = new();

    public static LogWrapper GetLogger(string name)
    {
        return _loggers.GetOrAdd(name, n => new LogWrapper(n));
    }

    public static LogWrapper GetLogger<T>()
    {
        return GetLogger(typeof(T).FullName ?? typeof(T).Name);
    }

    public static void AddListener(ILogListener listener)
    {
        LogWrapper.AddListener(listener);
    }

    public static void RemoveListener(ILogListener listener)
    {
        LogWrapper.RemoveListener(listener);
    }

    public static void ClearListeners()
    {
        LogWrapper.ClearListeners();
    }

    public static LogLevel GlobalLogLevel
    {
        get => LogWrapper.GlobalLogLevel;
        set => LogWrapper.GlobalLogLevel = value;
    }
}

public interface ILogListener
{
    void OnLog(LogLevel level, string loggerName, string message, Exception? exception = null);
}

public enum LogLevel
{
    Trace,
    Debug,
    Info,
    Warn,
    Error,
    Fatal
}