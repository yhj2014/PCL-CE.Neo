using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.IO.Net.Http;

public static class HttpRequest
{
    public static HttpRequestMessage Create(string url)
    {
        return new HttpRequestMessage(HttpMethod.Get, new Uri(url));
    }

    public static async Task<HttpResponseMessage> SendAsync(this HttpRequestMessage requestMessage, HttpClient httpClient, CancellationToken cancellationToken = default)
    {
        using var request = requestMessage;
        return await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}