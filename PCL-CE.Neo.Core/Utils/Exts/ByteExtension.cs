using System;
using System.Text;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class ByteExtension
{
    public static string ToHexString(this byte[] bytes) =>
        BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();

    public static string ToBase64String(this byte[] bytes) =>
        Convert.ToBase64String(bytes);

    public static string ToBase64UrlSafe(this byte[] bytes) =>
        Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');

    public static string ToUtf8String(this byte[] bytes) =>
        Encoding.UTF8.GetString(bytes);

    public static string ToAsciiString(this byte[] bytes) =>
        Encoding.ASCII.GetString(bytes);

    public static byte[] FromHexString(string hex)
    {
        var result = new byte[hex.Length / 2];
        for (int i = 0; i < hex.Length; i += 2)
        {
            result[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return result;
    }

    public static byte[] FromBase64String(string base64) =>
        Convert.FromBase64String(base64);

    public static byte[] ToUtf8Bytes(this string str) =>
        Encoding.UTF8.GetBytes(str);

    public static byte[] ToAsciiBytes(this string str) =>
        Encoding.ASCII.GetBytes(str);

    public static byte[] Combine(params byte[][] arrays)
    {
        var totalLength = arrays.Sum(a => a.Length);
        var result = new byte[totalLength];
        var offset = 0;
        foreach (var array in arrays)
        {
            Buffer.BlockCopy(array, 0, result, offset, array.Length);
            offset += array.Length;
        }
        return result;
    }

    public static bool EqualsBytes(this byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }
}