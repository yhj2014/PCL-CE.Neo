using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils;

public static class VarIntHelper
{
    public static byte[] Encode(uint value)
    {
        var result = new List<byte>();
        do
        {
            byte temp = (byte)(value & 0x7F);
            value >>= 7;
            if (value != 0)
            {
                temp |= 0x80;
            }
            result.Add(temp);
        } while (value != 0);
        return result.ToArray();
    }

    public static uint Read(ReadOnlySpan<byte> span, ref int offset)
    {
        uint result = 0;
        int shift = 0;
        byte b;
        do
        {
            if (offset >= span.Length)
                throw new EndOfStreamException("VarInt 读取超出范围");
            b = span[offset++];
            result |= (uint)(b & 0x7F) << shift;
            shift += 7;
        } while ((b & 0x80) != 0);
        return result;
    }

    public static async Task<uint> ReadFromStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        uint result = 0;
        int shift = 0;
        byte b;
        do
        {
            var buffer = new byte[1];
            var read = await stream.ReadAsync(buffer, 0, 1, cancellationToken);
            if (read == 0)
                throw new EndOfStreamException("VarInt 读取遇到流末尾");
            b = buffer[0];
            result |= (uint)(b & 0x7F) << shift;
            shift += 7;
            if (shift >= 32)
                throw new OverflowException("VarInt 超出32位范围");
        } while ((b & 0x80) != 0);
        return result;
    }
}