using System;
using System.Buffers.Binary;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Abstractions;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Client.Requests;

public class GetServerProtRequest : Request<int>
{
    public override string RequestType { get; } = "c:server_port";

    public override int ParseResponseBody(ReadOnlyMemory<byte> body)
    {
        if (body.Length != 2)
            throw new InvalidOperationException("Invalid response body length for server port.");

        return BinaryPrimitives.ReadUInt16BigEndian(body.Span);
    }
}