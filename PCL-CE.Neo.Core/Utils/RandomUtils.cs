namespace PCL_CE.Neo.Core.Utils;

public static class RandomUtils
{
    private static readonly Random _random = new Random();
    private static readonly object _lock = new object();

    public static int Next(int maxValue)
    {
        lock (_lock)
            return _random.Next(maxValue);
    }

    public static int Next(int minValue, int maxValue)
    {
        lock (_lock)
            return _random.Next(minValue, maxValue);
    }

    public static double NextDouble()
    {
        lock (_lock)
            return _random.NextDouble();
    }

    public static bool NextBoolean()
    {
        lock (_lock)
            return _random.Next(2) == 0;
    }

    public static long NextInt64()
    {
        lock (_lock)
        {
            byte[] bytes = new byte[8];
            _random.NextBytes(bytes);
            return BitConverter.ToInt64(bytes, 0) & long.MaxValue;
        }
    }

    public static void Shuffle<T>(IList<T> list)
    {
        lock (_lock)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }

    public static T? RandomItem<T>(IList<T> list)
    {
        if (list == null || list.Count == 0)
            return default;

        lock (_lock)
            return list[_random.Next(list.Count)];
    }

    public static string RandomString(int length)
    {
        return RandomString(length, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
    }

    public static string RandomString(int length, string chars)
    {
        if (length <= 0)
            return string.Empty;

        char[] result = new char[length];
        lock (_lock)
        {
            for (int i = 0; i < length; i++)
                result[i] = chars[_random.Next(chars.Length)];
        }
        return new string(result);
    }

    public static string RandomHexString(int length)
    {
        return RandomString(length, "0123456789ABCDEF");
    }

    public static Guid RandomGuid()
    {
        return Guid.NewGuid();
    }

    public static byte[] RandomBytes(int length)
    {
        byte[] bytes = new byte[length];
        lock (_lock)
            _random.NextBytes(bytes);
        return bytes;
    }

    public static T RandomEnum<T>() where T : Enum
    {
        Array values = Enum.GetValues(typeof(T));
        lock (_lock)
            return (T)values.GetValue(_random.Next(values.Length))!;
    }

    public static int[] RandomIntArray(int length, int minValue, int maxValue)
    {
        int[] result = new int[length];
        lock (_lock)
        {
            for (int i = 0; i < length; i++)
                result[i] = _random.Next(minValue, maxValue);
        }
        return result;
    }

    public static double NextGaussian(double mean = 0, double standardDeviation = 1)
    {
        lock (_lock)
        {
            double u1 = 1.0 - _random.NextDouble();
            double u2 = 1.0 - _random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mean + standardDeviation * randStdNormal;
        }
    }
}