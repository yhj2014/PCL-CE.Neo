using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Link.McPing;

public static class VarIntHelper
{
    public static byte[] Encode(ulong value)
    {
        using var stream = new MemoryStream();
        do
        {
            var temp = (byte)(value & 0x7F);
            value >>= 7;
            if (value != 0)
                temp |= 0x80;
            stream.WriteByte(temp);
        } while (value != 0);

        return stream.ToArray();
    }

    public static byte[] Encode(uint value) => Encode((ulong)value);

    public static ulong Decode(byte[] bytes, out int readLength)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        ulong result = 0;
        var shift = 0;
        var bytesRead = 0;
        const int maxBytes = 10;

        foreach (var b in bytes)
        {
            if (bytesRead >= maxBytes)
                throw new FormatException("VarInt exceeds maximum length");

            result |= (ulong)(b & 0x7F) << shift;
            bytesRead++;

            if ((b & 0x80) == 0)
            {
                readLength = bytesRead;
                return result;
            }

            shift += 7;
        }

        throw new FormatException("Incomplete VarInt encoding");
    }

    public static uint DecodeUInt(byte[] bytes, out int readLength)
    {
        var result = Decode(bytes, out readLength);
        if (result > uint.MaxValue)
            throw new OverflowException("Decoded value exceeds UInt32 range");
        return (uint)result;
    }

    public static async Task<ulong> ReadFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ulong result = 0;
        var shift = 0;
        var bytesRead = 0;
        const int maxBytes = 10;
        var buffer = new byte[1];
        while (true)
        {
            var readLength = await stream.ReadAsync(buffer, 0, 1, cancellationToken);
            if (readLength == 0)
                throw new EndOfStreamException();

            var b = buffer[0];
            bytesRead++;

            if (bytesRead > maxBytes)
                throw new FormatException("VarInt exceeds maximum length");

            result |= (ulong)(b & 0x7F) << shift;

            if ((b & 0x80) == 0)
                return result;

            shift += 7;
        }
    }

    public static async Task<uint> ReadUIntFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var result = await ReadFromStreamAsync(stream, cancellationToken);
        if (result > uint.MaxValue)
            throw new OverflowException("Decoded value exceeds UInt32 range");
        return (uint)result;
    }

    public static long DecodeZigZag(ulong value) => (long)(value >> 1) ^ -(long)(value & 1);

    public static ulong EncodeZigZag(ulong value) => ((value << 1) ^ (value >> 63));
}