using System.Buffers;

namespace PCL_CE.Neo.Core.Utils.Memory;

public static class BufferPool
{
    private const int DefaultBufferSize = 4096;

    public static byte[] Rent(int minimumLength = DefaultBufferSize)
    {
        return ArrayPool<byte>.Shared.Rent(minimumLength);
    }

    public static void Return(byte[] buffer)
    {
        ArrayPool<byte>.Shared.Return(buffer);
    }

    public static void Return(byte[] buffer, bool clearArray)
    {
        ArrayPool<byte>.Shared.Return(buffer, clearArray);
    }

    public static void ClearAndReturn(byte[] buffer)
    {
        Array.Clear(buffer, 0, buffer.Length);
        Return(buffer);
    }

    public static Memory<byte> RentMemory(int minimumLength = DefaultBufferSize)
    {
        return Rent(minimumLength).AsMemory();
    }

    public static Span<byte> RentSpan(int minimumLength = DefaultBufferSize)
    {
        return Rent(minimumLength).AsSpan();
    }

    public static T[] Rent<T>(int minimumLength)
    {
        return ArrayPool<T>.Shared.Rent(minimumLength);
    }

    public static void Return<T>(T[] buffer)
    {
        ArrayPool<T>.Shared.Return(buffer);
    }

    public static void Return<T>(T[] buffer, bool clearArray)
    {
        ArrayPool<T>.Shared.Return(buffer, clearArray);
    }
}