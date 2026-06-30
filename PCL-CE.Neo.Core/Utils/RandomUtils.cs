using System;
using System.Security.Cryptography;

namespace PCL_CE.Neo.Core.Utils;

public static class RandomUtils
{
    private static readonly Random _random = new();

    public static int Next(int minValue, int maxValue)
    {
        return _random.Next(minValue, maxValue);
    }

    public static int Next(int maxValue)
    {
        return _random.Next(maxValue);
    }

    public static double NextDouble()
    {
        return _random.NextDouble();
    }

    public static void NextBytes(byte[] buffer)
    {
        _random.NextBytes(buffer);
    }

    public static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var result = new char[length];
        
        for (var i = 0; i < length; i++)
        {
            result[i] = chars[_random.Next(chars.Length)];
        }
        
        return new string(result);
    }

    public static string GenerateRandomHexString(int length)
    {
        const string chars = "0123456789ABCDEF";
        var result = new char[length];
        
        for (var i = 0; i < length; i++)
        {
            result[i] = chars[_random.Next(chars.Length)];
        }
        
        return new string(result);
    }

    public static string GenerateSecureRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var result = new char[length];
        var bytes = new byte[length];
        
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        
        for (var i = 0; i < length; i++)
        {
            result[i] = chars[bytes[i] % chars.Length];
        }
        
        return new string(result);
    }

    public static byte[] GenerateSecureRandomBytes(int length)
    {
        var bytes = new byte[length];
        
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        
        return bytes;
    }

    public static string GenerateSecureRandomHexString(int length)
    {
        var bytes = new byte[length / 2];
        
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        
        return BitConverter.ToString(bytes).Replace("-", string.Empty);
    }

    public static T[] Shuffle<T>(T[] array)
    {
        var result = (T[])array.Clone();
        
        for (var i = result.Length - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }
        
        return result;
    }
}