using System;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Exceptions;

public class ScaffoldingRequestException : Exception
{
    public int StatusCode { get; }

    public ScaffoldingRequestException(string message) : base(message) { }

    public ScaffoldingRequestException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public ScaffoldingRequestException(string message, Exception inner) : base(message, inner) { }
}