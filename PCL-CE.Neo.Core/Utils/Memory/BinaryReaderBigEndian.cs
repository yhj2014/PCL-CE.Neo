using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace PCL_CE.Neo.Core.Utils.Memory;

public class BinaryReaderBigEndian : BinaryReader
{
    public BinaryReaderBigEndian(Stream input) : base(input) { }
    public BinaryReaderBigEndian(Stream input, Encoding encoding) : base(input, encoding) { }
    public BinaryReaderBigEndian(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen) { }

    public override short ReadInt16()
    {
        return BinaryPrimitives.ReadInt16BigEndian(ReadBytes(2));
    }

    public override int ReadInt32()
    {
        return BinaryPrimitives.ReadInt32BigEndian(ReadBytes(4));
    }

    public override long ReadInt64()
    {
        return BinaryPrimitives.ReadInt64BigEndian(ReadBytes(8));
    }

    public override ushort ReadUInt16()
    {
        return BinaryPrimitives.ReadUInt16BigEndian(ReadBytes(2));
    }

    public override uint ReadUInt32()
    {
        return BinaryPrimitives.ReadUInt32BigEndian(ReadBytes(4));
    }

    public override ulong ReadUInt64()
    {
        return BinaryPrimitives.ReadUInt64BigEndian(ReadBytes(8));
    }

    public override float ReadSingle()
    {
        return BinaryPrimitives.ReadSingleBigEndian(ReadBytes(4));
    }

    public override double ReadDouble()
    {
        return BinaryPrimitives.ReadDoubleBigEndian(ReadBytes(8));
    }
}