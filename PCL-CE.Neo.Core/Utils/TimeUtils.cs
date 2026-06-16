using System;

namespace PCL_CE.Neo.Core.Utils;

public static class TimeUtils
{
    public static long ToUnixTimestamp(this DateTime dateTime) =>
        (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

    public static DateTime FromUnixTimestamp(long timestamp) =>
        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);

    public static long ToUnixMilliseconds(this DateTime dateTime) =>
        (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

    public static DateTime FromUnixMilliseconds(long milliseconds) =>
        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(milliseconds);

    public static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalDays >= 1)
            return $"{(int)duration.TotalDays}天{duration.Hours}小时{duration.Minutes}分钟";
        if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours}小时{duration.Minutes}分钟{duration.Seconds}秒";
        if (duration.TotalMinutes >= 1)
            return $"{(int)duration.TotalMinutes}分钟{duration.Seconds}秒";
        return $"{duration.Seconds}秒";
    }

    public static string FormatTime(DateTime dateTime) =>
        dateTime.ToString("yyyy-MM-dd HH:mm:ss");

    public static DateTime NowUtc => DateTime.UtcNow;

    public static DateTime NowLocal => DateTime.Now;
}