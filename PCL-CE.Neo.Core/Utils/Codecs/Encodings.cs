using System.Text;

namespace PCL_CE.Neo.Core.Utils.Codecs;

public static class Encodings
{
    public static readonly Encoding UTF8 = new UTF8Encoding(false);
    public static readonly Encoding UTF8WithBom = new UTF8Encoding(true);
    public static readonly Encoding UTF16 = new UnicodeEncoding(false, false);
    public static readonly Encoding UTF16BigEndian = new UnicodeEncoding(true, false);
    public static readonly Encoding UTF16WithBom = new UnicodeEncoding(false, true);
    public static readonly Encoding UTF16BigEndianWithBom = new UnicodeEncoding(true, true);
    public static readonly Encoding ASCII = Encoding.ASCII;
    public static readonly Encoding Latin1 = Encoding.GetEncoding("ISO-8859-1");
    public static readonly Encoding GB2312 = Encoding.GetEncoding("GB2312");
    public static readonly Encoding GBK = Encoding.GetEncoding("GBK");
    public static readonly Encoding GB18030 = Encoding.GetEncoding("GB18030");
    public static readonly Encoding ShiftJIS = Encoding.GetEncoding("Shift_JIS");
    public static readonly Encoding EUCJP = Encoding.GetEncoding("EUC-JP");
    public static readonly Encoding KOI8R = Encoding.GetEncoding("KOI8-R");
    public static readonly Encoding Windows1251 = Encoding.GetEncoding("Windows-1251");
    public static readonly Encoding Windows1252 = Encoding.GetEncoding("Windows-1252");
    public static readonly Encoding Big5 = Encoding.GetEncoding("Big5");

    public static Encoding GetEncodingByCodePage(int codePage)
    {
        try
        {
            return Encoding.GetEncoding(codePage);
        }
        catch
        {
            return UTF8;
        }
    }

    public static Encoding? GetEncodingByName(string name)
    {
        try
        {
            return Encoding.GetEncoding(name);
        }
        catch
        {
            return null;
        }
    }

    public static Encoding TryGetEncoding(string name, Encoding fallback = null)
    {
        return GetEncodingByName(name) ?? fallback ?? UTF8;
    }

    public static string[] GetAvailableEncodingNames()
    {
        return Encoding.GetEncodings().Select(e => e.Name).ToArray();
    }

    public static int[] GetAvailableEncodingCodePages()
    {
        return Encoding.GetEncodings().Select(e => e.CodePage).ToArray();
    }
}