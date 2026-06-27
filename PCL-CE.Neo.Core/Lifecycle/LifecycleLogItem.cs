using System;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.Lifecycle;

/// <summary>
/// 生命周期日志项
/// </summary>
public readonly record struct LifecycleLogItem(
    ILifecycleService? Source,
    string Message,
    Exception? Exception,
    LogLevel Level,
    ActionLevel ActionLevel)
{
    /// <summary>
    /// 创建该日志项的时间
    /// </summary>
    public DateTime Time { get; } = DateTime.Now;

    /// <summary>
    /// 创建该日志项的 Task ID 或线程名
    /// </summary>
    public string ContextName { get; } =
        (Task.CurrentId is { } id ? $"TSK#{id}" : null)
        ?? Thread.CurrentThread.Name
        ?? $"#{Environment.CurrentManagedThreadId}";

    public override string ToString()
    {
        var source = (Source == null) ? "" : $" [{Source.Name}|{Source.Identifier}]";
        var basic = $"[{Time:HH:mm:ss.fff}]{source}";
        return Exception == null ? $"{basic} {Message}" : $"{basic} ({Message}) {Exception.GetType().FullName}: {Exception.Message}";
    }

    public string ComposeMessage()
    {
        var source = (Source == null) ? "" : $" [{Source.Name}|{Source.Identifier}]";
        var result = $"[{Time:HH:mm:ss.fff}] [{Level}] [{ContextName}]{source} {Message}";
        if (Exception != null) result += $"\n{Exception}";
        return result;
    }
}