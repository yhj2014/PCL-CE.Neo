using System.Text;

namespace PCL_CE.Neo.Core.Utils.Codecs;

public static class EncodingUtils
{
    public static bool IsDefaultEncodingUtf8() => Encoding.Default.CodePage == 65001;

    public static bool IsDefaultEncodingGbk() => Encoding.Default.CodePage == 936;

    public static string DecodeBytes(byte[] bytes)
    {
        if (bytes.Length == 0) return "";

        var encoding = EncodingDetector.DetectEncoding(bytes);

        if (!encoding.Equals(Encoding.Default))
        {
            ReadOnlySpan<byte> span = bytes.AsSpan();
            if (encoding.Equals(Encoding.UTF8) && span.Length >= 3 && span[0] == 0xEF && span[1] == 0xBB && span[2] == 0xBF)
            {
                return encoding.GetString(span[3..]);
            }
            if (encoding.Equals(Encoding.BigEndianUnicode) && span.Length >= 2 && span[0] == 0xFE && span[1] == 0xFF)
            {
                return encoding.GetString(span[2..]);
            }
            if (encoding.Equals(Encoding.Unicode) && span.Length >= 2 && span[0] == 0xFF && span[1] == 0xFE)
            {
                return encoding.GetString(span[2..]);
            }
            if (encoding.Equals(Encoding.UTF32) && span.Length >= 4 && span[0] == 0xFF && span[1] == 0xFE && span[2] == 0x00 && span[3] == 0x00)
            {
                return encoding.GetString(span[4..]);
            }
            var utf32BeCodePage = Encoding.GetEncoding("utf-32BE").CodePage;
            if (encoding.CodePage == utf32BeCodePage && span.Length >= 4 && span[0] == 0x00 && span[1] == 0x00 && span[2] == 0xFE && span[3] == 0xFF)
            {
                return encoding.GetString(span[4..]);
            }
            return encoding.GetString(span);
        }

        try
        {
            var utf8Result = Encoding.UTF8.GetString(bytes);
            return utf8Result.Contains('\uFFFD') ? Encodings.GB18030.GetString(bytes) : utf8Result;
        }
        catch (DecoderFallbackException)
        {
            return Encodings.GB18030.GetString(bytes);
        }
    }
}