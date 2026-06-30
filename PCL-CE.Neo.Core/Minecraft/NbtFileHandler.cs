using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft;

public class NbtFileHandler
{
    private readonly ILogger<NbtFileHandler> _logger;

    public NbtFileHandler(ILogger<NbtFileHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<NbtCompound?> ReadFileAsync(string filePath, bool compressed = true)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogError("NBT file not found: {FilePath}", filePath);
            return null;
        }

        try
        {
            using var stream = File.OpenRead(filePath);
            return await ReadStreamAsync(stream, compressed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read NBT file: {FilePath}", filePath);
            return null;
        }
    }

    public NbtCompound? ReadFile(string filePath, bool compressed = true)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogError("NBT file not found: {FilePath}", filePath);
            return null;
        }

        try
        {
            using var stream = File.OpenRead(filePath);
            return ReadStream(stream, compressed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read NBT file: {FilePath}", filePath);
            return null;
        }
    }

    public async Task<NbtCompound?> ReadStreamAsync(Stream stream, bool compressed = true)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        try
        {
            var reader = compressed 
                ? new BinaryReader(new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress))
                : new BinaryReader(stream);

            var rootTagType = (NbtTagType)reader.ReadByte();
            if (rootTagType == NbtTagType.End)
                return null;

            var rootName = reader.ReadString();
            var root = await ReadTagAsync(reader, rootTagType);

            reader.Close();

            if (root is NbtCompound compound)
                return compound;

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read NBT stream");
            return null;
        }
    }

    public NbtCompound? ReadStream(Stream stream, bool compressed = true)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        try
        {
            var reader = compressed 
                ? new BinaryReader(new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress))
                : new BinaryReader(stream);

            var rootTagType = (NbtTagType)reader.ReadByte();
            if (rootTagType == NbtTagType.End)
                return null;

            var rootName = reader.ReadString();
            var root = ReadTag(reader, rootTagType);

            reader.Close();

            if (root is NbtCompound compound)
                return compound;

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read NBT stream");
            return null;
        }
    }

    public async Task<bool> WriteFileAsync(string filePath, NbtCompound compound, bool compressed = true)
    {
        if (compound == null)
            throw new ArgumentNullException(nameof(compound));

        try
        {
            var targetDir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            using var stream = File.Create(filePath);
            await WriteStreamAsync(stream, compound, compressed);

            _logger.LogInformation("Successfully wrote NBT file: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write NBT file: {FilePath}", filePath);
            return false;
        }
    }

    public bool WriteFile(string filePath, NbtCompound compound, bool compressed = true)
    {
        if (compound == null)
            throw new ArgumentNullException(nameof(compound));

        try
        {
            var targetDir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            using var stream = File.Create(filePath);
            WriteStream(stream, compound, compressed);

            _logger.LogInformation("Successfully wrote NBT file: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write NBT file: {FilePath}", filePath);
            return false;
        }
    }

    public async Task WriteStreamAsync(Stream stream, NbtCompound compound, bool compressed = true)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        if (compound == null)
            throw new ArgumentNullException(nameof(compound));

        var writer = compressed 
            ? new BinaryWriter(new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Compress))
            : new BinaryWriter(stream);

        writer.Write((byte)NbtTagType.Compound);
        writer.Write(string.Empty);
        await WriteTagAsync(writer, compound);

        writer.Close();
    }

    public void WriteStream(Stream stream, NbtCompound compound, bool compressed = true)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        if (compound == null)
            throw new ArgumentNullException(nameof(compound));

        var writer = compressed 
            ? new BinaryWriter(new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Compress))
            : new BinaryWriter(stream);

        writer.Write((byte)NbtTagType.Compound);
        writer.Write(string.Empty);
        WriteTag(writer, compound);

        writer.Close();
    }

    private Task<NbtTag> ReadTagAsync(BinaryReader reader, NbtTagType type)
    {
        return Task.FromResult(ReadTag(reader, type));
    }

    private NbtTag ReadTag(BinaryReader reader, NbtTagType type)
    {
        return type switch
        {
            NbtTagType.Byte => new NbtByte(reader.ReadByte()),
            NbtTagType.Short => new NbtShort(reader.ReadInt16()),
            NbtTagType.Int => new NbtInt(reader.ReadInt32()),
            NbtTagType.Long => new NbtLong(reader.ReadInt64()),
            NbtTagType.Float => new NbtFloat(reader.ReadSingle()),
            NbtTagType.Double => new NbtDouble(reader.ReadDouble()),
            NbtTagType.ByteArray => ReadByteArray(reader),
            NbtTagType.String => new NbtString(reader.ReadString()),
            NbtTagType.List => ReadList(reader),
            NbtTagType.Compound => ReadCompound(reader),
            NbtTagType.IntArray => ReadIntArray(reader),
            NbtTagType.LongArray => ReadLongArray(reader),
            NbtTagType.End => new NbtEnd(),
            _ => throw new NotSupportedException($"Unknown NBT tag type: {type}")
        };
    }

    private Task<NbtByteArray> ReadByteArrayAsync(BinaryReader reader)
    {
        return Task.FromResult(ReadByteArray(reader));
    }

    private NbtByteArray ReadByteArray(BinaryReader reader)
    {
        var length = reader.ReadInt32();
        var data = reader.ReadBytes(length);
        return new NbtByteArray(data);
    }

    private Task<NbtList> ReadListAsync(BinaryReader reader)
    {
        return Task.FromResult(ReadList(reader));
    }

    private NbtList ReadList(BinaryReader reader)
    {
        var elementType = (NbtTagType)reader.ReadByte();
        var length = reader.ReadInt32();
        var list = new NbtList(elementType);

        for (int i = 0; i < length; i++)
        {
            var tag = ReadTag(reader, elementType);
            list.Add(tag);
        }

        return list;
    }

    private Task<NbtCompound> ReadCompoundAsync(BinaryReader reader)
    {
        return Task.FromResult(ReadCompound(reader));
    }

    private NbtCompound ReadCompound(BinaryReader reader)
    {
        var compound = new NbtCompound();

        while (true)
        {
            var type = (NbtTagType)reader.ReadByte();
            if (type == NbtTagType.End)
                break;

            var name = reader.ReadString();
            var tag = ReadTag(reader, type);
            compound[name] = tag;
        }

        return compound;
    }

    private Task<NbtIntArray> ReadIntArrayAsync(BinaryReader reader)
    {
        return Task.FromResult(ReadIntArray(reader));
    }

    private NbtIntArray ReadIntArray(BinaryReader reader)
    {
        var length = reader.ReadInt32();
        var data = new int[length];
        for (int i = 0; i < length; i++)
            data[i] = reader.ReadInt32();
        return new NbtIntArray(data);
    }

    private Task<NbtLongArray> ReadLongArrayAsync(BinaryReader reader)
    {
        return Task.FromResult(ReadLongArray(reader));
    }

    private NbtLongArray ReadLongArray(BinaryReader reader)
    {
        var length = reader.ReadInt32();
        var data = new long[length];
        for (int i = 0; i < length; i++)
            data[i] = reader.ReadInt64();
        return new NbtLongArray(data);
    }

    private Task WriteTagAsync(BinaryWriter writer, NbtTag tag)
    {
        WriteTag(writer, tag);
        return Task.CompletedTask;
    }

    private void WriteTag(BinaryWriter writer, NbtTag tag)
    {
        switch (tag)
        {
            case NbtByte byteTag:
                writer.Write(byteTag.Value);
                break;
            case NbtShort shortTag:
                writer.Write(shortTag.Value);
                break;
            case NbtInt intTag:
                writer.Write(intTag.Value);
                break;
            case NbtLong longTag:
                writer.Write(longTag.Value);
                break;
            case NbtFloat floatTag:
                writer.Write(floatTag.Value);
                break;
            case NbtDouble doubleTag:
                writer.Write(doubleTag.Value);
                break;
            case NbtByteArray byteArrayTag:
                writer.Write(byteArrayTag.Value.Length);
                writer.Write(byteArrayTag.Value);
                break;
            case NbtString stringTag:
                writer.Write(stringTag.Value);
                break;
            case NbtList listTag:
                writer.Write((byte)listTag.ElementType);
                writer.Write(listTag.Count);
                foreach (var item in listTag)
                    WriteTag(writer, item);
                break;
            case NbtCompound compoundTag:
                foreach (var kvp in compoundTag)
                {
                    writer.Write((byte)kvp.Value.Type);
                    writer.Write(kvp.Key);
                    WriteTag(writer, kvp.Value);
                }
                writer.Write((byte)NbtTagType.End);
                break;
            case NbtIntArray intArrayTag:
                writer.Write(intArrayTag.Value.Length);
                foreach (var item in intArrayTag.Value)
                    writer.Write(item);
                break;
            case NbtLongArray longArrayTag:
                writer.Write(longArrayTag.Value.Length);
                foreach (var item in longArrayTag.Value)
                    writer.Write(item);
                break;
            case NbtEnd:
                writer.Write((byte)NbtTagType.End);
                break;
        }
    }
}

