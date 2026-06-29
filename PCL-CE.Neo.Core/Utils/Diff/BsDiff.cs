using System;

namespace PCL_CE.Neo.Core.Utils.Diff;

public class BsDiff : IBinaryDiff
{
    public byte[] CreatePatch(byte[] oldData, byte[] newData)
    {
        try
        {
            if (oldData == null)
                throw new ArgumentNullException(nameof(oldData));
            if (newData == null)
                throw new ArgumentNullException(nameof(newData));

            var result = new System.IO.MemoryStream();
            var writer = new System.IO.BinaryWriter(result);

            writer.Write(oldData.Length);
            writer.Write(newData.Length);

            var diffs = ComputeDiffs(oldData, newData);
            
            foreach (var diff in diffs)
            {
                writer.Write((byte)diff.Type);
                
                if (diff.Type == DiffType.Add)
                {
                    writer.Write(diff.Data.Length);
                    writer.Write(diff.Data);
                }
                else if (diff.Type == DiffType.Copy)
                {
                    writer.Write(diff.Offset);
                    writer.Write(diff.Length);
                }
                else if (diff.Type == DiffType.Remove)
                {
                    writer.Write(diff.Length);
                }
            }

            return result.ToArray();
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to create BsDiff patch");
            throw;
        }
    }

    public byte[] ApplyPatch(byte[] oldData, byte[] patch)
    {
        try
        {
            if (oldData == null)
                throw new ArgumentNullException(nameof(oldData));
            if (patch == null)
                throw new ArgumentNullException(nameof(patch));

            var reader = new System.IO.BinaryReader(new System.IO.MemoryStream(patch));
            var result = new System.IO.MemoryStream();

            var oldLength = reader.ReadInt32();
            var newLength = reader.ReadInt32();

            if (oldLength != oldData.Length)
                throw new InvalidOperationException("Old data length mismatch");

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var type = (DiffType)reader.ReadByte();

                switch (type)
                {
                    case DiffType.Add:
                        var dataLength = reader.ReadInt32();
                        var data = reader.ReadBytes(dataLength);
                        result.Write(data, 0, data.Length);
                        break;

                    case DiffType.Copy:
                        var offset = reader.ReadInt32();
                        var length = reader.ReadInt32();
                        result.Write(oldData, offset, length);
                        break;

                    case DiffType.Remove:
                        var removeLength = reader.ReadInt32();
                        break;
                }
            }

            return result.ToArray();
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to apply BsDiff patch");
            throw;
        }
    }

    private List<DiffItem> ComputeDiffs(byte[] oldData, byte[] newData)
    {
        var diffs = new List<DiffItem>();
        var oldIndex = 0;
        var newIndex = 0;

        while (newIndex < newData.Length)
        {
            if (oldIndex < oldData.Length && oldData[oldIndex] == newData[newIndex])
            {
                var matchLength = 0;
                while (oldIndex + matchLength < oldData.Length && 
                       newIndex + matchLength < newData.Length && 
                       oldData[oldIndex + matchLength] == newData[newIndex + matchLength])
                {
                    matchLength++;
                }

                diffs.Add(new DiffItem(DiffType.Copy, oldIndex, matchLength));
                oldIndex += matchLength;
                newIndex += matchLength;
            }
            else
            {
                var addLength = 0;
                while (newIndex + addLength < newData.Length && 
                       (oldIndex + addLength >= oldData.Length || 
                        oldData[oldIndex + addLength] != newData[newIndex + addLength]))
                {
                    addLength++;
                }

                var addedData = new byte[addLength];
                Array.Copy(newData, newIndex, addedData, 0, addLength);
                diffs.Add(new DiffItem(DiffType.Add, 0, addLength, addedData));
                newIndex += addLength;
            }
        }

        return diffs;
    }
}

public enum DiffType
{
    Add,
    Copy,
    Remove
}

public class DiffItem
{
    public DiffType Type { get; }
    public int Offset { get; }
    public int Length { get; }
    public byte[]? Data { get; }

    public DiffItem(DiffType type, int offset, int length, byte[]? data = null)
    {
        Type = type;
        Offset = offset;
        Length = length;
        Data = data;
    }
}