using System.Text;

namespace PCL_CE.Neo.Core.Utils.Codecs;

public static class EncodingUtils
{
    public static string ConvertEncoding(string text, Encoding fromEncoding, Encoding toEncoding)
    {
        var bytes = fromEncoding.GetBytes(text);
        return toEncoding.GetString(bytes);
    }

    public static string ConvertEncoding(string text, string fromEncodingName, string toEncodingName)
    {
        var fromEncoding = Encoding.GetEncoding(fromEncodingName);
        var toEncoding = Encoding.GetEncoding(toEncodingName);
        return ConvertEncoding(text, fromEncoding, toEncoding);
    }

    public static string ConvertToUtf8(string text, Encoding fromEncoding)
    {
        return ConvertEncoding(text, fromEncoding, Encodings.UTF8);
    }

    public static string ConvertToUtf8(string text, string fromEncodingName)
    {
        var fromEncoding = Encoding.GetEncoding(fromEncodingName);
        return ConvertToUtf8(text, fromEncoding);
    }

    public static byte[] ConvertEncoding(byte[] data, Encoding fromEncoding, Encoding toEncoding)
    {
        var text = fromEncoding.GetString(data);
        return toEncoding.GetBytes(text);
    }

    public static byte[] ConvertToUtf8Bytes(byte[] data, Encoding fromEncoding)
    {
        return ConvertEncoding(data, fromEncoding, Encodings.UTF8);
    }

    public static bool IsValidUtf8(byte[] data)
    {
        try
        {
            Encodings.UTF8.GetString(data);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool HasUtf8Bom(byte[] data)
    {
        return data.Length >= 3 &&
               data[0] == 0xEF &&
               data[1] == 0xBB &&
               data[2] == 0xBF;
    }

    public static bool HasUtf16LeBom(byte[] data)
    {
        return data.Length >= 2 &&
               data[0] == 0xFF &&
               data[1] == 0xFE;
    }

    public static bool HasUtf16BeBom(byte[] data)
    {
        return data.Length >= 2 &&
               data[0] == 0xFE &&
               data[1] == 0xFF;
    }

    public static byte[] RemoveBom(byte[] data)
    {
        if (HasUtf8Bom(data))
            return data.Skip(3).ToArray();

        if (HasUtf16LeBom(data) || HasUtf16BeBom(data))
            return data.Skip(2).ToArray();

        return data;
    }

    public static byte[] AddUtf8Bom(byte[] data)
    {
        if (HasUtf8Bom(data))
            return data;

        var bom = new byte[] { 0xEF, 0xBB, 0xBF };
        return bom.Concat(data).ToArray();
    }

    public static byte[] AddUtf16LeBom(byte[] data)
    {
        if (HasUtf16LeBom(data))
            return data;

        var bom = new byte[] { 0xFF, 0xFE };
        return bom.Concat(data).ToArray();
    }

    public static byte[] AddUtf16BeBom(byte[] data)
    {
        if (HasUtf16BeBom(data))
            return data;

        var bom = new byte[] { 0xFE, 0xFF };
        return bom.Concat(data).ToArray();
    }

    public static int GetByteCount(string text, Encoding encoding)
    {
        return encoding.GetByteCount(text);
    }

    public static int GetCharCount(byte[] data, Encoding encoding)
    {
        return encoding.GetCharCount(data);
    }

    public static string GetString(byte[] data, Encoding encoding)
    {
        return encoding.GetString(data);
    }

    public static byte[] GetBytes(string text, Encoding encoding)
    {
        return encoding.GetBytes(text);
    }

    public static byte[] GetBytes(char[] chars, Encoding encoding)
    {
        return encoding.GetBytes(chars);
    }

    public static string GetString(byte[] data, int index, int count, Encoding encoding)
    {
        return encoding.GetString(data, index, count);
    }

    public static int GetBytes(string text, int charIndex, int charCount, byte[] bytes, int byteIndex, Encoding encoding)
    {
        return encoding.GetBytes(text, charIndex, charCount, bytes, byteIndex);
    }

    public static string NormalizeLineEndings(string text, string lineEnding = "\n")
    {
        return text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", lineEnding);
    }

    public static string NormalizeToCrLf(string text)
    {
        return NormalizeLineEndings(text, "\r\n");
    }

    public static string NormalizeToLf(string text)
    {
        return NormalizeLineEndings(text, "\n");
    }

    public static string NormalizeToCr(string text)
    {
        return NormalizeLineEndings(text, "\r");
    }
}