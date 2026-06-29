using System;
using System.Security.Cryptography;

namespace PCL.CE.Neo.Core.Utils.Encryption;

public class ChaCha20Poly1305Provider : IEncryptionProvider
{
    public static ChaCha20Poly1305Provider Instance { get; } = new();

    private const int NonceSize = 12;

    public byte[] Encrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        byte[] nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        byte[] ciphertext = new byte[data.Length];
        byte[] tag = new byte[16];

        ChaCha20Poly1305.Encrypt(nonce, data, ciphertext, tag, key);

        byte[] result = new byte[NonceSize + tag.Length + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, result, NonceSize, tag.Length);
        Buffer.BlockCopy(ciphertext, 0, result, NonceSize + tag.Length, ciphertext.Length);

        return result;
    }

    public byte[] Decrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        if (data.Length < NonceSize + 16)
            throw new ArgumentException("加密数据长度不足。");

        ReadOnlySpan<byte> nonce = data.Slice(0, NonceSize);
        ReadOnlySpan<byte> tag = data.Slice(NonceSize, 16);
        ReadOnlySpan<byte> ciphertext = data.Slice(NonceSize + 16);

        byte[] plaintext = new byte[ciphertext.Length];
        
        if (!ChaCha20Poly1305.TryDecrypt(nonce, ciphertext, tag, plaintext, key))
            throw new CryptographicException("解密失败，数据可能已被篡改。");

        return plaintext;
    }

    public bool IsSupported => ChaCha20Poly1305.IsSupported;
}