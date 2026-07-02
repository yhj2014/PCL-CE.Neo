using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PCL_CE.Neo.Core.Utils;

/// <summary>
/// 命令行参数构建器，用于构建游戏启动参数
/// </summary>
public class ArgumentsBuilder
{
    private readonly List<Argument> _args = [];

    private enum ArgumentStyle
    {
        Flag,

        /// <summary>
        /// 使用等号连接（key=value）
        /// </summary>
        Equals,

        /// <summary>
        /// 使用空格连接（key value）
        /// </summary>
        Space
    }

    private readonly struct Argument(string key, string? value, ArgumentStyle style)
    {
        public readonly string Key = key ?? throw new ArgumentNullException(nameof(key));
        public readonly string? Value = value;
        public readonly ArgumentStyle Style = style;
    }

    /// <summary>
    /// 添加键值对参数（自动处理空格转义）
    /// </summary>
    /// <param name="key">参数名（不带前缀）</param>
    /// <param name="value">参数值</param>
    public ArgumentsBuilder Add(string key, string value)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (value is null) throw new ArgumentNullException(nameof(value));
        _args.Add(new Argument(key, HandleValue(value), ArgumentStyle.Equals));
        return this;
    }

    /// <summary>
    /// 添加由空格连接的键值对参数（自动处理空格转义）
    /// </summary>
    /// <param name="key">参数名（不带前缀）</param>
    /// <param name="value">参数值</param>
    public ArgumentsBuilder AddWithSpace(string key, string value)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (value is null) throw new ArgumentNullException(nameof(value));
        _args.Add(new Argument(key, HandleValue(value), ArgumentStyle.Space));
        return this;
    }

    /// <summary>
    /// 添加标志参数（无值参数）
    /// </summary>
    /// <param name="flag">标志名（不带前缀）</param>
    public ArgumentsBuilder AddFlag(string flag)
    {
        if (flag is null) throw new ArgumentNullException(nameof(flag));
        _args.Add(new Argument(flag, null, ArgumentStyle.Flag));
        return this;
    }

    /// <summary>
    /// 条件添加参数（仅当condition为true时添加）
    /// </summary>
    public ArgumentsBuilder AddIf(bool condition, string key, string value)
    {
        if (condition) Add(key, value);
        return this;
    }

    /// <summary>
    /// 条件添加由空格连接的参数（仅当condition为true时添加）
    /// </summary>
    public ArgumentsBuilder AddWithSpaceIf(bool condition, string key, string value)
    {
        if (condition) AddWithSpace(key, value);
        return this;
    }

    /// <summary>
    /// 条件添加标志（仅当condition为true时添加）
    /// </summary>
    public ArgumentsBuilder AddFlagIf(bool condition, string flag)
    {
        if (condition) AddFlag(flag);
        return this;
    }

    /// <summary>
    /// 前缀样式
    /// </summary>
    public enum PrefixStyle
    {
        /// <summary>
        /// 自动（单字符用-，多字符用--）
        /// </summary>
        Auto,
        
        /// <summary>
        /// 强制单横线
        /// </summary>
        SingleLine,
        
        /// <summary>
        /// 强制双横线
        /// </summary>
        DoubleLine,
        
        /// <summary>
        /// 无前缀（用于JVM参数）
        /// </summary>
        None
    }

    /// <summary>
    /// 构建参数字符串
    /// </summary>
    /// <param name="prefixStyle">前缀样式</param>
    public string GetResult(PrefixStyle prefixStyle = PrefixStyle.Auto)
    {
        var sb = new StringBuilder();

        foreach (var arg in _args)
        {
            if (sb.Length > 0) sb.Append(' ');

            // 添加前缀
            switch (prefixStyle)
            {
                case PrefixStyle.SingleLine:
                    sb.Append('-').Append(arg.Key);
                    break;
                case PrefixStyle.DoubleLine:
                    sb.Append("--").Append(arg.Key);
                    break;
                case PrefixStyle.None:
                    sb.Append(arg.Key);
                    break;
                default: // Auto
                    sb.Append(arg.Key.Length == 1 ? "-" : "--").Append(arg.Key);
                    break;
            }

            // 添加值（如果有）
            if (arg.Value is not null)
            {
                switch (arg.Style)
                {
                    case ArgumentStyle.Equals:
                        sb.Append('=').Append(arg.Value);
                        break;
                    case ArgumentStyle.Space:
                        sb.Append(' ').Append(arg.Value);
                        break;
                }
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 获取参数数组（用于ProcessStartInfo）
    /// </summary>
    public string[] GetArgumentArray()
    {
        var result = new List<string>();
        
        foreach (var arg in _args)
        {
            if (arg.Value is null)
            {
                result.Add(arg.Key);
            }
            else
            {
                switch (arg.Style)
                {
                    case ArgumentStyle.Equals:
                        result.Add($"{arg.Key}={arg.Value}");
                        break;
                    case ArgumentStyle.Space:
                        result.Add(arg.Key);
                        result.Add(arg.Value);
                        break;
                    default:
                        result.Add(arg.Key);
                        break;
                }
            }
        }
        
        return result.ToArray();
    }

    public override string ToString() => GetResult();

    /// <summary>
    /// 清空所有参数
    /// </summary>
    public void Clear() => _args.Clear();

    /// <summary>
    /// 获取参数数量
    /// </summary>
    public int Count => _args.Count;

    private static readonly char[] CharNeedToQuote = [' ', '=', '|', '"'];

    /// <summary>
    /// 转义包含特殊字符的值（用双引号包裹）
    /// </summary>
    private static string HandleValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return $"\"{value}\"";
        
        return value.All(x => !CharNeedToQuote.Contains(x))
            ? value
            : $"\"{value.Replace("\"", "\\\"")}\"";
    }
}