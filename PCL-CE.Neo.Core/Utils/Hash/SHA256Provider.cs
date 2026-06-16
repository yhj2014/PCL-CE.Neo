using System;
using System.Security.Cryptography;
using System.Text;

namespace PCL_CE.Neo.Core.Utils.Hash;

public class SHA256Provider : IHashProvider
{
    public string ComputeHash(byte[] data)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
    }

    public string ComputeHash(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        return ComputeHash(bytes);
    }

    public byte[] ComputeHashBytes(byte[] data)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(data);
    }
}