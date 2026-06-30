using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Link.Scaffolding.Server.Abstractions;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Server.Handlers;

public class PingHandler : IRequestHandler
{
    public string RequestType { get; } = "c:ping";
    private readonly ILogger<PingHandler> _logger;

    public PingHandler(ILogger<PingHandler> logger)
    {
        _logger = logger;
    }

    public Task<(byte Status, ReadOnlyMemory<byte> Body)> HandleAsync(
        ReadOnlyMemory<byte> requestBody, IServerContext context, string sessionId, CancellationToken ct)
    {
        _logger.LogDebug("[{SessionId}] 处理 ping 请求", sessionId);
        return Task.FromResult((Status: (byte)0, Body: requestBody));
    }
}