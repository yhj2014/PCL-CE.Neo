using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace PCL_CE.Neo.Core.Utils.Memory;

public class BinaryWriterBigEndian : BinaryWriter
{
    public BinaryWriterBigEndian(Stream output) : base(output) { }
    public BinaryWriterBigEndian(Stream output, Encoding encoding) : base(output, encoding) { }
    public BinaryWriterBigEndian(Stream output, Encoding encoding, bool leaveOpen) : base(output, encoding, leaveOpen) { }

    public override void Write(short value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        base.Write(bytes);
    }

    public override void Write(int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        base.Write(bytes);
    }

    public override void Write(long value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        base.Write(bytes);
    }

    public override void Write(ushort value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        base.Write(bytes);
    }

    public override void Write(uint value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        base.Write(bytes);
    }

    public override void Write(ulong value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        base.Write(bytes);
    }

    public override void Write(float value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        base.Write(bytes);
    }

    public override void Write(double value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        base.Write(bytes);
    }
}