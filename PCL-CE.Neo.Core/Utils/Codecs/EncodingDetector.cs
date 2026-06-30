using PCL_CE.Neo.Core.Logging;
using System;
using System.Text;

namespace PCL_CE.Neo.Core.Utils.Codecs;

public class EncodingDetector
{
    private const string ModuleName = "EncodingDetector";

    public Encoding Detect(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            LogWrapper.Debug("Empty bytes provided, returning UTF-8", ModuleName);
            return Encoding.UTF8;
        }

        var bomEncoding = Encodings.DetectBom(bytes);
        if (bomEncoding != null)
        {
            LogWrapper.Debug($"Detected BOM: {bomEncoding.EncodingName}", ModuleName);
            return bomEncoding;
        }

        var utf8Confidence = _CheckUtf8(bytes);
        var utf16LeConfidence = _CheckUtf16Le(bytes);
        var utf16BeConfidence = _CheckUtf16Be(bytes);
        var gb2312Confidence = _CheckGb2312(bytes);

        var maxConfidence = Math.Max(Math.Max(utf8Confidence, utf16LeConfidence), Math.Max(utf16BeConfidence, gb2312Confidence));

        if (maxConfidence < 0.5)
        {
            LogWrapper.Debug("Low confidence in all encodings, defaulting to UTF-8", ModuleName);
            return Encoding.UTF8;
        }

        if (utf8Confidence == maxConfidence)
        {
            LogWrapper.Debug("Detected encoding: UTF-8", ModuleName);
            return Encoding.UTF8;
        }

        if (utf16LeConfidence == maxConfidence)
        {
            LogWrapper.Debug("Detected encoding: UTF-16 LE", ModuleName);
            return Encoding.Unicode;
        }

        if (utf16BeConfidence == maxConfidence)
        {
            LogWrapper.Debug("Detected encoding: UTF-16 BE", ModuleName);
            return Encoding.BigEndianUnicode;
        }

        LogWrapper.Debug("Detected encoding: GB2312/GBK", ModuleName);
        return Encoding.GetEncoding("GB2312");
    }

    private double _CheckUtf8(byte[] bytes)
    {
        int valid = 0;
        int invalid = 0;
        int i = 0;

        while (i < bytes.Length)
        {
            byte b = bytes[i];

            if (b < 0x80)
            {
                valid++;
                i++;
            }
            else if (b >= 0xC2 && b <= 0xDF)
            {
                if (i + 1 < bytes.Length && bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
                {
                    valid++;
                    i += 2;
                }
                else
                {
                    invalid++;
                    i++;
                }
            }
            else if (b >= 0xE0 && b <= 0xEF)
            {
                if (i + 2 < bytes.Length &&
                    bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF &&
                    bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
                {
                    valid++;
                    i += 3;
                }
                else
                {
                    invalid++;
                    i++;
                }
            }
            else if (b >= 0xF0 && b <= 0xF4)
            {
                if (i + 3 < bytes.Length &&
                    bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF &&
                    bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF &&
                    bytes[i + 3] >= 0x80 && bytes[i + 3] <= 0xBF)
                {
                    valid++;
                    i += 4;
                }
                else
                {
                    invalid++;
                    i++;
                }
            }
            else
            {
                invalid++;
                i++;
            }
        }

        return invalid == 0 ? 1.0 : (double)valid / (valid + invalid);
    }

    private double _CheckUtf16Le(byte[] bytes)
    {
        if (bytes.Length < 2) return 0;

        int nullCount = 0;
        int validCount = 0;

        for (int i = 0; i < bytes.Length; i += 2)
        {
            if (i + 1 >= bytes.Length) break;

            byte low = bytes[i];
            byte high = bytes[i + 1];

            if (high == 0)
            {
                if (low >= 0x20 && low <= 0x7E)
                    validCount++;
                else if (low == 0x0A || low == 0x0D)
                    validCount++;
                else
                    nullCount++;
            }
            else if (high >= 0xD8 && high <= 0xDF)
            {
                if (i + 3 < bytes.Length && bytes[i + 3] >= 0xDC && bytes[i + 3] <= 0xDF)
                    validCount += 2;
            }
            else
            {
                validCount++;
            }
        }

        double total = nullCount + validCount;
        return total == 0 ? 0 : (double)validCount / total;
    }

    private double _CheckUtf16Be(byte[] bytes)
    {
        if (bytes.Length < 2) return 0;

        int nullCount = 0;
        int validCount = 0;

        for (int i = 0; i < bytes.Length; i += 2)
        {
            if (i + 1 >= bytes.Length) break;

            byte high = bytes[i];
            byte low = bytes[i + 1];

            if (high == 0)
            {
                if (low >= 0x20 && low <= 0x7E)
                    validCount++;
                else if (low == 0x0A || low == 0x0D)
                    validCount++;
                else
                    nullCount++;
            }
            else if (high >= 0xD8 && high <= 0xDF)
            {
                if (i + 3 < bytes.Length && bytes[i + 2] >= 0xDC && bytes[i + 2] <= 0xDF)
                    validCount += 2;
            }
            else
            {
                validCount++;
            }
        }

        double total = nullCount + validCount;
        return total == 0 ? 0 : (double)validCount / total;
    }

    private double _CheckGb2312(byte[] bytes)
    {
        int valid = 0;
        int invalid = 0;
        int i = 0;

        while (i < bytes.Length)
        {
            byte b = bytes[i];

            if (b < 0x80)
            {
                valid++;
                i++;
            }
            else if (b >= 0xA1 && b <= 0xF7)
            {
                if (i + 1 < bytes.Length)
                {
                    byte b2 = bytes[i + 1];
                    if ((b2 >= 0xA1 && b2 <= 0xFE))
                    {
                        valid++;
                        i += 2;
                    }
                    else
                    {
                        invalid++;
                        i++;
                    }
                }
                else
                {
                    invalid++;
                    i++;
                }
            }
            else
            {
                invalid++;
                i++;
            }
        }

        return invalid == 0 ? 1.0 : (double)valid / (valid + invalid);
    }
}