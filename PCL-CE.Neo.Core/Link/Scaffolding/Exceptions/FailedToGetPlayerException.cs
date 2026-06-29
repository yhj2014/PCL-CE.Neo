using System;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Exceptions;

public class FailedToGetPlayerException : Exception
{
    public FailedToGetPlayerException() : base()
    {
    }

    public FailedToGetPlayerException(string msg) : base(msg)
    {
    }
}