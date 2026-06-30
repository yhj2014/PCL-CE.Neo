using System;

namespace PCL_CE.Neo.Core.Utils;

public static class TimeUtils
{
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }

    public static long ToUnixTimestampMilliseconds(this DateTime dateTime)
    {
        return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }

    public static DateTime FromUnixTimestamp(long timestamp)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp).ToLocalTime();
    }

    public static DateTime FromUnixTimestampMilliseconds(long timestamp)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(timestamp).ToLocalTime();
    }

    public static DateTime FromUnixTimestampUtc(long timestamp)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
    }

    public static DateTime FromUnixTimestampMillisecondsUtc(long timestamp)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(timestamp);
    }

    public static string ToRelativeTime(this DateTime dateTime)
    {
        var now = DateTime.Now;
        var diff = now - dateTime;

        if (diff.TotalSeconds < 60)
            return "刚刚";

        if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes} 分钟前";

        if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours} 小时前";

        if (diff.TotalDays < 30)
            return $"{(int)diff.TotalDays} 天前";

        if (diff.TotalDays < 365)
            return $"{(int)(diff.TotalDays / 30)} 个月前";

        return $"{(int)(diff.TotalDays / 365)} 年前";
    }

    public static string FormatDuration(long milliseconds)
    {
        var ts = TimeSpan.FromMilliseconds(milliseconds);

        if (ts.TotalHours >= 1)
            return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";

        if (ts.TotalMinutes >= 1)
            return $"{ts.Minutes:D2}:{ts.Seconds:D2}";

        return $"{ts.Seconds}.{ts.Milliseconds:D3}秒";
    }

    public static string FormatFileSize(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";

        if (bytes < 1024 * 1024)
            return $"{bytes / 1024.0:F2} KB";

        if (bytes < 1024 * 1024 * 1024)
            return $"{bytes / (1024.0 * 1024):F2} MB";

        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }
}