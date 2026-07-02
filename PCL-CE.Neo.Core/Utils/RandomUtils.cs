using System;
using System.Collections.Generic;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils;

/// <summary>
/// 提供随机数和集合随机操作的实用方法
/// </summary>
public static class RandomUtils
{
    private static readonly Random SharedRandom = Random.Shared;

    /// <summary>
    /// 从集合中随机选择一个元素
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="collection">要从中选择元素的集合</param>
    /// <returns>随机选择的元素</returns>
    /// <exception cref="ArgumentNullException">集合为 null</exception>
    /// <exception cref="ArgumentException">集合为空</exception>
    public static T PickRandom<T>(ICollection<T> collection)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));
        
        if (collection.Count == 0)
            throw new ArgumentException("集合不能为空", nameof(collection));
        
        var index = SharedRandom.Next(collection.Count);
        
        if (collection is IList<T> list)
            return list[index];
        
        return collection.Skip(index).First();
    }

    /// <summary>
    /// 生成指定范围内的随机整数（包含 min 和 max）
    /// </summary>
    /// <param name="min">范围下限（包含）</param>
    /// <param name="max">范围上限（包含）</param>
    /// <returns>随机整数，范围为 [min, max]</returns>
    /// <exception cref="ArgumentOutOfRangeException">min 大于 max</exception>
    public static int NextInt(int min, int max)
    {
        if (min > max)
            throw new ArgumentOutOfRangeException(nameof(min), "最小值不能大于最大值");
        
        return SharedRandom.Next(min, max + 1);
    }

    /// <summary>
    /// 随机打乱列表的元素，返回新列表
    /// </summary>
    /// <typeparam name="T">列表元素类型</typeparam>
    /// <param name="list">要打乱的列表</param>
    /// <returns>包含随机顺序元素的新列表</returns>
    /// <exception cref="ArgumentNullException">列表为 null</exception>
    public static List<T> Shuffle<T>(IList<T> list)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));
        
        var result = new List<T>(list);
        var n = result.Count;
        
        for (var i = n - 1; i > 0; i--)
        {
            var j = SharedRandom.Next(0, i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }

        return result;
    }

    /// <summary>
    /// 原地随机打乱列表的元素
    /// </summary>
    /// <typeparam name="T">列表元素类型</typeparam>
    /// <param name="list">要打乱的列表</param>
    /// <exception cref="ArgumentNullException">列表为 null</exception>
    public static void ShuffleInPlace<T>(IList<T> list)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));
        
        var n = list.Count;
        
        for (var i = n - 1; i > 0; i--)
        {
            var j = SharedRandom.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    /// 生成随机双精度浮点数（0.0 - 1.0）
    /// </summary>
    /// <returns>随机双精度浮点数</returns>
    public static double NextDouble()
    {
        return SharedRandom.NextDouble();
    }

    /// <summary>
    /// 生成指定长度的随机字节数组
    /// </summary>
    /// <param name="length">字节长度</param>
    /// <returns>随机字节数组</returns>
    public static byte[] NextBytes(int length)
    {
        if (length < 0)
            throw new ArgumentException("长度不能为负数", nameof(length));
        
        var bytes = new byte[length];
        SharedRandom.NextBytes(bytes);
        return bytes;
    }

    /// <summary>
    /// 生成随机布尔值
    /// </summary>
    /// <returns>随机布尔值</returns>
    public static bool NextBool()
    {
        return SharedRandom.Next(2) == 0;
    }

    /// <summary>
    /// 生成随机字符串（指定长度和字符集）
    /// </summary>
    /// <param name="length">长度</param>
    /// <param name="charSet">字符集（默认为字母数字）</param>
    /// <returns>随机字符串</returns>
    public static string NextString(int length, string charSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")
    {
        if (length < 0)
            throw new ArgumentException("长度不能为负数", nameof(length));
        
        if (string.IsNullOrEmpty(charSet))
            throw new ArgumentException("字符集不能为空", nameof(charSet));
        
        var chars = new char[length];
        for (var i = 0; i < length; i++)
        {
            chars[i] = charSet[SharedRandom.Next(charSet.Length)];
        }
        
        return new string(chars);
    }
}