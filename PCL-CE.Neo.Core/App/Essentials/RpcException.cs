using System;

namespace PCL_CE.Neo.Core.App.Essentials;

public class RpcException(string reason) : Exception
{
    public string Reason => reason;
}