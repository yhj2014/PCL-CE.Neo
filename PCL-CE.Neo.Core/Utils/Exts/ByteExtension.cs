using System;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class ByteExtension
{
    public static string ToHexString(this byte[] bytes)
    {
        if (bytes == null) throw new ArgumentNullException(nameof(bytes));
        return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
    }

    public static byte[] FromHexString(this string hex)
    {
        if (hex == null) throw new ArgumentNullException(nameof(hex));
        var length = hex.Length;
        var bytes = new byte[length / 2];
        for (var i = 0; i < length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }
}