public enum NbtTagType
{
    End = 0,
    Byte = 1,
    Short = 2,
    Int = 3,
    Long = 4,
    Float = 5,
    Double = 6,
    ByteArray = 7,
    String = 8,
    List = 9,
    Compound = 10,
    IntArray = 11,
    LongArray = 12
}

public abstract class NbtTag
{
    public abstract NbtTagType Type { get; }
}

public class NbtByte : NbtTag
{
    public byte Value { get; }
    public override NbtTagType Type => NbtTagType.Byte;

    public NbtByte(byte value) => Value = value;
}

public class NbtShort : NbtTag
{
    public short Value { get; }
    public override NbtTagType Type => NbtTagType.Short;

    public NbtShort(short value) => Value = value;
}

public class NbtInt : NbtTag
{
    public int Value { get; }
    public override NbtTagType Type => NbtTagType.Int;

    public NbtInt(int value) => Value = value;
}

public class NbtLong : NbtTag
{
    public long Value { get; }
    public override NbtTagType Type => NbtTagType.Long;

    public NbtLong(long value) => Value = value;
}

public class NbtFloat : NbtTag
{
    public float Value { get; }
    public override NbtTagType Type => NbtTagType.Float;

    public NbtFloat(float value) => Value = value;
}

public class NbtDouble : NbtTag
{
    public double Value { get; }
    public override NbtTagType Type => NbtTagType.Double;

