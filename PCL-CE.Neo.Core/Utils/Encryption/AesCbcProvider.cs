using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Encryption;

public class AesCbcProvider : IEncryptionProvider
{
    public int KeySize => 256;

    public byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }

    public byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(data, 0, data.Length);
    }

    public async Task<byte[]> EncryptAsync(byte[] data, byte[] key, byte[] iv, CancellationToken cancellationToken = default)
    {
        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        await cryptoStream.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
        await cryptoStream.FlushFinalBlockAsync(cancellationToken).ConfigureAwait(false);
        return memoryStream.ToArray();
    }

    public async Task<byte[]> DecryptAsync(byte[] data, byte[] key, byte[] iv, CancellationToken cancellationToken = default)
    {
        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write);
        await cryptoStream.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
        await cryptoStream.FlushFinalBlockAsync(cancellationToken).ConfigureAwait(false);
        return memoryStream.ToArray();
    }
}