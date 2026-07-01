using System.Net.Http.Json;
using System.Text.Json;

namespace PCL_CE.Neo.Core.Utils.Network;

public static class HttpResponseExtensions
{
    public static async Task<T?> DeserializeJsonAsync<T>(this HttpResponseMessage response, JsonSerializerOptions? options = null)
    {
        if (!response.IsSuccessStatusCode)
            return default;

        try
        {
            return await response.Content.ReadFromJsonAsync<T>(options);
        }
        catch
        {
            return default;
        }
    }

    public static async Task<string> ReadContentAsStringAsync(this HttpResponseMessage response)
    {
        try
        {
            return await response.Content.ReadAsStringAsync();
        }
        catch
        {
            return string.Empty;
        }
    }

    public static async Task<byte[]> ReadContentAsByteArrayAsync(this HttpResponseMessage response)
    {
        try
        {
            return await response.Content.ReadAsByteArrayAsync();
        }
        catch
        {
            return Array.Empty<byte>();
        }
    }

    public static bool IsSuccessStatusCode(this HttpResponseMessage response)
    {
        return (int)response.StatusCode >= 200 && (int)response.StatusCode < 300;
    }

    public static bool IsClientError(this HttpResponseMessage response)
    {
        return (int)response.StatusCode >= 400 && (int)response.StatusCode < 500;
    }

    public static bool IsServerError(this HttpResponseMessage response)
    {
        return (int)response.StatusCode >= 500 && (int)response.StatusCode < 600;
    }

    public static string GetErrorMessage(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return string.Empty;

        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.BadRequest => "请求参数错误",
            System.Net.HttpStatusCode.Unauthorized => "未授权访问",
            System.Net.HttpStatusCode.Forbidden => "访问被拒绝",
            System.Net.HttpStatusCode.NotFound => "资源未找到",
            System.Net.HttpStatusCode.RequestTimeout => "请求超时",
            System.Net.HttpStatusCode.InternalServerError => "服务器内部错误",
            System.Net.HttpStatusCode.ServiceUnavailable => "服务不可用",
            _ => $"HTTP错误 {response.StatusCode}"
        };
    }
}