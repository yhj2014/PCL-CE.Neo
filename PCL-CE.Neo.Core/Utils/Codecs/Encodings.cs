using System.Text;

namespace PCL_CE.Neo.Core.Utils.Codecs;

public static class Encodings
{
    public static readonly Encoding GB18030 = Encoding.GetEncoding("GB18030");
    public static readonly Encoding GBK = Encoding.GetEncoding("GBK");
    public static readonly Encoding Big5 = Encoding.GetEncoding("Big5");
    public static readonly Encoding ShiftJIS = Encoding.GetEncoding("Shift_JIS");
    public static readonly Encoding EUCJP = Encoding.GetEncoding("EUC-JP");
    public static readonly Encoding EUCKR = Encoding.GetEncoding("EUC-KR");
    public static readonly Encoding UTF7 = Encoding.UTF7;
    public static readonly Encoding UTF8 = Encoding.UTF8;
    public static readonly Encoding UTF16 = Encoding.Unicode;
    public static readonly Encoding UTF16BE = Encoding.BigEndianUnicode;
    public static readonly Encoding UTF32 = Encoding.UTF32;
}