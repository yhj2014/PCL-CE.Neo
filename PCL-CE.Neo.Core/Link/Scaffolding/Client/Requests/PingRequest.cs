using System;
using System.Buffers;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Abstractions;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Client.Requests;

public sealed class PingRequest : IRequest<ReadOnlyMemory<byte>>
{
    private readonly ReadOnlyMemory<byte> _payload;

    public string RequestType { get; } = "c:ping";

    public PingRequest(ReadOnlyMemory<byte> payload)
    {
        if (payload.Length >= 32)
        {
            throw new ArgumentException("Payload must be less than 32 bytes.", nameof(payload));
        }

        _payload = payload;
    }

    public void WriteRequestBody(IBufferWriter<byte> writer)
    {
        writer.Write(_payload.Span);
    }

    public ReadOnlyMemory<byte> ParseResponseBody(ReadOnlyMemory<byte> responseBody)
    {
        return responseBody;
    }
}