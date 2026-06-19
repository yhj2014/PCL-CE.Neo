using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Hash;

public interface IHashProvider
{
    int Length { get; }
    
    byte[] ComputeHash(byte[] input);
    
    byte[] ComputeHash(ReadOnlySpan<byte> input);
    
    byte[] ComputeHash(string input, Encoding? encoding = null);
    
    byte[] ComputeHash(Stream input);
    
    ValueTask<byte[]> ComputeHashAsync(Stream input, CancellationToken cancellationToken = default);
    
    string ComputeHashString(string input, Encoding? encoding = null);
    
    string ComputeHashString(Stream input);
    
    Task<string> ComputeHashStringAsync(Stream input, CancellationToken cancellationToken = default);
}