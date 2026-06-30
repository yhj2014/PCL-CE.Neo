using PCL_CE.Neo.Core.Logging;
using System;
using System.Text;

namespace PCL_CE.Neo.Core.Utils.Codecs;

public static class EncodingUtils
{
    private const string ModuleName = "EncodingUtils";

    public static string Convert(string text, Encoding from, Encoding to)
    {
        if (text == null)
            throw new ArgumentNullException(nameof(text));
        if (from == null)
            throw new ArgumentNullException(nameof(from));
        if (to == null)
            throw new ArgumentNullException(nameof(to));

        try
        {
            LogWrapper.Debug($"Converting text from {from.EncodingName} to {to.EncodingName}", ModuleName);
            byte[] bytes = from.GetBytes(text);
            string result = to.GetString(bytes);
            LogWrapper.Debug($"Conversion completed", ModuleName);
            return result;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "Encoding conversion failed");
            throw;
        }
    }

    public static byte[] Convert(byte[] bytes, Encoding from, Encoding to)
    {
        if (bytes == null)
            throw new ArgumentNullException(nameof(bytes));
        if (from == null)
            throw new ArgumentNullException(nameof(from));
        if (to == null)
            throw new ArgumentNullException(nameof(to));

        try
        {
            LogWrapper.Debug($"Converting {bytes.Length} bytes from {from.EncodingName} to {to.EncodingName}", ModuleName);
            string text = from.GetString(bytes);
            byte[] result = to.GetBytes(text);
            LogWrapper.Debug($"Conversion completed: {bytes.Length} -> {result.Length} bytes", ModuleName);
            return result;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "Encoding conversion failed");
            throw;
        }
    }

    public static string EnsureUtf8(string text)
    {
        if (text == null)
            return string.Empty;

        var encoder = Encoding.UTF8.GetEncoder();
        var chars = text.ToCharArray();
        var bytes = new byte[encoder.GetByteCount(chars, 0, chars.Length, true)];
        encoder.GetBytes(chars, 0, chars.Length, bytes, 0, true);

        return Encoding.UTF8.GetString(bytes);
    }

    public static byte[] EnsureUtf8Bytes(byte[] bytes)
    {
        if (bytes == null)
            throw new ArgumentNullException(nameof(bytes));

        var detector = new EncodingDetector();
        var detected = detector.Detect(bytes);

        if (detected == Encoding.UTF8)
            return bytes;

        return Convert(bytes, detected, Encoding.UTF8);
    }

    public static string RemoveBom(string text)
    {
        if (text == null)
            return string.Empty;

        if (text.Length > 0 && text[0] == '\uFEFF')
            return text.Substring(1);

        return text;
    }

    public static byte[] RemoveBom(byte[] bytes)
    {
        if (bytes == null)
            throw new ArgumentNullException(nameof(bytes));

        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return bytes[3..];

        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return bytes[2..];

        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return bytes[2..];

        if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x00 && bytes[3] == 0x00)
            return bytes[4..];

        if (bytes.Length >= 4 && bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
            return bytes[4..];

        return bytes;
    }

    public static byte[] AddBom(byte[] bytes, Encoding encoding)
    {
        if (bytes == null)
            throw new ArgumentNullException(nameof(bytes));
        if (encoding == null)
            throw new ArgumentNullException(nameof(encoding));

        var preamble = encoding.GetPreamble();
        if (preamble.Length == 0)
            return bytes;

        var result = new byte[preamble.Length + bytes.Length];
        Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
        Buffer.BlockCopy(bytes, 0, result, preamble.Length, bytes.Length);

        return result;
    }

    public static bool ContainsNonAscii(string text)
    {
        if (text == null)
            return false;

        foreach (char c in text)
        {
            if (c > 127)
                return true;
        }

        return false;
    }

    public static bool ContainsInvalidUtf8(byte[] bytes)
    {
        if (bytes == null)
            throw new ArgumentNullException(nameof(bytes));

        try
        {
            _ = Encoding.UTF8.GetString(bytes);
            return false;
        }
        catch
        {
            return true;
        }
    }
}