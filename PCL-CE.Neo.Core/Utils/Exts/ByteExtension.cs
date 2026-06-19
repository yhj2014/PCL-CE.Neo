namespace PCL_CE.Neo.Core.Utils.Exts;

public static class ByteExtension
{
    public static string ToHexString(this byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }

    public static string ToHexString(this byte[] bytes, bool uppercase)
    {
        var result = BitConverter.ToString(bytes).Replace("-", "");
        return uppercase ? result.ToUpperInvariant() : result.ToLowerInvariant();
    }

    public static byte[] FromHexString(this string hex)
    {
        hex = hex.Replace("-", "").Replace(" ", "");
        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytes;
    }
}