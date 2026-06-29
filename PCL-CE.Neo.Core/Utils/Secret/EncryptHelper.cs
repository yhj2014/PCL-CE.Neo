using System;
using System.Security.Cryptography;
using System.Text;

namespace PCL_CE.Neo.Core.Utils.Secret;

public static class EncryptHelper
{
    public static string EncryptAes(string plainText, string key)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Key = GetKeyBytes(key);
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var memoryStream = new System.IO.MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
            cryptoStream.FlushFinalBlock();

            var encryptedBytes = memoryStream.ToArray();
            var combinedBytes = new byte[aes.IV.Length + encryptedBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, combinedBytes, 0, aes.IV.Length);
            Buffer.BlockCopy(encryptedBytes, 0, combinedBytes, aes.IV.Length, encryptedBytes.Length);

            return Convert.ToBase64String(combinedBytes);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "AES encryption failed");
            throw;
        }
    }

    public static string DecryptAes(string cipherText, string key)
    {
        try
        {
            var combinedBytes = Convert.FromBase64String(cipherText);
            var iv = new byte[16];
            var encryptedBytes = new byte[combinedBytes.Length - 16];
            
            Buffer.BlockCopy(combinedBytes, 0, iv, 0, 16);
            Buffer.BlockCopy(combinedBytes, 16, encryptedBytes, 0, encryptedBytes.Length);

            using var aes = Aes.Create();
            aes.Key = GetKeyBytes(key);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var memoryStream = new System.IO.MemoryStream(encryptedBytes);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new System.IO.StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "AES decryption failed");
            throw;
        }
    }

    public static string HashPassword(string password, string salt)
    {
        try
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 10000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);
            return Convert.ToBase64String(hash);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Password hashing failed");
            throw;
        }
    }

    public static string GenerateSalt()
    {
        try
        {
            var salt = new byte[16];
            RandomUtils.SecureRandom.NextBytes(salt);
            return Convert.ToBase64String(salt);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Salt generation failed");
            throw;
        }
    }

    public static string GenerateSecureToken(int length = 32)
    {
        try
        {
            var token = new byte[length];
            RandomUtils.SecureRandom.NextBytes(token);
            return Convert.ToBase64String(token);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Secure token generation failed");
            throw;
        }
    }

    private static byte[] GetKeyBytes(string key)
    {
        using var sha256 = SHA256.Create();
        var keyBytes = Encoding.UTF8.GetBytes(key);
        return sha256.ComputeHash(keyBytes);
    }
}