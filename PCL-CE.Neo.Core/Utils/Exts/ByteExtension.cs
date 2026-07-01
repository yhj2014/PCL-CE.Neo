using System.Text;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class ByteExtension
{
    public static string ToHexString(this byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", "");
    }

    public static string ToHexStringLower(this byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }

    public static string ToHexStringUpper(this byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", "").ToUpperInvariant();
    }

    public static byte[] FromHexString(string hex)
    {
        hex = hex.Replace("-", "").Replace(" ", "");

        if (hex.Length % 2 != 0)
            throw new ArgumentException("十六进制字符串长度必须为偶数。");

        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }

        return bytes;
    }

    public static string ToBase64String(this byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }

    public static byte[] FromBase64String(string base64)
    {
        return Convert.FromBase64String(base64);
    }

    public static string ToUtf8String(this byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    public static string ToUtf16String(this byte[] bytes)
    {
        return Encoding.Unicode.GetString(bytes);
    }

    public static string ToAsciiString(this byte[] bytes)
    {
        return Encoding.ASCII.GetString(bytes);
    }

    public static byte[] FromUtf8String(string str)
    {
        return Encoding.UTF8.GetBytes(str);
    }

    public static byte[] FromUtf16String(string str)
    {
        return Encoding.Unicode.GetBytes(str);
    }

    public static byte[] FromAsciiString(string str)
    {
        return Encoding.ASCII.GetBytes(str);
    }

    public static bool StartsWith(this byte[] bytes, byte[] prefix)
    {
        if (prefix.Length > bytes.Length)
            return false;

        for (int i = 0; i < prefix.Length; i++)
        {
            if (bytes[i] != prefix[i])
                return false;
        }

        return true;
    }

    public static bool EndsWith(this byte[] bytes, byte[] suffix)
    {
        if (suffix.Length > bytes.Length)
            return false;

        for (int i = 0; i < suffix.Length; i++)
        {
            if (bytes[bytes.Length - suffix.Length + i] != suffix[i])
                return false;
        }

        return true;
    }

    public static int IndexOf(this byte[] bytes, byte[] pattern)
    {
        if (pattern.Length == 0)
            return 0;

        for (int i = 0; i <= bytes.Length - pattern.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (bytes[i + j] != pattern[j])
                {
                    match = false;
                    break;
                }
            }

            if (match)
                return i;
        }

        return -1;
    }

    public static byte[] Slice(this byte[] bytes, int start)
    {
        return Slice(bytes, start, bytes.Length - start);
    }

    public static byte[] Slice(this byte[] bytes, int start, int length)
    {
        if (start < 0 || start >= bytes.Length)
            throw new ArgumentOutOfRangeException(nameof(start));

        if (length < 0 || start + length > bytes.Length)
            throw new ArgumentOutOfRangeException(nameof(length));

        byte[] result = new byte[length];
        Array.Copy(bytes, start, result, 0, length);
        return result;
    }

    public static byte[] Concat(this byte[] first, byte[] second)
    {
        byte[] result = new byte[first.Length + second.Length];
        Array.Copy(first, result, first.Length);
        Array.Copy(second, 0, result, first.Length, second.Length);
        return result;
    }

    public static byte[] Concat(this byte[] first, byte[] second, byte[] third)
    {
        byte[] result = new byte[first.Length + second.Length + third.Length];
        Array.Copy(first, result, first.Length);
        Array.Copy(second, 0, result, first.Length, second.Length);
        Array.Copy(third, 0, result, first.Length + second.Length, third.Length);
        return result;
    }

    public static byte[] Combine(params byte[][] arrays)
    {
        int totalLength = arrays.Sum(a => a.Length);
        byte[] result = new byte[totalLength];
        int offset = 0;

        foreach (byte[] array in arrays)
        {
            Array.Copy(array, 0, result, offset, array.Length);
            offset += array.Length;
        }

        return result;
    }

    public static byte[] Reverse(this byte[] bytes)
    {
        byte[] result = (byte[])bytes.Clone();
        Array.Reverse(result);
        return result;
    }

    public static int CompareTo(this byte[] first, byte[] second)
    {
        int minLength = Math.Min(first.Length, second.Length);

        for (int i = 0; i < minLength; i++)
        {
            if (first[i] != second[i])
                return first[i].CompareTo(second[i]);
        }

        return first.Length.CompareTo(second.Length);
    }

    public static bool EqualsTo(this byte[] first, byte[] second)
    {
        if (ReferenceEquals(first, second))
            return true;

        if (first == null || second == null)
            return false;

        if (first.Length != second.Length)
            return false;

        for (int i = 0; i < first.Length; i++)
        {
            if (first[i] != second[i])
                return false;
        }

        return true;
    }

    public static byte[] Xor(this byte[] bytes, byte[] key)
    {
        if (key.Length == 0)
            return (byte[])bytes.Clone();

        byte[] result = new byte[bytes.Length];
        for (int i = 0; i < bytes.Length; i++)
        {
            result[i] = (byte)(bytes[i] ^ key[i % key.Length]);
        }

        return result;
    }

    public static long ToInt64(this byte[] bytes)
    {
        return BitConverter.ToInt64(bytes, 0);
    }

    public static int ToInt32(this byte[] bytes)
    {
        return BitConverter.ToInt32(bytes, 0);
    }

    public static short ToInt16(this byte[] bytes)
    {
        return BitConverter.ToInt16(bytes, 0);
    }

    public static ulong ToUInt64(this byte[] bytes)
    {
        return BitConverter.ToUInt64(bytes, 0);
    }

    public static uint ToUInt32(this byte[] bytes)
    {
        return BitConverter.ToUInt32(bytes, 0);
    }

    public static ushort ToUInt16(this byte[] bytes)
    {
        return BitConverter.ToUInt16(bytes, 0);
    }

    public static float ToSingle(this byte[] bytes)
    {
        return BitConverter.ToSingle(bytes, 0);
    }

    public static double ToDouble(this byte[] bytes)
    {
        return BitConverter.ToDouble(bytes, 0);
    }

    public static byte[] GetBytes(long value)
    {
        return BitConverter.GetBytes(value);
    }

    public static byte[] GetBytes(int value)
    {
        return BitConverter.GetBytes(value);
    }

    public static byte[] GetBytes(short value)
    {
        return BitConverter.GetBytes(value);
    }

    public static byte[] GetBytes(ulong value)
    {
        return BitConverter.GetBytes(value);
    }

    public static byte[] GetBytes(uint value)
    {
        return BitConverter.GetBytes(value);
    }

    public static byte[] GetBytes(ushort value)
    {
        return BitConverter.GetBytes(value);
    }

    public static byte[] GetBytes(float value)
    {
        return BitConverter.GetBytes(value);
    }

    public static byte[] GetBytes(double value)
    {
        return BitConverter.GetBytes(value);
    }

    public static byte[] GetBytes(bool value)
    {
        return BitConverter.GetBytes(value);
    }

    public static byte[] PadLeft(this byte[] bytes, int totalLength, byte paddingValue = 0)
    {
        if (bytes.Length >= totalLength)
            return bytes;

        byte[] result = new byte[totalLength];
        Array.Fill(result, paddingValue);
        Array.Copy(bytes, 0, result, totalLength - bytes.Length, bytes.Length);
        return result;
    }

    public static byte[] PadRight(this byte[] bytes, int totalLength, byte paddingValue = 0)
    {
        if (bytes.Length >= totalLength)
            return bytes;

        byte[] result = new byte[totalLength];
        Array.Copy(bytes, result, bytes.Length);
        Array.Fill(result, paddingValue, bytes.Length, totalLength - bytes.Length);
        return result;
    }
}