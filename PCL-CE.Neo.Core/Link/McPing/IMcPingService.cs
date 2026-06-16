using System.Net;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Link.McPing.Model;

namespace PCL_CE.Neo.Core.Link.McPing;

public interface IMcPingService
{
    Task<McPingResult?> PingAsync(CancellationToken cancellationToken = default);
}