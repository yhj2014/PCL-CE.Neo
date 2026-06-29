using System.Buffers;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Client.Abstractions;

public interface IRequest<out T>
{
    string RequestType { get; }

    void WriteRequestBody(IBufferWriter<byte> writer);

    T ParseResponseBody(ReadOnlyMemory<byte> responseBody);
}