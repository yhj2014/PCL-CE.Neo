using System;
using System.Security.Cryptography;

namespace PCL_CE.Neo.Core.Utils.Encryption;

public sealed class AesCbcProvider : IEncryptionProvider
{
    public static AesCbcProvider Instance { get; } = new();

    private const int KeySize = 32;
    private const int IvSize = 16;
    private const int SaltSize = 16;

    public byte[] Encrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        var salt = new byte[SaltSize];
        var iv = new byte[IvSize];
        RandomNumberGenerator.Fill(salt);
        RandomNumberGenerator.Fill(iv);

        Span<byte> outputKey = stackalloc byte[KeySize];
        _DeriveKey(key, salt, outputKey);

        using var aes = Aes.Create();
        aes.Key = outputKey.ToArray();
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var ciphertext = encryptor.TransformFinalBlock(data.ToArray(), 0, data.Length);

        var result = new byte[SaltSize + IvSize + ciphertext.Length];
        var resultSpan = result.AsSpan();
        salt.CopyTo(resultSpan[..SaltSize]);
        iv.CopyTo(resultSpan.Slice(SaltSize, IvSize));
        ciphertext.AsSpan().CopyTo(resultSpan[(SaltSize + IvSize)..]);

        return result;
    }

    public byte[] Decrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        if (data.Length < SaltSize + IvSize)
            throw new ArgumentException("Invalid encrypted data length");

        var salt = data[..SaltSize];
        var iv = data.Slice(SaltSize, IvSize);
        var ciphertext = data[(SaltSize + IvSize)..];

        Span<byte> outputKey = stackalloc byte[KeySize];
        _DeriveKey(key, salt, outputKey);

        using var aes = Aes.Create();
        aes.Key = outputKey.ToArray();
        aes.IV = iv.ToArray();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(ciphertext.ToArray(), 0, ciphertext.Length);
    }

    private static readonly byte[] _Info = "PCL_CE.Neo.Core.Utils.Encryption.AesCbc"u8.ToArray();
    private static void _DeriveKey(ReadOnlySpan<byte> ikm, ReadOnlySpan<byte> salt, Span<byte> outputKey)
    {
        HKDF.DeriveKey(HashAlgorithmName.SHA256, ikm, outputKey, salt, _Info.AsSpan());
    }

    public bool IsSupported => AesCbc.IsSupported;
}