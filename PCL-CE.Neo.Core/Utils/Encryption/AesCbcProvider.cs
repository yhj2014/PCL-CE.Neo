using System;
using System.Security.Cryptography;

namespace PCL.CE.Neo.Core.Utils.Encryption;

public class AesCbcProvider : IEncryptionProvider
{
    public static AesCbcProvider Instance { get; } = new();

    private const int IvSize = 16;

    public byte[] Encrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        byte[] iv = new byte[IvSize];
        RandomNumberGenerator.Fill(iv);

        using (var aes = Aes.Create())
        {
            aes.Key = key.ToArray();
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (var encryptor = aes.CreateEncryptor())
            {
                byte[] ciphertext = encryptor.TransformFinalBlock(data.ToArray(), 0, data.Length);
                
                byte[] result = new byte[IvSize + ciphertext.Length];
                Buffer.BlockCopy(iv, 0, result, 0, IvSize);
                Buffer.BlockCopy(ciphertext, 0, result, IvSize, ciphertext.Length);

                return result;
            }
        }
    }

    public byte[] Decrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        if (data.Length < IvSize)
            throw new ArgumentException("加密数据长度不足。");

        ReadOnlySpan<byte> iv = data.Slice(0, IvSize);
        ReadOnlySpan<byte> ciphertext = data.Slice(IvSize);

        using (var aes = Aes.Create())
        {
            aes.Key = key.ToArray();
            aes.IV = iv.ToArray();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (var decryptor = aes.CreateDecryptor())
            {
                return decryptor.TransformFinalBlock(ciphertext.ToArray(), 0, ciphertext.Length);
            }
        }
    }

    public bool IsSupported => Aes.IsSupported;
}