using System;
using System.Net.Http;

namespace PCL_CE.Neo.Core.IO.Net.Http;

public static class HttpBasicExtension
{
    public static HttpRequestMessage WithHttpVersionOption(this HttpRequestMessage requestMessage, Version httpVersion)
    {
        requestMessage.Version = httpVersion;
        return requestMessage;
    }
}