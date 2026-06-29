using System;
using System.IO;
using System.Text;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.Minecraft;

public static class NbtFileHandler
{
    private const byte TagEnd = 0;
    private const byte TagByte = 1;
    private const byte TagShort = 2;
    private const byte TagInt = 3;
    private const byte TagLong = 4;
    private const byte TagFloat = 5;
    private const byte TagDouble = 6;
    private const byte TagByteArray = 7;
    private const byte TagString = 8;
    private const byte TagList = 9;
    private const byte TagCompound = 10;
    private const byte TagIntArray = 11;
    private const byte TagLongArray = 12;

    public static bool IsValidNbtFile(string filePath)
    {
        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return IsValidNbtStream(fs);
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, $"Failed to validate NBT file: {filePath}");
            return false;
        }
    }

    public static bool IsValidNbtStream(Stream stream)
    {
        try
        {
            var originalPosition = stream.Position;
            var reader = new BinaryReader(stream);

            var tagType = reader.ReadByte();
            if (tagType != TagCompound)
                return false;

            var nameLength = ReadShort(reader);
            if (nameLength < 0 || nameLength > 32767)
                return false;

            var nameBytes = reader.ReadBytes(nameLength);
            if (nameBytes.Length != nameLength)
                return false;

            stream.Position = originalPosition;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static NbtCompound? ReadNbtFile(string filePath)
    {
        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return ReadNbtStream(fs);
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, $"Failed to read NBT file: {filePath}");
            return null;
        }
    }

    public static NbtCompound? ReadNbtStream(Stream stream)
    {
        try
        {
            var reader = new BinaryReader(stream);
            return ReadTagCompound(reader);
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to read NBT stream");
            return null;
        }
    }

    private static NbtTag ReadTag(BinaryReader reader)
    {
        var tagType = reader.ReadByte();
        if (tagType == TagEnd)
            return new NbtEnd();

        var name = ReadString(reader);
        return tagType switch
        {
            TagByte => new NbtByte(name, reader.ReadByte()),
            TagShort => new NbtShort(name, ReadShort(reader)),
            TagInt => new NbtInt(name, ReadInt(reader)),
            TagLong => new NbtLong(name, ReadLong(reader)),
            TagFloat => new NbtFloat(name, reader.ReadSingle()),
            TagDouble => new NbtDouble(name, reader.ReadDouble()),
            TagByteArray => new NbtByteArray(name, ReadByteArray(reader)),
            TagString => new NbtString(name, ReadString(reader)),
            TagList => new NbtList(name, ReadTagList(reader)),
            TagCompound => new NbtCompound(name, ReadTagCompound(reader)),
            TagIntArray => new NbtIntArray(name, ReadIntArray(reader)),
            TagLongArray => new NbtLongArray(name, ReadLongArray(reader)),
            _ => throw new NotSupportedException($"Unknown tag type: {tagType}")
        };
    }

    private static Dictionary<string, NbtTag> ReadTagCompound(BinaryReader reader)
    {
        var tags = new Dictionary<string, NbtTag>();

        while (true)
        {
            var tag = ReadTag(reader);
            if (tag is NbtEnd)
                break;
            tags[tag.Name ?? ""] = tag;
        }

        return tags;
    }

    private static List<NbtTag> ReadTagList(BinaryReader reader)
    {
        var elementType = reader.ReadByte();
        var count = ReadInt(reader);
        var tags = new List<NbtTag>(count);

        for (int i = 0; i < count; i++)
        {
            var tag = elementType switch
            {
                TagByte => new NbtByte(null, reader.ReadByte()),
                TagShort => new NbtShort(null, ReadShort(reader)),
                TagInt => new NbtInt(null, ReadInt(reader)),
                TagLong => new NbtLong(null, ReadLong(reader)),
                TagFloat => new NbtFloat(null, reader.ReadSingle()),
                TagDouble => new NbtDouble(null, reader.ReadDouble()),
                TagByteArray => new NbtByteArray(null, ReadByteArray(reader)),
                TagString => new NbtString(null, ReadString(reader)),
                TagCompound => new NbtCompound(null, ReadTagCompound(reader)),
                TagIntArray => new NbtIntArray(null, ReadIntArray(reader)),
                TagLongArray => new NbtLongArray(null, ReadLongArray(reader)),
                _ => throw new NotSupportedException($"Unknown list element type: {elementType}")
            };
            tags.Add(tag);
        }

        return tags;
    }

    private static short ReadShort(BinaryReader reader)
    {
        var bytes = reader.ReadBytes(2);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return BitConverter.ToInt16(bytes, 0);
    }

    private static int ReadInt(BinaryReader reader)
    {
        var bytes = reader.ReadBytes(4);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return BitConverter.ToInt32(bytes, 0);
    }

    private static long ReadLong(BinaryReader reader)
    {
        var bytes = reader.ReadBytes(8);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return BitConverter.ToInt64(bytes, 0);
    }

    private static string ReadString(BinaryReader reader)
    {
        var length = ReadShort(reader);
        var bytes = reader.ReadBytes(length);
        return Encoding.UTF8.GetString(bytes);
    }

    private static byte[] ReadByteArray(BinaryReader reader)
    {
        var length = ReadInt(reader);
        return reader.ReadBytes(length);
    }

    private static int[] ReadIntArray(BinaryReader reader)
    {
        var length = ReadInt(reader);
        var array = new int[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = ReadInt(reader);
        }
        return array;
    }

    private static long[] ReadLongArray(BinaryReader reader)
    {
        var length = ReadInt(reader);
        var array = new long[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = ReadLong(reader);
        }
        return array;
    }
}

