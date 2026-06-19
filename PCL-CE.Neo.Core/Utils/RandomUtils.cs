using System.Security.Cryptography;

namespace PCL_CE.Neo.Core.Utils;

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

    public static string GenerateRandomString(int length, bool uppercase = true, bool lowercase = true, bool numbers = true)
    {
        var chars = string.Empty;
        if (uppercase) chars += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        if (lowercase) chars += "abcdefghijklmnopqrstuvwxyz";
        if (numbers) chars += "0123456789";

        if (string.IsNullOrEmpty(chars))
            throw new ArgumentException("At least one character type must be enabled");

        var result = new char[length];
        lock (_lock)
        {
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[_random.Next(chars.Length)];
            }
        }
        return new string(result);
    }

    public static string GenerateSecureRandomString(int length)
    {
        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);
        var chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = chars[bytes[i] % chars.Length];
        }
        return new string(result);
    }

    public static byte[] GenerateSecureRandomBytes(int length)
    {
        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);
        return bytes;
    }
}