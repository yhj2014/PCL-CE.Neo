using System;
using System.Security.Cryptography;

namespace PCL.CE.Neo.Core.Utils;

public static class RandomUtils
{
    private static readonly Random _random = new();
    private static readonly object _lock = new();

    public static int Next(int minValue, int maxValue)
    {
        lock (_lock)
        {
            return _random.Next(minValue, maxValue);
        }
    }

    public static int Next(int maxValue)
    {
        lock (_lock)
        {
            return _random.Next(maxValue);
        }
    }

    public static double NextDouble()
    {
        lock (_lock)
        {
            return _random.NextDouble();
        }
    }

    public static void NextBytes(byte[] buffer)
    {
        lock (_lock)
        {
            _random.NextBytes(buffer);
        }
    }

    public static bool NextBool()
    {
        lock (_lock)
        {
            return _random.Next(2) == 0;
        }
    }

    public static T RandomItem<T>(T[] items)
    {
        if (items == null || items.Length == 0)
            throw new ArgumentException("Array cannot be null or empty", nameof(items));

        lock (_lock)
        {
            return items[_random.Next(items.Length)];
        }
    }

    public static T RandomItem<T>(List<T> items)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("List cannot be null or empty", nameof(items));

        lock (_lock)
        {
            return items[_random.Next(items.Count)];
        }
    }

    public static T[] Shuffle<T>(T[] array)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));

        var result = new T[array.Length];
        array.CopyTo(result, 0);

        lock (_lock)
        {
            for (int i = result.Length - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (result[j], result[i]) = (result[i], result[j]);
            }
        }

        return result;
    }

    public static List<T> Shuffle<T>(List<T> list)
    {
        if (list == null) throw new ArgumentNullException(nameof(list));

        var result = new List<T>(list);

        lock (_lock)
        {
            for (int i = result.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (result[j], result[i]) = (result[i], result[j]);
            }
        }

        return result;
    }

    public static string RandomString(int length)
    {
        return RandomString(length, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
    }

    public static string RandomString(int length, string chars)
    {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        if (string.IsNullOrEmpty(chars)) throw new ArgumentException("Chars cannot be null or empty", nameof(chars));

        var result = new char[length];
        var charsArray = chars.ToCharArray();

        lock (_lock)
        {
            for (int i = 0; i < length; i++)
            {
                result[i] = charsArray[_random.Next(charsArray.Length)];
            }
        }

        return new string(result);
    }

    public static string RandomHexString(int length)
    {
        return RandomString(length, "0123456789ABCDEF");
    }

    public static string RandomBase64String(int byteLength)
    {
        var bytes = SecureRandomBytes(byteLength);
        return Convert.ToBase64String(bytes);
    }

    public static byte[] SecureRandomBytes(int length)
    {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);
        return bytes;
    }

    public static int SecureRandomInt(int minValue, int maxValue)
    {
        if (minValue >= maxValue) throw new ArgumentOutOfRangeException(nameof(minValue));

        var range = (long)maxValue - minValue;
        var bytes = SecureRandomBytes(4);
        var randomValue = BitConverter.ToUInt32(bytes, 0);
        var scaledValue = (long)(randomValue % range);
        return (int)(minValue + scaledValue);
    }

    public static double SecureRandomDouble()
    {
        var bytes = SecureRandomBytes(8);
        var randomValue = BitConverter.ToUInt64(bytes, 0);
        return randomValue / (double)ulong.MaxValue;
    }

    public static T WeightedRandom<T>(IEnumerable<(T Item, double Weight)> items)
    {
        var itemList = items.ToList();
        if (itemList.Count == 0)
            throw new ArgumentException("Items cannot be empty", nameof(items));

        var totalWeight = itemList.Sum(i => i.Weight);
        if (totalWeight <= 0)
            throw new ArgumentException("Total weight must be positive", nameof(items));

        var randomValue = NextDouble() * totalWeight;
        var accumulatedWeight = 0.0;

        foreach (var (item, weight) in itemList)
        {
            accumulatedWeight += weight;
            if (randomValue < accumulatedWeight)
            {
                return item;
            }
        }

        return itemList.Last().Item;
    }

    public static int RandomEnumValue<T>() where T : Enum
    {
        var values = Enum.GetValues(typeof(T));
        lock (_lock)
        {
            return (int)values.GetValue(_random.Next(values.Length))!;
        }
    }
}