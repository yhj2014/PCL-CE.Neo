using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace PCL_CE.Neo.Core.Utils.Encryption;

public class ChaCha20SoftwareProvider : IEncryptionProvider
{
    public static ChaCha20SoftwareProvider Instance { get; } = new();

    private static readonly uint[] Sigma = { 0x61707865u, 0x3320646eu, 0x79622d32u, 0x6b206574u };

    public static bool IsSupported => true;

    public byte[] Encrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        var result = new byte[data.Length + 12];
        var nonce = result.AsSpan(0, 12);

        RandomNumberGenerator.Fill(nonce);

        Process(data, result.AsSpan(12), key, nonce);
        return result;
    }

    public byte[] Decrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        if (data.Length < 12) throw new ArgumentException("Data length is insufficient to contain Nonce");

        var nonce = data[..12];
        var ciphertext = data[12..];
        var plaintext = new byte[ciphertext.Length];

        Process(ciphertext, plaintext, key, nonce);
        return plaintext;
    }

    private static void Process(ReadOnlySpan<byte> input, Span<byte> output, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce)
    {
        if (key.Length != 32) throw new ArgumentException("Key must be 32 bytes");
        if (nonce.Length != 12) throw new ArgumentException("Nonce must be 12 bytes");

        Span<uint> state = stackalloc uint[16];

        Sigma.CopyTo(state[..4]);
        for (var i = 0; i < 8; i++)
        {
            state[4 + i] = BinaryPrimitives.ReadUInt32LittleEndian(key.Slice(i * 4, 4));
        }
        state[12] = 0;
        state[13] = BinaryPrimitives.ReadUInt32LittleEndian(nonce[..4]);
        state[14] = BinaryPrimitives.ReadUInt32LittleEndian(nonce[4..8]);
        state[15] = BinaryPrimitives.ReadUInt32LittleEndian(nonce[8..12]);

        Span<uint> workingBlock = stackalloc uint[16];
        var offset = 0;
        var length = input.Length;

        while (offset < length)
        {
            GenerateBlock(workingBlock, state);
            state[12]++;

            var remaining = Math.Min(64, length - offset);
            var inputPart = input.Slice(offset, remaining);
            var outputPart = output.Slice(offset, remaining);

            var blockBytes = MemoryMarshal.AsBytes(workingBlock);
            for (var i = 0; i < remaining; i++)
            {
                outputPart[i] = (byte)(inputPart[i] ^ blockBytes[i]);
            }

            offset += 64;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GenerateBlock(Span<uint> x, ReadOnlySpan<uint> input)
    {
        input.CopyTo(x);

        for (var i = 0; i < 10; i++)
        {
            QuarterRound(x, 0, 4, 8, 12);
            QuarterRound(x, 1, 5, 9, 13);
            QuarterRound(x, 2, 6, 10, 14);
            QuarterRound(x, 3, 7, 11, 15);
            QuarterRound(x, 0, 5, 10, 15);
            QuarterRound(x, 1, 6, 11, 12);
            QuarterRound(x, 2, 7, 8, 13);
            QuarterRound(x, 3, 4, 9, 14);
        }

        for (var i = 0; i < 16; i++)
        {
            x[i] += input[i];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void QuarterRound(Span<uint> x, int a, int b, int c, int d)
    {
        x[a] += x[b]; x[d] ^= x[a]; x[d] = BitOperations.RotateLeft(x[d], 16);
        x[c] += x[d]; x[b] ^= x[c]; x[b] = BitOperations.RotateLeft(x[b], 12);
        x[a] += x[b]; x[d] ^= x[a]; x[d] = BitOperations.RotateLeft(x[d], 8);
        x[c] += x[d]; x[b] ^= x[c]; x[b] = BitOperations.RotateLeft(x[b], 7);
    }
}
