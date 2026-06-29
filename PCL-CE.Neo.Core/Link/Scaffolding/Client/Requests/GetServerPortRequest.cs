using System;
using System.Buffers;
using System.Buffers.Binary;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Abstractions;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Client.Requests;

public sealed class GetServerPortRequest : IRequest<ushort>
{
    public string RequestType { get; } = "c:server_port";

    public void WriteRequestBody(IBufferWriter<byte> writer)
    {
    }

    public ushort ParseResponseBody(ReadOnlyMemory<byte> responseBody)
    {
        if (responseBody.Length != 2)
        {
            throw new InvalidOperationException("Invalid response body for server port.");
        }

        return BinaryPrimitives.ReadUInt16BigEndian(responseBody.Span);
    }
}