using System;
using System.Text;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Abstractions;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Client.Requests;

public class GetProtocolsRequest : Request<string[]>
{
    public override string RequestType { get; } = "c:protocols";

    public override string[] ParseResponseBody(ReadOnlyMemory<byte> body)
    {
        var content = Encoding.ASCII.GetString(body.Span);
        return content.Split('\0');
    }
}