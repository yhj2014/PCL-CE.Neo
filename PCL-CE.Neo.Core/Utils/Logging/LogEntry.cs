namespace PCL_CE.Neo.Core.Utils.Logging;

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

    public override string ToString()
    {
        string levelStr = Level.ToString().PadRight(7);
        string timeStr = Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string exceptionStr = Exception != null ? $"\n{Exception}" : string.Empty;
        return $"[{timeStr}] [{levelStr}] [{Category}] {Message}{exceptionStr}";
    }
}