using System;

namespace PCL_CE.Neo.Core.Utils.Secret;

public static class Identify
{
    private const string Salt = "PCL-CE-NEO-SALT";
    private const string Key = "PCL-CE-NEO-KEY";

    public static string Encrypt(string plainText)
    {
        try
        {
            return EncryptHelper.EncryptAes(plainText, Key);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Identify encryption failed");
            throw;
        }
    }

    public static string Decrypt(string cipherText)
    {
        try
        {
            return EncryptHelper.DecryptAes(cipherText, Key);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Identify decryption failed");
            throw;
        }
    }

    public static string Hash(string input)
    {
        try
        {
            return EncryptHelper.HashPassword(input, Salt);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Identify hash failed");
            throw;
        }
    }

    public static string GenerateId()
    {
        try
        {
            return EncryptHelper.GenerateSecureToken(16);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Identify ID generation failed");
            throw;
        }
    }

    public static bool VerifyHash(string input, string hash)
    {
        try
        {
            var computedHash = Hash(input);
            return computedHash.Equals(hash, StringComparison.Ordinal);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Identify hash verification failed");
            return false;
        }
    }
}