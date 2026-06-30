using PCL_CE.Neo.Core.Logging;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Encryption;

public class ChaCha20Poly1305Provider : IEncryptionProvider
{
    private const string ModuleName = "ChaCha20Poly1305Provider";
    public int KeySize => 32;
    public int NonceSize => 12;
    public int TagSize => 16;

    public async Task<byte[]> EncryptAsync(byte[] plaintext, byte[] key, byte[] nonce, byte[]? associatedData = null, CancellationToken cancellationToken = default)
    {
        ValidateInputs(plaintext, key, nonce);
        
        try
        {
            LogWrapper.Debug("Starting ChaCha20-Poly1305 encryption", ModuleName);
            
            byte[] ciphertext = new byte[plaintext.Length + TagSize];
            
            await Task.Run(() =>
            {
                using var chacha = new ChaCha20Poly1305(key);
                chacha.Encrypt(nonce, plaintext, ciphertext, associatedData);
            }, cancellationToken);
            
            LogWrapper.Debug($"ChaCha20-Poly1305 encryption completed. Plaintext: {plaintext.Length} bytes, Ciphertext: {ciphertext.Length} bytes", ModuleName);
            return ciphertext;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "ChaCha20-Poly1305 encryption failed");
            throw;
        }
    }

    public async Task<byte[]> DecryptAsync(byte[] ciphertext, byte[] key, byte[] nonce, byte[]? associatedData = null, CancellationToken cancellationToken = default)
    {
        if (ciphertext.Length < TagSize)
            throw new ArgumentException($"Ciphertext must be at least {TagSize} bytes", nameof(ciphertext));
        
        ValidateInputs(ciphertext, key, nonce);
        
        try
        {
            LogWrapper.Debug("Starting ChaCha20-Poly1305 decryption", ModuleName);
            
            byte[] plaintext = new byte[ciphertext.Length - TagSize];
            
            await Task.Run(() =>
            {
                using var chacha = new ChaCha20Poly1305(key);
                chacha.Decrypt(nonce, ciphertext, plaintext, associatedData);
            }, cancellationToken);
            
            LogWrapper.Debug($"ChaCha20-Poly1305 decryption completed. Ciphertext: {ciphertext.Length} bytes, Plaintext: {plaintext.Length} bytes", ModuleName);
            return plaintext;
        }
        catch (CryptographicException ex)
        {
            LogWrapper.Error(ex, ModuleName, "ChaCha20-Poly1305 decryption failed - invalid key or corrupted data");
            throw;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "ChaCha20-Poly1305 decryption failed");
            throw;
        }
    }

    private void ValidateInputs(byte[] data, byte[] key, byte[] nonce)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentNullException(nameof(data));
        if (key == null || key.Length != KeySize)
            throw new ArgumentException($"Key must be {KeySize} bytes", nameof(key));
        if (nonce == null || nonce.Length != NonceSize)
            throw new ArgumentException($"Nonce must be {NonceSize} bytes", nameof(nonce));
    }
}