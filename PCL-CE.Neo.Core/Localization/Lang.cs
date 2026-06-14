using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace PCL_CE.Neo.Core.Localization;

/// <summary>
/// 本地化文本与展示格式访问辅助类
/// </summary>
public static class Lang
{
    private static readonly HashSet<string> _EastAsianCultures = new(StringComparer.OrdinalIgnoreCase)
    {
        "zh-CN", "zh-TW", "zh-HK", "zh-MO", "ja-JP", "ko-KR", "lzh"
    };

    /// <summary>
    /// 当前本地化格式化使用的展示区域性
    /// </summary>
    public static CultureInfo Culture { get; private set; } = CultureInfo.CurrentCulture;

    /// <summary>
    /// 当前展示区域性是否为 zh-CN
    /// </summary>
    public static bool IsChineseMainland => Culture.Name == "zh-CN";

    /// <summary>
    /// 同步展示区域性
    /// </summary>
    /// <param name="culture">新的展示区域性</param>
    internal static void SyncCulture(CultureInfo culture)
    {
        Culture = culture ?? CultureInfo.CurrentCulture;
    }

    /// <summary>
    /// 获取本地化文本
    /// </summary>
    /// <param name="key">资源键</param>
    /// <returns>本地化文本</returns>
    public static string Text(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var text = LocalizationService.GetText(key);
        if (text != null) return text;

#if DEBUG
        return $"!{key}!";
#else
        return key;
#endif
    }

    /// <summary>
    /// 获取本地化格式文本并格式化参数
    /// </summary>
    /// <param name="key">资源键</param>
    /// <param name="args">格式化参数</param>
    /// <returns>格式化后的本地化文本</returns>
    public static string Text(string key, params object?[] args)
    {
        return string.Format(Culture, Text(key), args);
    }

    /// <summary>
    /// 使用当前展示区域性格式化日期时间
    /// </summary>
    /// <param name="value">要格式化的日期时间</param>
    /// <param name="format">格式字符串，默认 G</param>
    /// <returns>格式化后的日期时间文本</returns>
    public static string Date(DateTime value, string format = "G")
    {
        return value.ToString(format, Culture);
    }

    /// <summary>
    /// 使用当前展示区域性格式化日期时间
    /// </summary>
    /// <param name="value">要格式化的日期时间</param>
    /// <param name="format">格式字符串，默认 G</param>
    /// <returns>格式化后的日期时间文本</returns>
    public static string Date(DateTimeOffset value, string format = "G")
    {
        return value.ToString(format, Culture);
    }

    /// <summary>
    /// 使用当前展示区域性格式化时间间隔
    /// </summary>
    /// <param name="span">时间间隔</param>
    /// <param name="precision">显示的时间单位数量</param>
    /// <param name="addAffixes">是否添加过去/未来语义</param>
    /// <returns>格式化后的时间间隔文本</returns>
    public static string TimeSpan(TimeSpan span, int precision = 2, bool addAffixes = true)
    {
        var isPast = span.TotalMilliseconds < 0;
        if (isPast) span = span.Negate();

        var text = FormatTimeSpan(span, precision);

        return addAffixes
            ? Text(isPast ? "Common.Format.TimeSpan.Past" : "Common.Format.TimeSpan.Future", text)
            : text;
    }

    /// <summary>
    /// 使用当前展示区域性格式化数值
    /// </summary>
    /// <typeparam name="T">数值类型</typeparam>
    /// <param name="value">要格式化的值</param>
    /// <param name="format">格式字符串</param>
    /// <returns>格式化后的文本</returns>
    public static string Number<T>(T value, string? format = null) where T : IFormattable
    {
        return value.ToString(format, Culture);
    }

    /// <summary>
    /// 使用当前展示区域性格式化紧凑数值
    /// </summary>
    /// <param name="value">要格式化的整数</param>
    /// <returns>格式化后的紧凑数值文本</returns>
    public static string CompactNumber(long value)
    {
        var absValue = Math.Abs((double)value);
        var sign = value < 0 ? -1 : 1;

        if (IsEastAsianCulture(Culture.Name))
        {
            return absValue switch
            {
                > 1_000_000_000_000d => Text("Common.Format.Number.Digit3", Number(sign * absValue / 1_000_000_000_000d, "N2")),
                > 100_000_000d => Text("Common.Format.Number.Digit2", Number(sign * absValue / 100_000_000d, "N2")),
                > 100_000d => Text("Common.Format.Number.Digit1", Number(sign * Math.Round(absValue / 10_000d), "N0")),
                _ => Number(value, "N0")
            };
        }

        return absValue switch
        {
            > 1_000_000_000d => Text("Common.Format.Number.Digit3", Number(sign * absValue / 1_000_000_000d, "N2")),
            > 1_000_000d => Text("Common.Format.Number.Digit2", Number(sign * absValue / 1_000_000d, "N2")),
            > 10_000d => Text("Common.Format.Number.Digit1", Number(sign * Math.Round(absValue / 1_000d), "N0")),
            _ => Number(value, "N0")
        };
    }

    /// <summary>
    /// 使用当前展示区域性格式化文件大小
    /// </summary>
    /// <param name="length">字节数</param>
    /// <param name="startUnit">起始单位索引，0 为 B</param>
    /// <returns>格式化后的文件大小文本</returns>
    public static string FileSize(long length, int startUnit = 0)
    {
        return FormatFileSize(length, startUnit);
    }

    private static bool IsEastAsianCulture(string cultureName)
    {
        return _EastAsianCultures.Contains(cultureName) ||
               cultureName.StartsWith("zh", StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatTimeSpan(global::System.TimeSpan span, int precision)
    {
        var parts = new List<string>();
        var remainingDays = span.TotalDays;

        if (remainingDays >= 365)
        {
            var years = (int)(remainingDays / 365);
            parts.Add(Text("Common.Format.TimeSpan.Year", years));
            remainingDays -= years * 365;
        }

        if (remainingDays >= 30 && parts.Count < precision)
        {
            var months = (int)(remainingDays / 30);
            parts.Add(Text("Common.Format.TimeSpan.Month", months));
            remainingDays -= months * 30;
        }

        var remainingTs = global::System.TimeSpan.FromDays(remainingDays);

        if (remainingTs.Days > 0 && parts.Count < precision)
        {
            parts.Add(Text("Common.Format.TimeSpan.Day", remainingTs.Days));
        }

        if (remainingTs.Hours > 0 && parts.Count < precision)
        {
            parts.Add(Text("Common.Format.TimeSpan.Hour", remainingTs.Hours));
        }

        if (remainingTs.Minutes > 0 && parts.Count < precision)
        {
            parts.Add(Text("Common.Format.TimeSpan.Minute", remainingTs.Minutes));
        }

        if (remainingTs.Seconds > 0 && parts.Count < precision)
        {
            parts.Add(Text("Common.Format.TimeSpan.Second", remainingTs.Seconds));
        }

        return parts.Count > 0 ? string.Join(" ", parts) : Text("Common.Format.TimeSpan.Second", 0);
    }

    private static string FormatFileSize(long length, int startUnit = 0)
    {
        var units = new[] { "B", "KB", "MB", "GB", "TB", "PB" };
        var unitIndex = startUnit;
        var value = (double)length;

        while (value >= 1024 && unitIndex < units.Length - 1)
        {
            value /= 1024;
            unitIndex++;
        }

        return $"{Number(value, "N2")} {units[unitIndex]}";
    }
}