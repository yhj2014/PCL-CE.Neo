using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Link.McPing;

public interface IMcPingService
{
    IPEndPoint Endpoint { get; }
    string Host { get; }
    int Timeout { get; }
    Task<Model.McPingResult?> PingAsync(CancellationToken cancellationToken = default);
}