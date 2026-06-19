namespace PCL_CE.Neo.Core.Utils;

public static class TimeUtils
{
    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static long ToUnixTimeSeconds(this DateTime dateTime)
    {
        return (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
    }

    public static long ToUnixTimeMilliseconds(this DateTime dateTime)
    {
        return (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalMilliseconds;
    }

    public static DateTime FromUnixTimeSeconds(long seconds)
    {
        return UnixEpoch.AddSeconds(seconds).ToLocalTime();
    }

    public static DateTime FromUnixTimeMilliseconds(long milliseconds)
    {
        return UnixEpoch.AddMilliseconds(milliseconds).ToLocalTime();
    }

    public static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalDays >= 1)
            return $"{(int)duration.TotalDays}天{(int)duration.Hours}小时";
        if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours}小时{(int)duration.Minutes}分钟";
        if (duration.TotalMinutes >= 1)
            return $"{(int)duration.TotalMinutes}分钟{(int)duration.Seconds}秒";
        return $"{(int)duration.TotalSeconds}秒";
    }

    public static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public static string FormatDateTimeUtc(DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss UTC");
    }

    public static DateTime GetCurrentUtcTime()
    {
        return DateTime.UtcNow;
    }

    public static DateTime GetCurrentLocalTime()
    {
        return DateTime.Now;
    }

    public static bool IsSameDay(DateTime date1, DateTime date2)
    {
        return date1.Date == date2.Date;
    }

    public static bool IsToday(DateTime dateTime)
    {
        return IsSameDay(dateTime, DateTime.Now);
    }

    public static bool IsYesterday(DateTime dateTime)
    {
        return IsSameDay(dateTime, DateTime.Now.AddDays(-1));
    }
}