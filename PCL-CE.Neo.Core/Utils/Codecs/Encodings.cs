using System;
using System.Text;

namespace PCL_CE.Neo.Core.Utils.Codecs;

public static class Encodings
{
    public static readonly Encoding Utf8 = Encoding.UTF8;
    public static readonly Encoding Utf8NoBom = new UTF8Encoding(false);
    public static readonly Encoding Utf16 = Encoding.Unicode;
    public static readonly Encoding Utf16BigEndian = Encoding.BigEndianUnicode;
    public static readonly Encoding Utf32 = Encoding.UTF32;
    public static readonly Encoding Ascii = Encoding.ASCII;
    public static readonly Encoding Default = Encoding.Default;

    public static Encoding GetEncoding(int codepage)
    {
        return Encoding.GetEncoding(codepage);
    }

    public static Encoding GetEncoding(string name)
    {
        return Encoding.GetEncoding(name);
    }

    public static bool TryGetEncoding(int codepage, out Encoding? encoding)
    {
        try
        {
            encoding = Encoding.GetEncoding(codepage);
            return true;
        }
        catch
        {
            encoding = null;
            return false;
        }
    }

    public static bool TryGetEncoding(string name, out Encoding? encoding)
    {
        try
        {
            encoding = Encoding.GetEncoding(name);
            return true;
        }
        catch
        {
            encoding = null;
            return false;
        }
    }

    public static byte[] GetPreamble(Encoding encoding)
    {
        return encoding.GetPreamble();
    }

    public static bool HasBom(byte[] bytes)
    {
        if (bytes == null || bytes.Length < 2)
            return false;

        return bytes[0] == 0xEF && bytes.Length >= 3 && bytes[1] == 0xBB && bytes[2] == 0xBF ||
               bytes[0] == 0xFF && bytes[1] == 0xFE ||
               bytes[0] == 0xFE && bytes[1] == 0xFF;
    }

    public static Encoding? DetectBom(byte[] bytes)
    {
        if (bytes == null || bytes.Length < 2)
            return null;

        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return Utf8;

        if (bytes[0] == 0xFF && bytes[1] == 0xFE)
            return Utf16;

        if (bytes[0] == 0xFE && bytes[1] == 0xFF)
            return Utf16BigEndian;

        if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x00 && bytes[3] == 0x00)
            return Utf32;

        if (bytes.Length >= 4 && bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
            return GetEncoding(1201);

        return null;
    }
}