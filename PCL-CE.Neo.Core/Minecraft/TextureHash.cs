using System;
using System.IO;
using System.Security.Cryptography;

namespace PCL_CE.Neo.Core.Minecraft;

public static class TextureHash
{
    public static string ComputeSha1(string filePath)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return ComputeSha1(stream);
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string ComputeSha1(Stream stream)
    {
        try
        {
            using var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string ComputeMd5(string filePath)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return ComputeMd5(stream);
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string ComputeMd5(Stream stream)
    {
        try
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string ComputeSha256(string filePath)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return ComputeSha256(stream);
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string ComputeSha256(Stream stream)
    {
        try
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        catch
        {
            return string.Empty;
        }
    }

    public static bool VerifyHash(string filePath, string expectedHash, HashAlgorithmType algorithm = HashAlgorithmType.Sha1)
    {
        try
        {
            string actualHash;
            
            switch (algorithm)
            {
                case HashAlgorithmType.Md5:
                    actualHash = ComputeMd5(filePath);
                    break;
                case HashAlgorithmType.Sha256:
                    actualHash = ComputeSha256(filePath);
                    break;
                default:
                    actualHash = ComputeSha1(filePath);
                    break;
            }

            return actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}

public enum HashAlgorithmType
{
    Md5,
    Sha1,
    Sha256
}