using System;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Abstractions;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Client.Requests;

public class PingRequest : Request<byte[]>
{
    public override string RequestType { get; } = "c:ping";

    public override byte[] ParseResponseBody(ReadOnlyMemory<byte> body)
    {
        return body.ToArray();
    }
}