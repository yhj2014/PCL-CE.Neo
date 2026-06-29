using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PCL_CE.Neo.Core.IO.Net.Http;

public static class HttpHeaderExtension
{
    public static HttpRequestMessage WithHeader(this HttpRequestMessage requestMessage, string key, string value)
    {
        if (key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase) && requestMessage.Content is not null)
            requestMessage.Content.Headers.TryAddWithoutValidation(key, value);
        else
            requestMessage.Headers.TryAddWithoutValidation(key, value);

        return requestMessage;
    }

    public static HttpRequestMessage WithHeaders(this HttpRequestMessage requestMessage, IDictionary<string, string> pairs)
    {
        ArgumentNullException.ThrowIfNull(pairs);

        foreach (var item in pairs)
        {
            requestMessage.WithHeader(item.Key, item.Value);
        }

        return requestMessage;
    }
}