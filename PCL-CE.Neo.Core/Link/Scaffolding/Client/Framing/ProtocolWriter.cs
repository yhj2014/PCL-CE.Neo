using System;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Abstractions;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Client.Framing;

public static class ProtocolWriter
{
    public static async Task WriteRequestAsync<TResponse>(
        PipeWriter writer, IRequest<TResponse> request, CancellationToken ct)
    {
        var typeInfo = request.RequestType;
        var typeInfoBytes = Encoding.UTF8.GetBytes(typeInfo);

        if (typeInfoBytes.Length > 128)
            throw new InvalidOperationException("Request type exceeds maximum length.");

        var body = request.SerializeRequestBody();

        var headerLength = 1 + typeInfoBytes.Length + 4;
        var totalLength = headerLength + body.Length;

        var buffer = writer.GetSpan(totalLength);

        buffer[0] = (byte)typeInfoBytes.Length;
        typeInfoBytes.CopyTo(buffer.Slice(1));
        BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(1 + typeInfoBytes.Length), (uint)body.Length);
        body.CopyTo(buffer.Slice(headerLength));

        writer.Advance(totalLength);
        await writer.FlushAsync(ct).ConfigureAwait(false);
    }
}