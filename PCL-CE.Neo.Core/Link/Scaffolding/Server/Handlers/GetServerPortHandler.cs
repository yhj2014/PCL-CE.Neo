using System;
using System.Buffers.Binary;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Link.Scaffolding.Server.Abstractions;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Server.Handlers;

public class GetServerPortHandler : IRequestHandler
{
    public string RequestType { get; } = "c:server_port";
    private readonly ILogger<GetServerPortHandler> _logger;

    public GetServerPortHandler(ILogger<GetServerPortHandler> logger)
    {
        _logger = logger;
    }

    public Task<(byte Status, ReadOnlyMemory<byte> Body)> HandleAsync(
        ReadOnlyMemory<byte> requestBody, IServerContext context, string sessionId, CancellationToken ct)
    {
        var port = context.MinecraftServerProt;

        if (port == 0)
        {
            _logger.LogWarning("[{SessionId}] Minecraft 服务器端口为 0", sessionId);
            return Task.FromResult<(byte, ReadOnlyMemory<byte>)>((32, ReadOnlyMemory<byte>.Empty));
        }

        var portBytes = new byte[2];
        var portAsUshort = (ushort)port;
        BinaryPrimitives.WriteUInt16BigEndian(portBytes.AsSpan(), portAsUshort);

        _logger.LogDebug("[{SessionId}] 返回服务器端口: {Port}", sessionId, port);
        return Task.FromResult<(byte, ReadOnlyMemory<byte>)>((0, portBytes));
    }
}