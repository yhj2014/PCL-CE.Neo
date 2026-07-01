using System.Security.Cryptography;

namespace PCL_CE.Neo.Core.Utils.Encryption;

public class AesCbcProvider : IEncryptionProvider
{
    public static AesCbcProvider Instance { get; } = new();

    private const int BlockSize = 16;

    public byte[] Encrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        byte[] iv = new byte[BlockSize];
        RandomNumberGenerator.Fill(iv);

        using var aes = Aes.Create();
        aes.Key = key.ToArray();
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        byte[] ciphertext = aes.CreateEncryptor().TransformFinalBlock(data.ToArray(), 0, data.Length);

        byte[] result = new byte[iv.Length + ciphertext.Length];
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(ciphertext, 0, result, iv.Length, ciphertext.Length);

        return result;
    }

    public byte[] Decrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        if (data.Length < BlockSize)
            throw new ArgumentException("加密数据长度不足。");

        ReadOnlySpan<byte> iv = data.Slice(0, BlockSize);
        ReadOnlySpan<byte> ciphertext = data.Slice(BlockSize);

        using var aes = Aes.Create();
        aes.Key = key.ToArray();
        aes.IV = iv.ToArray();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        return aes.CreateDecryptor().TransformFinalBlock(ciphertext.ToArray(), 0, ciphertext.Length);
    }

    public bool IsSupported => true;
}