    public NbtDouble(double value) => Value = value;
}

public class NbtByteArray : NbtTag
{
    public byte[] Value { get; }
    public override NbtTagType Type => NbtTagType.ByteArray;

    public NbtByteArray(byte[] value) => Value = value;
}

public class NbtString : NbtTag
{
    public string Value { get; }
    public override NbtTagType Type => NbtTagType.String;

    public NbtString(string value) => Value = value;
}

public class NbtList : NbtTag, IList<NbtTag>
{
    private readonly List<NbtTag> _items = new();
    public NbtTagType ElementType { get; }
    public override NbtTagType Type => NbtTagType.List;

    public NbtList(NbtTagType elementType) => ElementType = elementType;

    public int Count => _items.Count;
    public bool IsReadOnly => false;

    public NbtTag this[int index]
    {
        get => _items[index];
        set
        {
            if (value != null && value.Type != ElementType)
                throw new ArgumentException($"Expected tag type {ElementType}, got {value.Type}");
            _items[index] = value;
        }
    }

    public void Add(NbtTag item)
    {
        if (item != null && item.Type != ElementType)
            throw new ArgumentException($"Expected tag type {ElementType}, got {item.Type}");
        _items.Add(item);
    }

    public void Clear() => _items.Clear();
    public bool Contains(NbtTag item) => _items.Contains(item);
    public void CopyTo(NbtTag[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
    public IEnumerator<NbtTag> GetEnumerator() => _items.GetEnumerator();
    public int IndexOf(NbtTag item) => _items.IndexOf(item);
    public void Insert(int index, NbtTag item)
    {
        if (item != null && item.Type != ElementType)
            throw new ArgumentException($"Expected tag type {ElementType}, got {item.Type}");
        _items.Insert(index, item);
    }
    public bool Remove(NbtTag item) => _items.Remove(item);
    public void RemoveAt(int index) => _items.RemoveAt(index);
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

public class NbtCompound : NbtTag, IDictionary<string, NbtTag>
{
    private readonly Dictionary<string, NbtTag> _items = new();
    public override NbtTagType Type => NbtTagType.Compound;

    public int Count => _items.Count;
    public bool IsReadOnly => false;
    public ICollection<string> Keys => _items.Keys;
    public ICollection<NbtTag> Values => _items.Values;

    public NbtTag this[string key]
    {
        get => _items[key];
        set => _items[key] = value;
    }

    public void Add(string key, NbtTag value) => _items.Add(key, value);
    public void Add(KeyValuePair<string, NbtTag> item) => _items.Add(item.Key, item.Value);
    public void Clear() => _items.Clear();
    public bool Contains(KeyValuePair<string, NbtTag> item) => _items.Contains(item);
    public bool ContainsKey(string key) => _items.ContainsKey(key);
    public void CopyTo(KeyValuePair<string, NbtTag>[] array, int arrayIndex) => ((IDictionary<string, NbtTag>)_items).CopyTo(array, arrayIndex);
    public IEnumerator<KeyValuePair<string, NbtTag>> GetEnumerator() => _items.GetEnumerator();
    public bool Remove(string key) => _items.Remove(key);
    public bool Remove(KeyValuePair<string, NbtTag> item) => _items.Remove(item.Key);
    public bool TryGetValue(string key, out NbtTag value) => _items.TryGetValue(key, out value);
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

public class NbtIntArray : NbtTag
{
    public int[] Value { get; }
    public override NbtTagType Type => NbtTagType.IntArray;

    public NbtIntArray(int[] value) => Value = value;
}

public class NbtLongArray : NbtTag
{
    public long[] Value { get; }
    public override NbtTagType Type => NbtTagType.LongArray;

    public NbtLongArray(long[] value) => Value = value;
}

public class NbtEnd : NbtTag
{
    public override NbtTagType Type => NbtTagType.End;
}