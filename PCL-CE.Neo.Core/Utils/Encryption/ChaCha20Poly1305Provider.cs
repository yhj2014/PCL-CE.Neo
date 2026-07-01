using System.Security.Cryptography;

namespace PCL_CE.Neo.Core.Utils.Encryption;

public class ChaCha20Poly1305Provider : IEncryptionProvider
{
    public static ChaCha20Poly1305Provider Instance { get; } = new();

    private const int NonceSize = 12;
    private const int TagSize = 16;

    public byte[] Encrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        byte[] nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        byte[] ciphertext = new byte[data.Length];
        byte[] tag = new byte[TagSize];

        using var chaCha20 = new ChaCha20Poly1305(key);
        chaCha20.Encrypt(nonce, data, ciphertext, tag);

        byte[] result = new byte[NonceSize + TagSize + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
        Buffer.BlockCopy(ciphertext, 0, result, NonceSize + TagSize, ciphertext.Length);

        return result;
    }

    public byte[] Decrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        if (data.Length < NonceSize + TagSize)
            throw new ArgumentException("加密数据长度不足。");

        ReadOnlySpan<byte> nonce = data.Slice(0, NonceSize);
        ReadOnlySpan<byte> tag = data.Slice(NonceSize, TagSize);
        ReadOnlySpan<byte> ciphertext = data.Slice(NonceSize + TagSize);

        byte[] plaintext = new byte[ciphertext.Length];

        using var chaCha20 = new ChaCha20Poly1305(key);
        try
        {
            chaCha20.Decrypt(nonce, ciphertext, tag, plaintext);
        }
        catch (CryptographicException)
        {
            throw new CryptographicException("解密失败，数据可能已被篡改。");
        }

        return plaintext;
    }

    public bool IsSupported => ChaCha20Poly1305.IsSupported;
}