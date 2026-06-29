using System;

namespace PCL.CE.Neo.Core.Utils;

public static class TimeUtils
{
    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
    }

    public static long ToUnixTimestampMs(this DateTime dateTime)
    {
        return (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalMilliseconds;
    }

    public static DateTime FromUnixTimestamp(long timestamp)
    {
        return UnixEpoch.AddSeconds(timestamp).ToLocalTime();
    }

    public static DateTime FromUnixTimestampMs(long timestamp)
    {
        return UnixEpoch.AddMilliseconds(timestamp).ToLocalTime();
    }

    public static long CurrentUnixTimestamp()
    {
        return DateTime.UtcNow.ToUnixTimestamp();
    }

    public static long CurrentUnixTimestampMs()
    {
        return DateTime.UtcNow.ToUnixTimestampMs();
    }

    public static string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalDays >= 1)
        {
            return $"{(int)timeSpan.TotalDays}天 {timeSpan.Hours}小时 {timeSpan.Minutes}分钟";
        }
        if (timeSpan.TotalHours >= 1)
        {
            return $"{(int)timeSpan.TotalHours}小时 {timeSpan.Minutes}分钟 {timeSpan.Seconds}秒";
        }
        if (timeSpan.TotalMinutes >= 1)
        {
            return $"{(int)timeSpan.TotalMinutes}分钟 {timeSpan.Seconds}秒";
        }
        if (timeSpan.TotalSeconds >= 1)
        {
            return $"{timeSpan.Seconds}秒 {timeSpan.Milliseconds}毫秒";
        }
        return $"{timeSpan.Milliseconds}毫秒";
    }

    public static string FormatDuration(long milliseconds)
    {
        return FormatTimeSpan(TimeSpan.FromMilliseconds(milliseconds));
    }

    public static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public static string FormatDateTimeUtc(DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss UTC");
    }

    public static bool IsSameDay(DateTime date1, DateTime date2)
    {
        return date1.Date == date2.Date;
    }

    public static bool IsSameWeek(DateTime date1, DateTime date2)
    {
        var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        var d1 = date1.Date;
        var d2 = date2.Date;
        var w1 = cal.GetWeekOfYear(d1, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        var w2 = cal.GetWeekOfYear(d2, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        return w1 == w2 && d1.Year == d2.Year;
    }

    public static bool IsSameMonth(DateTime date1, DateTime date2)
    {
        return date1.Year == date2.Year && date1.Month == date2.Month;
    }

    public static bool IsSameYear(DateTime date1, DateTime date2)
    {
        return date1.Year == date2.Year;
    }

    public static DateTime StartOfDay(DateTime dateTime)
    {
        return dateTime.Date;
    }

    public static DateTime EndOfDay(DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }

    public static DateTime StartOfWeek(DateTime dateTime, DayOfWeek startDay = DayOfWeek.Monday)
    {
        var diff = dateTime.DayOfWeek - startDay;
        if (diff < 0) diff += 7;
        return dateTime.Date.AddDays(-diff);
    }

    public static DateTime EndOfWeek(DateTime dateTime, DayOfWeek startDay = DayOfWeek.Monday)
    {
        return StartOfWeek(dateTime, startDay).AddDays(7).AddTicks(-1);
    }

    public static DateTime StartOfMonth(DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    public static DateTime EndOfMonth(DateTime dateTime)
    {
        return StartOfMonth(dateTime).AddMonths(1).AddTicks(-1);
    }

    public static DateTime StartOfYear(DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 1, 1);
    }

    public static DateTime EndOfYear(DateTime dateTime)
    {
        return StartOfYear(dateTime).AddYears(1).AddTicks(-1);
    }

    public static int GetDaysInMonth(int year, int month)
    {
        return DateTime.DaysInMonth(year, month);
    }

    public static int GetDaysInMonth(DateTime dateTime)
    {
        return DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
    }

    public static bool IsLeapYear(int year)
    {
        return DateTime.IsLeapYear(year);
    }

    public static bool IsLeapYear(DateTime dateTime)
    {
        return DateTime.IsLeapYear(dateTime.Year);
    }

    public static DateTime GetNextWeekday(DateTime startDate, DayOfWeek day)
    {
        var daysToAdd = ((int)day - (int)startDate.DayOfWeek + 7) % 7;
        if (daysToAdd == 0) daysToAdd = 7;
        return startDate.AddDays(daysToAdd);
    }

    public static DateTime GetPreviousWeekday(DateTime startDate, DayOfWeek day)
    {
        var daysToSubtract = ((int)startDate.DayOfWeek - (int)day + 7) % 7;
        if (daysToSubtract == 0) daysToSubtract = 7;
        return startDate.AddDays(-daysToSubtract);
    }

    public static int GetAge(DateTime birthDate)
    {
        var now = DateTime.Now;
        var age = now.Year - birthDate.Year;
        if (now.Month < birthDate.Month || (now.Month == birthDate.Month && now.Day < birthDate.Day))
        {
            age--;
        }
        return age;
    }

    public static DateTime ParseIso8601(string iso8601String)
    {
        return DateTime.Parse(iso8601String, null, System.Globalization.DateTimeStyles.RoundtripKind);
    }

    public static bool TryParseIso8601(string iso8601String, out DateTime result)
    {
        return DateTime.TryParse(iso8601String, null, System.Globalization.DateTimeStyles.RoundtripKind, out result);
    }

    public static string ToIso8601(DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToString("o");
    }
}