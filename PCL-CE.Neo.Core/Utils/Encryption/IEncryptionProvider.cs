using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Encryption;

public interface IEncryptionProvider
{
    int KeySize { get; }
    
    byte[] Encrypt(byte[] data, byte[] key, byte[] iv);
    
    byte[] Decrypt(byte[] data, byte[] key, byte[] iv);
    
    Task<byte[]> EncryptAsync(byte[] data, byte[] key, byte[] iv, CancellationToken cancellationToken = default);
    
    Task<byte[]> DecryptAsync(byte[] data, byte[] key, byte[] iv, CancellationToken cancellationToken = default);
}