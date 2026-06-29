using PCL_CE.Neo.Core.Link.Scaffolding.Server.Abstractions;
using System;
using System.Buffers.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Server.Handlers;

public class GetServerPortHandler : IRequestHandler
{
    public string RequestType { get; } = "c:server_port";

    public Task<(byte Status, ReadOnlyMemory<byte> Body)> HandleAsync(ReadOnlyMemory<byte> requestBody,
        IServerContext context, string sessionId, CancellationToken ct)
    {
        var port = context.MinecraftServerPort;

        if (port == 0)
        {
            return Task.FromResult<(byte, ReadOnlyMemory<byte>)>((32, ReadOnlyMemory<byte>.Empty));
        }

        var portBytes = new byte[2];
        var portAsUshort = (ushort)port;

        BinaryPrimitives.WriteUInt16BigEndian(portBytes.AsSpan(), portAsUshort);

        return Task.FromResult<(byte, ReadOnlyMemory<byte>)>((0, portBytes));
    }
}