public abstract class NbtTag
{
    public string? Name { get; }
    public abstract byte TagType { get; }

    protected NbtTag(string? name)
    {
        Name = name;
    }
}

public class NbtEnd : NbtTag
{
    public NbtEnd() : base(null) { }
    public override byte TagType => 0;
}

public class NbtByte : NbtTag
{
    public byte Value { get; }
    public NbtByte(string? name, byte value) : base(name) => Value = value;
    public override byte TagType => 1;
}

public class NbtShort : NbtTag
{
    public short Value { get; }
    public NbtShort(string? name, short value) : base(name) => Value = value;
    public override byte TagType => 2;
}

public class NbtInt : NbtTag
{
    public int Value { get; }
    public NbtInt(string? name, int value) : base(name) => Value = value;
    public override byte TagType => 3;
}

public class NbtLong : NbtTag
{
    public long Value { get; }
    public NbtLong(string? name, long value) : base(name) => Value = value;
    public override byte TagType => 4;
}

public class NbtFloat : NbtTag
{
    public float Value { get; }
    public NbtFloat(string? name, float value) : base(name) => Value = value;
    public override byte TagType => 5;
}

public class NbtDouble : NbtTag
{
    public double Value { get; }
    public NbtDouble(string? name, double value) : base(name) => Value = value;
    public override byte TagType => 6;
}

public class NbtByteArray : NbtTag
{
    public byte[] Value { get; }
    public NbtByteArray(string? name, byte[] value) : base(name) => Value = value;
    public override byte TagType => 7;
}

public class NbtString : NbtTag
{
    public string Value { get; }
    public NbtString(string? name, string value) : base(name) => Value = value;
    public override byte TagType => 8;
}

public class NbtList : NbtTag
{
    public List<NbtTag> Value { get; }
    public NbtList(string? name, List<NbtTag> value) : base(name) => Value = value;
    public override byte TagType => 9;
}

public class NbtCompound : NbtTag
{
    public Dictionary<string, NbtTag> Value { get; }
    public NbtCompound(string? name, Dictionary<string, NbtTag> value) : base(name) => Value = value;
    public override byte TagType => 10;
}

public class NbtIntArray : NbtTag
{
    public int[] Value { get; }
    public NbtIntArray(string? name, int[] value) : base(name) => Value = value;
    public override byte TagType => 11;
}

public class NbtLongArray : NbtTag
{
    public long[] Value { get; }
    public NbtLongArray(string? name, long[] value) : base(name) => Value = value;
    public override byte TagType => 12;
}