using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.Adapters;

/// <summary>
/// Adapter for logging functionality
/// </summary>
public class LoggerAdapter : ILoggerAdapter
{
    public void Trace(string message, params object[] args)
    {
        LogWrapper.Trace(string.Format(message, args));
    }

    public void Debug(string message, params object[] args)
    {
        LogWrapper.Debug(string.Format(message, args));
    }

    public void Information(string message, params object[] args)
    {
        LogWrapper.Info(string.Format(message, args));
    }

    public void Warning(string message, params object[] args)
    {
        LogWrapper.Warn(string.Format(message, args));
    }

    public void Warning(Exception? ex, string message, params object[] args)
    {
        if (ex != null)
        {
            LogWrapper.Error(ex, message: string.Format(message, args));
        }
        else
        {
            LogWrapper.Warn(string.Format(message, args));
        }
    }

    public void Error(string message, params object[] args)
    {
        LogWrapper.Error(string.Format(message, args));
    }

    public void Error(Exception? ex, string message, params object[] args)
    {
        if (ex != null)
        {
            LogWrapper.Error(ex, message: string.Format(message, args));
        }
        else
        {
            LogWrapper.Error(string.Format(message, args));
        }
    }

    public void Fatal(string message, params object[] args)
    {
        LogWrapper.Fatal(string.Format(message, args));
    }

    public void Fatal(Exception? ex, string message, params object[] args)
    {
        if (ex != null)
        {
            LogWrapper.Fatal(ex, message: string.Format(message, args));
        }
        else
        {
            LogWrapper.Fatal(string.Format(message, args));
        }
    }

    public IDisposable? BeginScope(string scope)
    {
        // TODO: Implement scope
        return null;
    }

    public bool IsEnabled(Abstractions.LogLevel level)
    {
        return true;
    }

    public void SetLevel(Abstractions.LogLevel level)
    {
        // TODO: Implement level setting
    }
}
