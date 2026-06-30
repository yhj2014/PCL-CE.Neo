using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Client.Abstractions;

public interface IRequest<out TResponse>
{
    string RequestType { get; }
    ReadOnlyMemory<byte> SerializeRequestBody();
    TResponse ParseResponseBody(ReadOnlyMemory<byte> body);
}

public abstract class Request<TResponse> : IRequest<TResponse>
{
    public abstract string RequestType { get; }
    public virtual ReadOnlyMemory<byte> SerializeRequestBody() => ReadOnlyMemory<byte>.Empty;
    public abstract TResponse ParseResponseBody(ReadOnlyMemory<byte> body);
}