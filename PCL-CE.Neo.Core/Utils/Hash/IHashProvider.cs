namespace PCL_CE.Neo.Core.Utils.Hash;

public interface IHashProvider
{
    int Length { get; }
    byte[] ComputeHash(byte[] input);
    byte[] ComputeHash(ReadOnlySpan<byte> input);
    byte[] ComputeHash(string input, System.Text.Encoding? encoding = null);
    byte[] ComputeHash(System.IO.Stream input);
    System.Threading.Tasks.ValueTask<byte[]> ComputeHashAsync(System.IO.Stream input, System.Threading.CancellationToken cancellationToken = default);
}