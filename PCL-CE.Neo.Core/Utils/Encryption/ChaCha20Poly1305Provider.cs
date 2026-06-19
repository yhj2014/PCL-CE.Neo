using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Encryption;

public class ChaCha20Poly1305Provider : IEncryptionProvider
{
    public int KeySize => 256;

    public byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
    {
        var tag = new byte[16];
        using var cipher = new ChaCha20Poly1305(key);
        var result = new byte[data.Length];
        cipher.Encrypt(iv, data, result, tag);
        return result;
    }

    public byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
    {
        var tag = new byte[16];
        using var cipher = new ChaCha20Poly1305(key);
        var result = new byte[data.Length];
        cipher.Decrypt(iv, data, tag, result);
        return result;
    }

    public Task<byte[]> EncryptAsync(byte[] data, byte[] key, byte[] iv, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Encrypt(data, key, iv));
    }

    public Task<byte[]> DecryptAsync(byte[] data, byte[] key, byte[] iv, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Decrypt(data, key, iv));
    }
}