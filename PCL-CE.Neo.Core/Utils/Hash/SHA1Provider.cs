using System;
using System.Security.Cryptography;
using System.Text;

namespace PCL_CE.Neo.Core.Utils.Hash;

public class SHA1Provider : IHashProvider
{
    public string ComputeHash(byte[] data)
    {
        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
    }

    public string ComputeHash(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        return ComputeHash(bytes);
    }

    public byte[] ComputeHashBytes(byte[] data)
    {
        using var sha1 = SHA1.Create();
        return sha1.ComputeHash(data);
    }
}