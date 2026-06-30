using System;

namespace PCL_CE.Neo.Core.Utils.Hash;

public interface IHashProviderFactory
{
    IHashProvider GetProvider(string algorithmName);
}

public class HashProviderFactory : IHashProviderFactory
{
    public IHashProvider GetProvider(string algorithmName)
    {
        if (string.IsNullOrWhiteSpace(algorithmName))
            throw new ArgumentNullException(nameof(algorithmName));

        return algorithmName.ToUpperInvariant() switch
        {
            "MD5" => new MD5Provider(),
            "SHA1" => new SHA1Provider(),
            "SHA256" => new SHA256Provider(),
            "SHA512" => new SHA512Provider(),
            _ => throw new NotSupportedException($"Unsupported hash algorithm: {algorithmName}")
        };
    }
}