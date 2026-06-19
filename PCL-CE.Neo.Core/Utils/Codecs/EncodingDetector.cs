using System.IO;
using System.Linq;
using System.Text;

namespace PCL_CE.Neo.Core.Utils.Codecs;

public static class EncodingDetector
{
    public static Encoding DetectEncoding(Stream stream, bool readFromBegin = false)
    {
        if (!stream.CanRead)
            throw new ArgumentException("流必须支持读操作");
        if (!stream.CanSeek)
            throw new ArgumentException("流必须支持 Seek 操作");

        var originalPosition = stream.Position;
        if (readFromBegin) stream.Seek(0, SeekOrigin.Begin);

        try
        {
            return _DetectByBom(stream, originalPosition) ?? _DetectWithoutBOM(stream, originalPosition) ?? Encoding.Default;
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    public static Encoding DetectEncoding(byte[] bytes)
    {
        return DetectEncoding(new MemoryStream(bytes), true);
    }

    private static Encoding? _DetectByBom(Stream stream, long originalPosition)
    {
        stream.Position = originalPosition;
        var readableLength = stream.Length - stream.Position;
        var sampleLength = Math.Min(readableLength, 4);
        var buffer = new byte[sampleLength];
        var actualRead = stream.Read(buffer, 0, buffer.Length);
        if (actualRead != sampleLength) throw new Exception("无法获取样本长度");

        if (sampleLength >= 3 && buffer is [0xef, 0xbb, 0xbf])
            return Encoding.UTF8;

        if (sampleLength >= 2)
        {
            if (buffer is [0xfe, 0xff])
                return Encoding.BigEndianUnicode;
            if (buffer is [0xff, 0xfe])
            {
                if (sampleLength >= 4 && buffer is [_, _, 0x00, 0x00])
                    return Encoding.UTF32;
                return Encoding.Unicode;
            }
        }

        if (sampleLength >= 4)
        {
            if (buffer is [0x00, 0x00, 0xfe, 0xff])
                return Encoding.GetEncoding("utf-32BE");
            if (buffer is [0xff, 0xfe, 0x00, 0x00])
                return Encoding.UTF32;
        }

        return null;
    }

    private static Encoding? _DetectWithoutBOM(Stream stream, long originalPosition)
    {
        return _IsValidUtf8(stream, originalPosition) ? Encoding.UTF8 : null;
    }

    private static bool _IsValidUtf8(Stream stream, long originalPosition)
    {
        const int sampleSize = 1024;
        var buffer = new byte[sampleSize];
        stream.Position = originalPosition;

        try
        {
            var decoded = Encoding.UTF8.GetString(buffer);
            var roundTrip = Encoding.UTF8.GetBytes(decoded);
            return roundTrip.SequenceEqual(buffer);
        }
        catch
        {
            return false;
        }
    }
}