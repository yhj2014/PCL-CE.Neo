using System.Diagnostics;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.App;

/// <summary>
/// 日志服务，负责初始化和管理 Logger
/// </summary>
public static class LogService
{
    private static Logger? _logger;
    private static bool _initialized;

    /// <summary>
    /// 获取当前 Logger 实例
    /// </summary>
    public static Logger Logger => _logger ?? throw new InvalidOperationException("LogService not initialized");

    /// <summary>
    /// 初始化日志服务
    /// </summary>
    public static void Initialize()
    {
        if (_initialized) return;

        try
        {
            var logDirectory = Path.Combine(Paths.Data, "Log");
            var config = new LoggerConfiguration(logDirectory);
            _logger = new Logger(config);

            // 连接 LogWrapper 到我们的 Logger
            LogWrapper.OnLog += OnLog;
            
            _logger.Info("LogService 初始化完成");
            _initialized = true;
        }
        catch (Exception ex)
        {
            WriteInitializationError(ex);
        }
    }

    private static void WriteInitializationError(Exception ex)
    {
        try
        {
            var errorLogPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PCL-CE.Neo",
                "Error.log");
            Directory.CreateDirectory(Path.GetDirectoryName(errorLogPath)!);
            File.AppendAllText(errorLogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LogService initialization failed: {ex}\n");
        }
        catch
        {
            Debug.WriteLine($"LogService initialization failed: {ex}");
        }
    }

    /// <summary>
    /// 清理日志服务资源
    /// </summary>
    public static async ValueTask DisposeAsync()
    {
        if (!_initialized) return;

        LogWrapper.OnLog -= OnLog;
        
        if (_logger != null)
        {
            await _logger.DisposeAsync();
        }
        
        _logger = null;
        _initialized = false;
    }

    private static void OnLog(LogLevel level, string message, string? module, Exception? ex)
    {
        try
        {
            var threadName = Thread.CurrentThread.Name ?? $"#{Environment.CurrentManagedThreadId}";
            var formattedMessage = string.IsNullOrEmpty(module) 
                ? $"[{DateTime.Now:HH:mm:ss.fff}] [{level.PrintName()}] [{threadName}] {message}"
                : $"[{DateTime.Now:HH:mm:ss.fff}] [{level.PrintName()}] [{threadName}] [{module}] {message}";

            if (ex != null)
            {
                formattedMessage += $"\n{ex}";
            }

            _logger?.Log(formattedMessage);
        }
        catch
        {
            // 防止日志服务出错导致应用崩溃
        }
    }
}
