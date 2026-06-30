using PCL_CE.Neo.Core.Logging;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Encryption;

public class AesGcmProvider : IEncryptionProvider
{
    private const string ModuleName = "AesGcmProvider";
    public int KeySize => 32;
    public int NonceSize => 12;
    public int TagSize => 16;

    public async Task<byte[]> EncryptAsync(byte[] plaintext, byte[] key, byte[] nonce, byte[]? associatedData = null, CancellationToken cancellationToken = default)
    {
        ValidateInputs(plaintext, key, nonce);
        
        try
        {
            LogWrapper.Debug("Starting AES-GCM encryption", ModuleName);
            
            byte[] ciphertext = new byte[plaintext.Length];
            byte[] tag = new byte[TagSize];
            
            await Task.Run(() =>
            {
                using var aesGcm = new AesGcm(key);
                aesGcm.Encrypt(nonce, plaintext, ciphertext, tag, associatedData);
            }, cancellationToken);
            
            byte[] result = new byte[ciphertext.Length + tag.Length];
            Buffer.BlockCopy(ciphertext, 0, result, 0, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, result, ciphertext.Length, tag.Length);
            
            LogWrapper.Debug($"AES-GCM encryption completed. Plaintext: {plaintext.Length} bytes, Ciphertext: {result.Length} bytes", ModuleName);
            return result;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "AES-GCM encryption failed");
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
            LogWrapper.Debug("Starting AES-GCM decryption", ModuleName);
            
            byte[] encryptedData = new byte[ciphertext.Length - TagSize];
            byte[] tag = new byte[TagSize];
            
            Buffer.BlockCopy(ciphertext, 0, encryptedData, 0, encryptedData.Length);
            Buffer.BlockCopy(ciphertext, encryptedData.Length, tag, 0, tag.Length);
            
            byte[] plaintext = new byte[encryptedData.Length];
            
            await Task.Run(() =>
            {
                using var aesGcm = new AesGcm(key);
                aesGcm.Decrypt(nonce, encryptedData, tag, plaintext, associatedData);
            }, cancellationToken);
            
            LogWrapper.Debug($"AES-GCM decryption completed. Ciphertext: {ciphertext.Length} bytes, Plaintext: {plaintext.Length} bytes", ModuleName);
            return plaintext;
        }
        catch (CryptographicException ex)
        {
            LogWrapper.Error(ex, ModuleName, "AES-GCM decryption failed - invalid key or corrupted data");
            throw;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "AES-GCM decryption failed");
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