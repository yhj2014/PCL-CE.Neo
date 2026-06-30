using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Utils.Encryption;

public class AesCbcProvider : IEncryptionProvider
{
    private readonly ILogger<AesCbcProvider> _logger;

    public AesCbcProvider(ILogger<AesCbcProvider> logger)
    {
        _logger = logger;
    }

    public string ProviderName => "AES-CBC";

    public int KeySize => 256;

    public int NonceSize => 16;

    public int TagSize => 0;

    public byte[] GenerateKey()
    {
        try
        {
            using var aes = Aes.Create();
            aes.KeySize = KeySize;
            aes.GenerateKey();
            _logger.LogDebug("AES-CBC key generated");
            return aes.Key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate AES-CBC key");
            throw;
        }
    }

    public byte[] GenerateNonce()
    {
        try
        {
            var nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);
            _logger.LogDebug("AES-CBC nonce generated");
            return nonce;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate AES-CBC nonce");
            throw;
        }
    }

    public byte[] Encrypt(byte[] plaintext, byte[] key, byte[] nonce)
    {
        if (plaintext == null)
            throw new ArgumentNullException(nameof(plaintext));
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        if (nonce == null)
            throw new ArgumentNullException(nameof(nonce));
        if (nonce.Length != NonceSize)
            throw new ArgumentException($"Nonce must be {NonceSize} bytes", nameof(nonce));

        try
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = nonce;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(plaintext, 0, plaintext.Length);
            }

            var ciphertext = ms.ToArray();
            _logger.LogDebug("AES-CBC encryption completed, ciphertext length: {Length}", ciphertext.Length);
            return ciphertext;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AES-CBC encryption failed");
            throw;
        }
    }

    public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] nonce)
    {
        if (ciphertext == null)
            throw new ArgumentNullException(nameof(ciphertext));
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        if (nonce == null)
            throw new ArgumentNullException(nameof(nonce));
        if (nonce.Length != NonceSize)
            throw new ArgumentException($"Nonce must be {NonceSize} bytes", nameof(nonce));

        try
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = nonce;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
            {
                cs.Write(ciphertext, 0, ciphertext.Length);
            }

            var plaintext = ms.ToArray();
            _logger.LogDebug("AES-CBC decryption completed, plaintext length: {Length}", plaintext.Length);
            return plaintext;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AES-CBC decryption failed");
            throw;
        }
    }

    public Task<byte[]> EncryptAsync(byte[] plaintext, byte[] key, byte[] nonce, byte[]? associatedData = null, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Encrypt(plaintext, key, nonce), cancellationToken);
    }

    public Task<byte[]> DecryptAsync(byte[] ciphertext, byte[] key, byte[] nonce, byte[]? associatedData = null, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Decrypt(ciphertext, key, nonce), cancellationToken);
    }

    public byte[] EncryptWithIntegrity(byte[] plaintext, byte[] key)
    {
        var nonce = GenerateNonce();
        var ciphertext = Encrypt(plaintext, key, nonce);
        var result = new byte[nonce.Length + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(ciphertext, 0, result, nonce.Length, ciphertext.Length);
        return result;
    }

    public byte[] DecryptWithIntegrity(byte[] data, byte[] key)
    {
        if (data.Length < NonceSize)
            throw new ArgumentException("Data too short", nameof(data));

        var nonce = new byte[NonceSize];
        var ciphertext = new byte[data.Length - NonceSize];
        Buffer.BlockCopy(data, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(data, NonceSize, ciphertext, 0, ciphertext.Length);
        return Decrypt(ciphertext, key, nonce);
    }

    public Task<byte[]> EncryptWithIntegrityAsync(byte[] plaintext, byte[] key)
    {
        return Task.Run(() => EncryptWithIntegrity(plaintext, key));
    }

    public Task<byte[]> DecryptWithIntegrityAsync(byte[] data, byte[] key)
    {
        return Task.Run(() => DecryptWithIntegrity(data, key));
    }
}