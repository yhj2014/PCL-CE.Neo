namespace PCL_CE.Neo.Core.Logging;

/// <summary>
/// Log level.
/// </summary>
public enum LogLevel
{
    Trace = 000 + ActionLevel.TraceLog,
    Debug = 100 + ActionLevel.NormalLog,
    Info = 200 + ActionLevel.NormalLog,
    Warning = 300 + ActionLevel.NormalLog,
    Error = 400 + ActionLevel.HintErr,
    Fatal = 500 + ActionLevel.MsgBoxFatal,
}

public static class LogLevelExtensions
{
    private static readonly Dictionary<LogLevel, string> _LevelNameMap = new()
    {
        [LogLevel.Trace] = "TRA",
        [LogLevel.Debug] = "DBG",
        [LogLevel.Info] = "INFO",
        [LogLevel.Warning] = "WARN",
        [LogLevel.Error] = "ERR!",
        [LogLevel.Fatal] = "FTL!"
    };

    public static string PrintName(this LogLevel level) => _LevelNameMap[level];
    public static ActionLevel DefaultActionLevel(this LogLevel level) => (ActionLevel)((int)level % 100);

    public static LogLevel RealLevel(this LogLevel level) => (int)level switch
    {
        < 100 => LogLevel.Trace,
        < 200 => LogLevel.Debug,
        < 300 => LogLevel.Info,
        < 400 => LogLevel.Warning,
        < 500 => LogLevel.Error,
        _ => LogLevel.Fatal,
    };

    public static int Header(this LogLevel level) => (int)level / 100 * 100;
}
