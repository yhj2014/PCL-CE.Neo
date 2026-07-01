using System.Text;

namespace PCL_CE.Neo.Core.Utils.Codecs;

public static class EncodingDetector
{
    public static Encoding DetectEncoding(byte[] data)
    {
        if (data == null || data.Length == 0)
            return Encodings.UTF8;

        if (HasUtf8Bom(data))
            return Encodings.UTF8WithBom;

        if (HasUtf16LeBom(data))
            return Encodings.UTF16WithBom;

        if (HasUtf16BeBom(data))
            return Encodings.UTF16BigEndianWithBom;

        if (IsValidUtf8(data))
            return Encodings.UTF8;

        if (IsValidUtf16Le(data))
            return Encodings.UTF16;

        if (IsValidUtf16Be(data))
            return Encodings.UTF16BigEndian;

        if (IsProbablyGb2312(data))
            return Encodings.GB2312;

        if (IsProbablyGbk(data))
            return Encodings.GBK;

        if (IsProbablyShiftJis(data))
            return Encodings.ShiftJIS;

        return Encodings.Latin1;
    }

    public static string DetectEncodingName(byte[] data)
    {
        return DetectEncoding(data).EncodingName;
    }

    public static int DetectCodePage(byte[] data)
    {
        return DetectEncoding(data).CodePage;
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

    public static bool IsValidUtf8(byte[] data)
    {
        int i = 0;
        while (i < data.Length)
        {
            byte b = data[i];

            if (b < 0x80)
            {
                i++;
            }
            else if ((b & 0xE0) == 0xC0)
            {
                if (i + 1 >= data.Length)
                    return false;
                if ((data[i + 1] & 0xC0) != 0x80)
                    return false;
                i += 2;
            }
            else if ((b & 0xF0) == 0xE0)
            {
                if (i + 2 >= data.Length)
                    return false;
                if ((data[i + 1] & 0xC0) != 0x80 || (data[i + 2] & 0xC0) != 0x80)
                    return false;
                i += 3;
            }
            else if ((b & 0xF8) == 0xF0)
            {
                if (i + 3 >= data.Length)
                    return false;
                if ((data[i + 1] & 0xC0) != 0x80 || (data[i + 2] & 0xC0) != 0x80 || (data[i + 3] & 0xC0) != 0x80)
                    return false;
                i += 4;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsValidUtf16Le(byte[] data)
    {
        if (data.Length % 2 != 0)
            return false;

        for (int i = 0; i < data.Length; i += 2)
        {
            uint codePoint = (uint)(data[i] | (data[i + 1] << 8));

            if (codePoint >= 0xD800 && codePoint <= 0xDFFF)
            {
                if (i + 2 >= data.Length)
                    return false;

                uint nextCodePoint = (uint)(data[i + 2] | (data[i + 3] << 8));
                if (nextCodePoint < 0xDC00 || nextCodePoint > 0xDFFF)
                    return false;

                i += 2;
            }
        }

        return true;
    }

    public static bool IsValidUtf16Be(byte[] data)
    {
        if (data.Length % 2 != 0)
            return false;

        for (int i = 0; i < data.Length; i += 2)
        {
            uint codePoint = (uint)((data[i] << 8) | data[i + 1]);

            if (codePoint >= 0xD800 && codePoint <= 0xDFFF)
            {
                if (i + 2 >= data.Length)
                    return false;

                uint nextCodePoint = (uint)((data[i + 2] << 8) | data[i + 3]);
                if (nextCodePoint < 0xDC00 || nextCodePoint > 0xDFFF)
                    return false;

                i += 2;
            }
        }

        return true;
    }

    public static bool IsProbablyGb2312(byte[] data)
    {
        int validCount = 0;
        int totalCount = 0;

        for (int i = 0; i < data.Length; i++)
        {
            byte b = data[i];

            if (b < 0x80)
            {
                totalCount++;
                validCount++;
            }
            else if (b >= 0xB0 && b <= 0xF7)
            {
                if (i + 1 >= data.Length)
                    continue;

                byte next = data[i + 1];
                if (next >= 0xA1 && next <= 0xFE)
                {
                    validCount += 2;
                    totalCount += 2;
                    i++;
                }
            }
        }

        return totalCount > 0 && (double)validCount / totalCount > 0.9;
    }

    public static bool IsProbablyGbk(byte[] data)
    {
        int validCount = 0;
        int totalCount = 0;

        for (int i = 0; i < data.Length; i++)
        {
            byte b = data[i];

            if (b < 0x80)
            {
                totalCount++;
                validCount++;
            }
            else if (b >= 0x81 && b <= 0xFE)
            {
                if (i + 1 >= data.Length)
                    continue;

                byte next = data[i + 1];
                if ((next >= 0x40 && next <= 0xFE && next != 0x7F))
                {
                    validCount += 2;
                    totalCount += 2;
                    i++;
                }
            }
        }

        return totalCount > 0 && (double)validCount / totalCount > 0.9;
    }

    public static bool IsProbablyShiftJis(byte[] data)
    {
        int validCount = 0;
        int totalCount = 0;

        for (int i = 0; i < data.Length; i++)
        {
            byte b = data[i];

            if (b < 0x80)
            {
                totalCount++;
                validCount++;
            }
            else if ((b >= 0x81 && b <= 0x9F) || (b >= 0xE0 && b <= 0xFC))
            {
                if (i + 1 >= data.Length)
                    continue;

                byte next = data[i + 1];
                if ((next >= 0x40 && next <= 0xFC && next != 0x7F))
                {
                    validCount += 2;
                    totalCount += 2;
                    i++;
                }
            }
        }

        return totalCount > 0 && (double)validCount / totalCount > 0.9;
    }

    public static Encoding DetectEncodingFromBom(byte[] data)
    {
        if (HasUtf8Bom(data))
            return Encodings.UTF8WithBom;

        if (HasUtf16LeBom(data))
            return Encodings.UTF16WithBom;

        if (HasUtf16BeBom(data))
            return Encodings.UTF16BigEndianWithBom;

        return null!;
    }
}