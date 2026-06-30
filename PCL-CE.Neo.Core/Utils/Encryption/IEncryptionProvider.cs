using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Encryption;

public interface IEncryptionProvider
{
    Task<byte[]> EncryptAsync(byte[] plaintext, byte[] key, byte[] nonce, byte[]? associatedData = null, CancellationToken cancellationToken = default);
    Task<byte[]> DecryptAsync(byte[] ciphertext, byte[] key, byte[] nonce, byte[]? associatedData = null, CancellationToken cancellationToken = default);
    int KeySize { get; }
    int NonceSize { get; }
    int TagSize { get; }
}