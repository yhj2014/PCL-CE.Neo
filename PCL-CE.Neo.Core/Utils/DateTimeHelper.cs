using System;

namespace PCL_CE.Neo.Core.Utils;

public static class DateTimeHelper
{
    public static DateTime ToUtc(this DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        };
    }

    public static DateTime ToLocal(this DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Local => dateTime,
            DateTimeKind.Utc => dateTime.ToLocalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).ToLocalTime()
        };
    }

    public static DateTime ToDateTime(this long timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
    }

    public static DateTime ToDateTimeMilliseconds(this long timestamp)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
    }

    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime.ToUtc()).ToUnixTimeSeconds();
    }

    public static long ToUnixTimestampMilliseconds(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime.ToUtc()).ToUnixTimeMilliseconds();
    }

    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, dateTime.Kind);
    }

    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, 999, dateTime.Kind);
    }

    public static DateTime StartOfWeek(this DateTime dateTime, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        var diff = dateTime.DayOfWeek - startOfWeek;
        if (diff < 0)
            diff += 7;

        return dateTime.AddDays(-diff).StartOfDay();
    }

    public static DateTime EndOfWeek(this DateTime dateTime, DayOfWeek endOfWeek = DayOfWeek.Sunday)
    {
        var diff = endOfWeek - dateTime.DayOfWeek;
        if (diff < 0)
            diff += 7;

        return dateTime.AddDays(diff).EndOfDay();
    }

    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, dateTime.Kind);
    }

    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month), 
            23, 59, 59, 999, dateTime.Kind);
    }

    public static DateTime StartOfQuarter(this DateTime dateTime)
    {
        var quarter = (dateTime.Month - 1) / 3 + 1;
        var month = (quarter - 1) * 3 + 1;
        return new DateTime(dateTime.Year, month, 1, 0, 0, 0, dateTime.Kind);
    }

    public static DateTime EndOfQuarter(this DateTime dateTime)
    {
        var quarter = (dateTime.Month - 1) / 3 + 1;
        var month = quarter * 3;
        return new DateTime(dateTime.Year, month, DateTime.DaysInMonth(dateTime.Year, month), 
            23, 59, 59, 999, dateTime.Kind);
    }

    public static DateTime StartOfYear(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 1, 1, 0, 0, 0, dateTime.Kind);
    }

    public static DateTime EndOfYear(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 12, 31, 23, 59, 59, 999, dateTime.Kind);
    }

    public static bool IsSameDay(this DateTime dateTime1, DateTime dateTime2)
    {
        return dateTime1.Date == dateTime2.Date;
    }

    public static bool IsSameWeek(this DateTime dateTime1, DateTime dateTime2, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        return dateTime1.StartOfWeek(startOfWeek) == dateTime2.StartOfWeek(startOfWeek);
    }

    public static bool IsSameMonth(this DateTime dateTime1, DateTime dateTime2)
    {
        return dateTime1.Year == dateTime2.Year && dateTime1.Month == dateTime2.Month;
    }

    public static bool IsSameYear(this DateTime dateTime1, DateTime dateTime2)
    {
        return dateTime1.Year == dateTime2.Year;
    }

    public static int GetDaysInMonth(this DateTime dateTime)
    {
        return DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
    }

    public static int GetDaysInYear(this DateTime dateTime)
    {
        return DateTime.IsLeapYear(dateTime.Year) ? 366 : 365;
    }

    public static int GetWeekOfYear(this DateTime dateTime, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        var startOfYear = new DateTime(dateTime.Year, 1, 1);
        var startOfWeekAdjusted = startOfYear.StartOfWeek(startOfWeek);

        if (startOfWeekAdjusted > startOfYear)
            startOfWeekAdjusted = startOfWeekAdjusted.AddDays(-7);

        var diff = dateTime - startOfWeekAdjusted;
        return (int)(diff.TotalDays / 7) + 1;
    }

    public static int GetQuarter(this DateTime dateTime)
    {
        return (dateTime.Month - 1) / 3 + 1;
    }

    public static DateTime AddBusinessDays(this DateTime dateTime, int days)
    {
        var sign = Math.Sign(days);
        var remaining = Math.Abs(days);
        var current = dateTime;

        while (remaining > 0)
        {
            current = current.AddDays(sign);
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
            {
                remaining--;
            }
        }

        return current;
    }

    public static int GetBusinessDays(this DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
            return -GetBusinessDays(endDate, startDate);

        var days = 0;
        var current = startDate;

        while (current <= endDate)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
            {
                days++;
            }
            current = current.AddDays(1);
        }

        return days;
    }

    public static string ToRelativeTime(this DateTime dateTime)
    {
        var now = DateTime.Now;
        var diff = now - dateTime;

        if (diff.TotalSeconds < 60)
            return "刚刚";

        if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes}分钟前";

        if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours}小时前";

        if (diff.TotalDays < 7)
            return $"{(int)diff.TotalDays}天前";

        if (diff.TotalDays < 30)
            return $"{(int)diff.TotalDays / 7}周前";

        if (diff.TotalDays < 365)
            return $"{(int)diff.TotalDays / 30}个月前";

        return $"{(int)diff.TotalDays / 365}年前";
    }

    public static string FormatDateTime(this DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss")
    {
        return dateTime.ToString(format);
    }

    public static string FormatDate(this DateTime dateTime, string format = "yyyy-MM-dd")
    {
        return dateTime.ToString(format);
    }

    public static string FormatTime(this DateTime dateTime, string format = "HH:mm:ss")
    {
        return dateTime.ToString(format);
    }

    public static DateTime? ParseDateTime(string dateTimeString, string? format = null)
    {
        if (string.IsNullOrEmpty(dateTimeString))
            return null;

        if (!string.IsNullOrEmpty(format))
        {
            if (DateTime.TryParseExact(dateTimeString, format, null, System.Globalization.DateTimeStyles.None, out var result))
                return result;

            return null;
        }

        if (DateTime.TryParse(dateTimeString, out var parsed))
            return parsed;

        return null;
    }

    public static bool IsLeapYear(this DateTime dateTime)
    {
        return DateTime.IsLeapYear(dateTime.Year);
    }

    public static bool IsWeekend(this DateTime dateTime)
    {
        return dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday;
    }

    public static bool IsWeekday(this DateTime dateTime)
    {
        return !dateTime.IsWeekend();
    }
}