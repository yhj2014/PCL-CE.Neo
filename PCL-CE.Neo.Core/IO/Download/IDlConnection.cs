using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.IO.Download;

public interface IDlConnection
{
    Task<NDlConnectionInfo> StartAsync(long beginOffset);
    Task StopAsync();
    Task<byte[]> ReadAsync(int length);
}