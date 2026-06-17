using System;

namespace PCL_CE.Neo.Core.Utils;

public static class TimeUtils
{
    public static string GetTimeNow()
    {
        return DateTime.Now.ToString("HH:mm:ss.fff");
    }

    public static long GetTimeTick()
    {
        return Environment.TickCount64;
    }

    public static string GetTimeSpanString(TimeSpan span, bool isShortForm)
    {
        var isPast = span.TotalMilliseconds < 0;
        var endFix = isPast ? "前" : "后";
        if (isPast)
        {
            span = span.Negate();
        }

        return Humanize(span, isShortForm ? 1 : 2) + endFix;
    }

    private static string Humanize(TimeSpan span, int precision)
    {
        if (span.TotalDays >= 365)
            return Format(span.TotalDays / 365, "年", precision--);
        if (span.TotalDays >= 30)
            return Format(span.TotalDays / 30, "月", precision--);
        if (span.TotalDays >= 1)
            return Format(span.TotalDays, "天", precision--);
        if (span.TotalHours >= 1)
            return Format(span.TotalHours, "小时", precision--);
        if (span.TotalMinutes >= 1)
            return Format(span.TotalMinutes, "分钟", precision--);
        return Format(span.TotalSeconds, "秒", precision);
    }

    private static string Format(double value, string unit, int precision)
    {
        var rounded = Math.Round(value, 0);
        return $"{(int)rounded}{unit}";
    }

    public static long GetUnixTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public static DateTimeOffset FromUnixTimestamp(long unixTimestamp)
    {
        if (unixTimestamp < 0)
            throw new ArgumentOutOfRangeException(nameof(unixTimestamp), "Unix 时间戳不能为负数。");

        try
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).ToLocalTime();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new ArgumentOutOfRangeException(nameof(unixTimestamp), "Unix 时间戳超出有效范围。", ex.Message);
        }
    }

    public static DateTimeOffset ToLocalTime(DateTimeOffset utcDate) =>
        utcDate.ToLocalTime();

    public static string FormatUnixTimestamp(long unixTimestamp) =>
        FromUnixTimestamp(unixTimestamp).ToString("yyyy/MM/dd HH:mm");
}