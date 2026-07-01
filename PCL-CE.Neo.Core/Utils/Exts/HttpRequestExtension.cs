using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class HttpRequestExtension
{
    public static async Task<T?> GetJsonAsync<T>(this HttpClient client, string requestUri, JsonSerializerOptions? options = null)
    {
        return await client.GetFromJsonAsync<T>(requestUri, options);
    }

    public static async Task<T?> PostJsonAsync<T>(this HttpClient client, string requestUri, object content, JsonSerializerOptions? options = null)
    {
        string json = JsonSerializer.Serialize(content, options);
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync(requestUri, stringContent);
        return await response.Content.ReadFromJsonAsync<T>(options);
    }

    public static async Task<T?> PutJsonAsync<T>(this HttpClient client, string requestUri, object content, JsonSerializerOptions? options = null)
    {
        string json = JsonSerializer.Serialize(content, options);
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PutAsync(requestUri, stringContent);
        return await response.Content.ReadFromJsonAsync<T>(options);
    }

    public static async Task<T?> DeleteJsonAsync<T>(this HttpClient client, string requestUri, JsonSerializerOptions? options = null)
    {
        HttpResponseMessage response = await client.DeleteAsync(requestUri);
        return await response.Content.ReadFromJsonAsync<T>(options);
    }

    public static HttpRequestMessage WithHeader(this HttpRequestMessage request, string name, string value)
    {
        request.Headers.TryAddWithoutValidation(name, value);
        return request;
    }

    public static HttpRequestMessage WithHeaders(this HttpRequestMessage request, IDictionary<string, string> headers)
    {
        foreach (var (key, value) in headers)
            request.Headers.TryAddWithoutValidation(key, value);
        return request;
    }

    public static HttpRequestMessage WithBearerToken(this HttpRequestMessage request, string token)
    {
        return request.WithHeader("Authorization", $"Bearer {token}");
    }

    public static HttpRequestMessage WithUserAgent(this HttpRequestMessage request, string userAgent)
    {
        return request.WithHeader("User-Agent", userAgent);
    }
}