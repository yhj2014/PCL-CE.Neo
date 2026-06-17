using System.Net.Http;

namespace PCL_CE.Neo.Core.IO.Net.Http;

public static class HttpHeaderExtension
{
    public static HttpRequestMessage WithHeader(this HttpRequestMessage requestMessage, string name, string value)
    {
        requestMessage.Headers.Add(name, value);
        return requestMessage;
    }

    public static HttpRequestMessage WithHeaders(this HttpRequestMessage requestMessage, params (string name, string value)[] headers)
    {
        foreach (var (name, value) in headers)
        {
            requestMessage.Headers.Add(name, value);
        }
        return requestMessage;
    }
}