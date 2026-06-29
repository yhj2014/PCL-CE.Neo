using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class HttpRequestExtension
{
    public static HttpRequestMessage AddJsonContent<T>(this HttpRequestMessage request, T content)
    {
        request.Content = new StringContent(
            JsonSerializer.Serialize(content),
            Encoding.UTF8,
            "application/json");
        return request;
    }

    public static HttpRequestMessage AddFormContent(this HttpRequestMessage request, params (string Key, string Value)[] formData)
    {
        var content = new FormUrlEncodedContent(formData);
        request.Content = content;
        return request;
    }

    public static HttpRequestMessage AddBearerToken(this HttpRequestMessage request, string token)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    public static HttpRequestMessage AddBasicAuth(this HttpRequestMessage request, string username, string password)
    {
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        return request;
    }

    public static HttpRequestMessage AddHeader(this HttpRequestMessage request, string name, string value)
    {
        request.Headers.Add(name, value);
        return request;
    }

    public static HttpRequestMessage AddAcceptJson(this HttpRequestMessage request)
    {
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }

    public static HttpRequestMessage AddAcceptXml(this HttpRequestMessage request)
    {
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
        return request;
    }

    public static async Task<T?> ReadJsonContentAsync<T>(this HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content);
    }

    public static async Task<string> ReadContentAsStringAsync(this HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<byte[]> ReadContentAsByteArrayAsync(this HttpResponseMessage response)
    {
        return await response.Content.ReadAsByteArrayAsync();
    }

    public static bool IsSuccessStatusCode(this HttpResponseMessage response)
    {
        return (int)response.StatusCode >= 200 && (int)response.StatusCode < 300;
    }

    public static async Task EnsureSuccessStatusCodeAsync(this HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode())
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {content}");
        }
    }

    public static HttpRequestMessage WithTimeout(this HttpRequestMessage request, TimeSpan timeout)
    {
        request.Properties["Timeout"] = timeout;
        return request;
    }

    public static HttpClient AddUserAgent(this HttpClient client, string userAgent)
    {
        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        return client;
    }

    public static HttpClient AddDefaultHeaders(this HttpClient client, params (string Name, string Value)[] headers)
    {
        foreach (var (name, value) in headers)
        {
            client.DefaultRequestHeaders.Add(name, value);
        }
        return client;
    }
}