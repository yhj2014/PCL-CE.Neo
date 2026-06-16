using System;
using System.Security.Cryptography;
using System.Text;

namespace PCL_CE.Neo.Core.Utils.Hash;

public class MD5Provider : IHashProvider
{
    public string ComputeHash(byte[] data)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
    }

    public string ComputeHash(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        return ComputeHash(bytes);
    }

    public byte[] ComputeHashBytes(byte[] data)
    {
        using var md5 = MD5.Create();
        return md5.ComputeHash(data);
    }
}