using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Utils.Secret;

public class EncryptHelper
{
    private readonly ILogger<EncryptHelper> _logger;

    public EncryptHelper(ILogger<EncryptHelper> logger)
    {
        _logger = logger;
    }

    public byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
    {
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException(nameof(plainText));
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException(nameof(Key));
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException(nameof(IV));

        byte[] encrypted;

        try
        {
            using var aesAlg = Aes.Create();
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using var msEncrypt = new System.IO.MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using var swEncrypt = new System.IO.StreamWriter(csEncrypt);
            swEncrypt.Write(plainText);

            encrypted = msEncrypt.ToArray();
            _logger.LogDebug("String encrypted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt string");
            throw;
        }

        return encrypted;
    }

    public string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
    {
        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException(nameof(cipherText));
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException(nameof(Key));
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException(nameof(IV));

        string plaintext;

        try
        {
            using var aesAlg = Aes.Create();
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using var msDecrypt = new System.IO.MemoryStream(cipherText);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new System.IO.StreamReader(csDecrypt);
            plaintext = srDecrypt.ReadToEnd();

            _logger.LogDebug("String decrypted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt string");
            throw;
        }

        return plaintext;
    }

    public byte[] GenerateSalt(int size = 32)
    {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Salt size must be positive");

        var salt = new byte[size];
        RandomNumberGenerator.Fill(salt);
        _logger.LogDebug("Salt generated with size: {Size}", size);
        return salt;
    }

    public byte[] DeriveKey(string password, byte[] salt, int keySize = 256, int iterations = 100000)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password));
        if (salt == null || salt.Length == 0)
            throw new ArgumentNullException(nameof(salt));

        try
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var key = pbkdf2.GetBytes(keySize / 8);
            _logger.LogDebug("Key derived with {Iterations} iterations", iterations);
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to derive key");
            throw;
        }
    }

    public string Protect(string data, string password)
    {
        if (string.IsNullOrWhiteSpace(data))
            throw new ArgumentNullException(nameof(data));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password));

        try
        {
            var salt = GenerateSalt();
            var key = DeriveKey(password, salt);
            var iv = GenerateSalt(16);

            var bytes = Encoding.UTF8.GetBytes(data);
            var encrypted = EncryptStringToBytes_Aes(data, key, iv);

            var result = Convert.ToBase64String(salt.Concat(iv).Concat(encrypted).ToArray());
            _logger.LogDebug("Data protected successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to protect data");
            throw;
        }
    }

    public string Unprotect(string protectedData, string password)
    {
        if (string.IsNullOrWhiteSpace(protectedData))
            throw new ArgumentNullException(nameof(protectedData));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password));

        try
        {
            var data = Convert.FromBase64String(protectedData);

            const int saltSize = 32;
            const int ivSize = 16;

            if (data.Length < saltSize + ivSize)
                throw new FormatException("Invalid protected data format");

            var salt = data.Take(saltSize).ToArray();
            var iv = data.Skip(saltSize).Take(ivSize).ToArray();
            var encrypted = data.Skip(saltSize + ivSize).ToArray();

            var key = DeriveKey(password, salt);
            var result = DecryptStringFromBytes_Aes(encrypted, key, iv);

            _logger.LogDebug("Data unprotected successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unprotect data");
            throw;
        }
    }

    public bool TryUnprotect(string protectedData, string password, out string? result)
    {
        result = null;

        try
        {
            result = Unprotect(protectedData, password);
            return true;
        }
        catch
        {
            return false;
        }
    }
}