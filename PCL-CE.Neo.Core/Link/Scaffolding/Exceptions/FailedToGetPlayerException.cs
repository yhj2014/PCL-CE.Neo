using System;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Exceptions;

public class FailedToGetPlayerException : Exception
{
    public FailedToGetPlayerException() { }

    public FailedToGetPlayerException(string message) : base(message) { }

    public FailedToGetPlayerException(string message, Exception inner) : base(message, inner) { }
}