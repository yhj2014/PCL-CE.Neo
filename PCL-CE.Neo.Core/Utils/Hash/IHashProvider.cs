using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Hash;

public interface IHashProvider
{
    int Length { get; }
    ValueTask<byte[]> ComputeHashAsync(Stream input, CancellationToken cancellationToken = default);
    byte[] ComputeHash(Stream input);
    byte[] ComputeHash(ReadOnlySpan<byte> input);
    byte[] ComputeHash(string input, Encoding? encoding = null);
    Task<byte[]> ComputeHashAsync(byte[] data, CancellationToken cancellationToken = default);
    Task<string> ComputeHashStringAsync(byte[] data, CancellationToken cancellationToken = default);
}