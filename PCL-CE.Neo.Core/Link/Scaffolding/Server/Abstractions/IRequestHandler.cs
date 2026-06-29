using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Server.Abstractions;

public interface IRequestHandler
{
    string RequestType { get; }

    Task<(byte Status, ReadOnlyMemory<byte> Body)> HandleAsync
        (ReadOnlyMemory<byte> requestBody, IServerContext context, string sessionId, CancellationToken ct);
}