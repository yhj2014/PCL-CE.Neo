using System;
using System.IO;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.BZip2;

namespace PCL_CE.Neo.Core.Utils.Diff;

public class BsDiff : IBinaryDiff
{
    private const int HeaderSize = 32;
    private const long HeaderVersion = 0x3034464649445342;
    private const int HeaderCtrlIndex = 8;
    private const int HeaderDiffIndex = 16;
    private const int HeaderNewSizeIndex = 24;

    public async Task<byte[]> ApplyAsync(byte[] originData, byte[] diffData)
    {
        return await Task.Run(() =>
        {
            if (diffData.Length < HeaderSize)
                throw new Exception("Diff file size is less than the header size");
            if (BitConverter.ToInt64(diffData, HeaderVersionIndex) != HeaderVersion)
                throw new Exception("Diff file version is wrong");

            var ctrlLen = BitConverter.ToInt64(diffData, HeaderCtrlIndex);
            var diffLen = BitConverter.ToInt64(diffData, HeaderDiffIndex);
            var newLen = BitConverter.ToInt64(diffData, HeaderNewSizeIndex);
            var extraLen = diffData.Length - HeaderSize - ctrlLen - diffLen;

            if (ctrlLen < 0 || diffLen < 0 || extraLen < 0)
                throw new Exception("Block size is negative");
            if (newLen < 0)
                throw new Exception("Final file size info is negative");
            if (HeaderSize + ctrlLen + diffLen + extraLen > diffData.Length)
                throw new Exception("Diff file size info is not correct");

            var ctrlContent = new byte[ctrlLen];
            long curOffset = HeaderSize;
            Array.Copy(diffData, curOffset, ctrlContent, 0, ctrlLen);
            using var ctrlStream = new BZip2InputStream(new MemoryStream(ctrlContent));
            using var ctrlReader = new BinaryReader(ctrlStream);

            curOffset += ctrlLen;
            var diffContent = new byte[diffLen];
            Array.Copy(diffData, curOffset, diffContent, 0, diffLen);
            using var diffStream = new BZip2InputStream(new MemoryStream(diffContent));
            using var diffReader = new BinaryReader(diffStream);

            curOffset += diffLen;
            var extraContent = new byte[extraLen];
            Array.Copy(diffData, curOffset, extraContent, 0, extraLen);
            using var extraStream = new BZip2InputStream(new MemoryStream(extraContent));
            using var extraReader = new BinaryReader(extraStream);

            var ret = new byte[newLen];
            long newDataPos = 0;
            long oldDataPos = 0;

            while (newDataPos < newLen)
            {
                var addRange = ReadInt64(ctrlReader.ReadBytes(8));
                var copyRange = ReadInt64(ctrlReader.ReadBytes(8));
                var seekPos = ReadInt64(ctrlReader.ReadBytes(8));

                if (newDataPos + addRange > newLen)
                    throw new Exception($"Add range overflows, want add {addRange}, but only have {newLen - newDataPos} left");

                for (long i = 0; i < addRange; i++)
                {
                    var readedByte = diffReader.ReadByte();
                    if (oldDataPos + i < originData.Length)
                        ret[newDataPos + i] = (byte)(readedByte + originData[oldDataPos + i]);
                    else
                        ret[newDataPos + i] = readedByte;
                }

                newDataPos += addRange;
                oldDataPos += addRange;

                if (newDataPos + copyRange > newLen)
                    throw new Exception($"Copy range overflows, want copy {copyRange}, but only have {newLen - newDataPos} left");

                for (var i = 0; i < copyRange; i++)
                {
                    ret[newDataPos + i] = extraReader.ReadByte();
                }

                newDataPos += copyRange;
                oldDataPos += seekPos;

                if (oldDataPos > originData.Length)
                    throw new Exception($"Old data pos overflows, current old data length = {originData.Length}, but want {oldDataPos}");
            }

            return ret;
        });
    }

    public Task<byte[]> MakeAsync(byte[] originData, byte[] newData)
    {
        throw new NotSupportedException("BsDiff Make is not implemented");
    }

    internal static long ReadInt64(byte[] buffer, int offset = 0)
    {
        var value = ((long)buffer[offset] << 0) | ((long)buffer[offset + 1] << 8) |
                    ((long)buffer[offset + 2] << 16) | ((long)buffer[offset + 3] << 24) |
                    ((long)buffer[offset + 4] << 32) | ((long)buffer[offset + 5] << 40) |
                    ((long)buffer[offset + 6] << 48) | ((long)buffer[offset + 7] << 56);

        var mask = value >> 63;
        return (~mask & value) |
               (((value & unchecked((long)0x8000000000000000)) - value) & mask);
    }
}