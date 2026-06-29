using System;
using System.IO;

namespace PCL_CE.Neo.Core.Utils;

public static class VarIntHelper
{
    public static int ReadVarInt(Stream stream)
    {
        try
        {
            int value = 0;
            int position = 0;
            byte currentByte;

            do
            {
                currentByte = (byte)stream.ReadByte();
                value |= (currentByte & 0x7F) << position;
                position += 7;

                if (position >= 32)
                    throw new OverflowException("VarInt is too big");
            }
            while ((currentByte & 0x80) != 0);

            return value;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to read VarInt");
            throw;
        }
    }

    public static int ReadVarInt(BinaryReader reader)
    {
        try
        {
            int value = 0;
            int position = 0;
            byte currentByte;

            do
            {
                currentByte = reader.ReadByte();
                value |= (currentByte & 0x7F) << position;
                position += 7;

                if (position >= 32)
                    throw new OverflowException("VarInt is too big");
            }
            while ((currentByte & 0x80) != 0);

            return value;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to read VarInt from BinaryReader");
            throw;
        }
    }

    public static long ReadVarLong(Stream stream)
    {
        try
        {
            long value = 0;
            int position = 0;
            byte currentByte;

            do
            {
                currentByte = (byte)stream.ReadByte();
                value |= (long)(currentByte & 0x7F) << position;
                position += 7;

                if (position >= 64)
                    throw new OverflowException("VarLong is too big");
            }
            while ((currentByte & 0x80) != 0);

            return value;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to read VarLong");
            throw;
        }
    }

    public static byte[] WriteVarInt(int value)
    {
        try
        {
            var buffer = new MemoryStream();

            do
            {
                byte currentByte = (byte)(value & 0x7F);
                value >>= 7;

                if (value != 0)
                    currentByte |= 0x80;

                buffer.WriteByte(currentByte);
            }
            while (value != 0);

            return buffer.ToArray();
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to write VarInt");
            throw;
        }
    }

    public static void WriteVarInt(Stream stream, int value)
    {
        try
        {
            do
            {
                byte currentByte = (byte)(value & 0x7F);
                value >>= 7;

                if (value != 0)
                    currentByte |= 0x80;

                stream.WriteByte(currentByte);
            }
            while (value != 0);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to write VarInt to stream");
            throw;
        }
    }

    public static byte[] WriteVarLong(long value)
    {
        try
        {
            var buffer = new MemoryStream();

            do
            {
                byte currentByte = (byte)(value & 0x7F);
                value >>= 7;

                if (value != 0)
                    currentByte |= 0x80;

                buffer.WriteByte(currentByte);
            }
            while (value != 0);

            return buffer.ToArray();
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to write VarLong");
            throw;
        }
    }

    public static void WriteVarLong(Stream stream, long value)
    {
        try
        {
            do
            {
                byte currentByte = (byte)(value & 0x7F);
                value >>= 7;

                if (value != 0)
                    currentByte |= 0x80;

                stream.WriteByte(currentByte);
            }
            while (value != 0);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to write VarLong to stream");
            throw;
        }
    }

    public static int GetVarIntSize(int value)
    {
        if (value < 0)
            return 5;

        if (value < 0x80)
            return 1;

        if (value < 0x4000)
            return 2;

        if (value < 0x200000)
            return 3;

        if (value < 0x10000000)
            return 4;

        return 5;
    }

    public static int GetVarLongSize(long value)
    {
        if (value < 0)
            return 10;

        if (value < 0x80)
            return 1;

        if (value < 0x4000)
            return 2;

        if (value < 0x200000)
            return 3;

        if (value < 0x10000000)
            return 4;

        if (value < 0x800000000)
            return 5;

        if (value < 0x40000000000)
            return 6;

        if (value < 0x2000000000000)
            return 7;

        if (value < 0x100000000000000)
            return 8;

        if (value < 0x8000000000000000)
            return 9;

        return 10;
    }
}