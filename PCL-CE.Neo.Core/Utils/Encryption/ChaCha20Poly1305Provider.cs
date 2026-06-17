using System;
using System.Security.Cryptography;

namespace PCL_CE.Neo.Core.Utils.Encryption;

public sealed class ChaCha20Poly1305Provider : IEncryptionProvider
{
    public static ChaCha20Poly1305Provider Instance { get; } = new();

    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int KeySize = 32;
    private const int SaltSize = 16;

    public byte[] Encrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        var salt = new byte[SaltSize];
        var nonce = new byte[NonceSize];
        var tag = new byte[TagSize];
        RandomNumberGenerator.Fill(salt);
        RandomNumberGenerator.Fill(nonce);

        Span<byte> outputKey = stackalloc byte[KeySize];
        _DeriveKey(key, salt, outputKey);
        using var chacha = new ChaCha20Poly1305(outputKey);

        var ciphertext = new byte[data.Length];
        chacha.Encrypt(nonce, data, ciphertext, tag);

        var result = new byte[SaltSize + NonceSize + TagSize + ciphertext.Length];
        var resultSpan = result.AsSpan();

        salt.CopyTo(resultSpan[..SaltSize]);
        nonce.CopyTo(resultSpan.Slice(SaltSize, NonceSize));
        tag.CopyTo(resultSpan.Slice(SaltSize + NonceSize, TagSize));
        ciphertext.CopyTo(resultSpan.Slice(SaltSize + NonceSize + TagSize, ciphertext.Length));

        return result;
    }

    public byte[] Decrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        if (data.Length < SaltSize + NonceSize + TagSize)
            throw new ArgumentException("Invalid encrypted data length");

        var salt = data[..SaltSize];
        var nonce = data.Slice(SaltSize, NonceSize);
        var tag = data.Slice(SaltSize + NonceSize, TagSize);
        var ciphertext = data[(SaltSize + NonceSize + TagSize)..];

        Span<byte> outputKey = stackalloc byte[KeySize];
        _DeriveKey(key, salt, outputKey);
        using var chacha = new ChaCha20Poly1305(outputKey);

        var plaintext = new byte[ciphertext.Length];
        chacha.Decrypt(nonce, ciphertext, tag, plaintext);

        return plaintext;
    }

    private static readonly byte[] _Info = "PCL_CE.Neo.Core.Utils.Encryption.ChaCha20"u8.ToArray();
    private static void _DeriveKey(ReadOnlySpan<byte> ikm, ReadOnlySpan<byte> salt, Span<byte> outputKey)
    {
        HKDF.DeriveKey(
            HashAlgorithmName.SHA256,
            ikm,
            outputKey,
            salt,
            _Info.AsSpan());
    }

    public bool IsSupported => ChaCha20Poly1305.IsSupported;
}