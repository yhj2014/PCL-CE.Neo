using System;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Client.Abstractions;

public readonly struct ScaffoldingResponse
{
    public readonly byte Status;
    public readonly ReadOnlyMemory<byte> Body;

    public ScaffoldingResponse(byte status, ReadOnlyMemory<byte> body)
    {
        Status = status;
        Body = body;
    }
}