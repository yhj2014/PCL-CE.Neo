using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Hash;

public class SHA256Provider : IHashProvider
{
    public static SHA256Provider Instance { get; } = new();
    public int Length => 64;

    public byte[] ComputeHash(ReadOnlySpan<byte> input)
    {
        return SHA256.HashData(input);
    }

    public byte[] ComputeHash(string input, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        return ComputeHash(encoding.GetBytes(input));
    }

    public byte[] ComputeHash(Stream input)
    {
        return SHA256.HashData(input);
    }

    public async ValueTask<byte[]> ComputeHashAsync(Stream input, CancellationToken cancellationToken = default)
    {
        return await SHA256.HashDataAsync(input, cancellationToken).ConfigureAwait(false);
    }

    public async Task<byte[]> ComputeHashAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        return await SHA256.HashDataAsync(data, cancellationToken).ConfigureAwait(false);
    }

    public Task<string> ComputeHashStringAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        var hash = SHA256.HashData(data);
        return Task.FromResult(BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant());
    }
}