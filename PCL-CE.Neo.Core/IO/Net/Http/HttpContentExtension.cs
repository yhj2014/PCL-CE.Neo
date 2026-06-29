using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace PCL_CE.Neo.Core.IO.Net.Http;

public static class HttpContentExtension
{
    public static HttpRequestMessage WithJsonContent<T>(this HttpRequestMessage requestMessage, T content, string? contentType = null)
    {
        requestMessage.Content = new StringContent(
            JsonSerializer.Serialize(content),
            Encoding.UTF8,
            contentType ?? "application/json");
        return requestMessage;
    }

    public static async Task<T?> AsJsonAsync<T>(this HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(content);
    }
}