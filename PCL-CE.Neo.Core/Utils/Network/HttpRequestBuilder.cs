using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PCL_CE.Neo.Core.Utils.Network;

public class HttpRequestBuilder
{
    private readonly HttpRequestMessage _request = new HttpRequestMessage();
    private readonly Dictionary<string, string> _queryParams = new Dictionary<string, string>();
    private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();
    private HttpContent? _content;

    public HttpRequestBuilder Method(HttpMethod method)
    {
        _request.Method = method;
        return this;
    }

    public HttpRequestBuilder Get() => Method(HttpMethod.Get);
    public HttpRequestBuilder Post() => Method(HttpMethod.Post);
    public HttpRequestBuilder Put() => Method(HttpMethod.Put);
    public HttpRequestBuilder Delete() => Method(HttpMethod.Delete);
    public HttpRequestBuilder Patch() => Method(new HttpMethod("PATCH"));

    public HttpRequestBuilder Url(string url)
    {
        _request.RequestUri = new Uri(url);
        return this;
    }

    public HttpRequestBuilder Url(Uri uri)
    {
        _request.RequestUri = uri;
        return this;
    }

    public HttpRequestBuilder QueryParam(string key, string value)
    {
        _queryParams[key] = value;
        return this;
    }

    public HttpRequestBuilder QueryParams(Dictionary<string, string> parameters)
    {
        foreach (var (key, value) in parameters)
            _queryParams[key] = value;
        return this;
    }

    public HttpRequestBuilder Header(string key, string value)
    {
        _headers[key] = value;
        return this;
    }

    public HttpRequestBuilder Headers(Dictionary<string, string> headers)
    {
        foreach (var (key, value) in headers)
            _headers[key] = value;
        return this;
    }

    public HttpRequestBuilder BearerToken(string token)
    {
        _headers["Authorization"] = $"Bearer {token}";
        return this;
    }

    public HttpRequestBuilder UserAgent(string userAgent)
    {
        _headers["User-Agent"] = userAgent;
        return this;
    }

    public HttpRequestBuilder AcceptJson()
    {
        _headers["Accept"] = "application/json";
        return this;
    }

    public HttpRequestBuilder JsonBody(object obj)
    {
        string json = JsonSerializer.Serialize(obj);
        _content = new StringContent(json, Encoding.UTF8, "application/json");
        return this;
    }

    public HttpRequestBuilder FormBody(Dictionary<string, string> formData)
    {
        _content = new FormUrlEncodedContent(formData);
        return this;
    }

    public HttpRequestBuilder StringBody(string content, string mediaType = "text/plain")
    {
        _content = new StringContent(content, Encoding.UTF8, mediaType);
        return this;
    }

    public HttpRequestBuilder ByteArrayBody(byte[] content, string mediaType = "application/octet-stream")
    {
        _content = new ByteArrayContent(content);
        _content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        return this;
    }

    public HttpRequestBuilder Timeout(TimeSpan timeout)
    {
        return this;
    }

    public HttpRequestMessage Build()
    {
        if (_queryParams.Count > 0 && _request.RequestUri != null)
        {
            string queryString = string.Join("&", _queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            string newUri = _request.RequestUri.ToString();
            newUri += newUri.Contains("?") ? "&" : "?";
            newUri += queryString;
            _request.RequestUri = new Uri(newUri);
        }

        foreach (var (key, value) in _headers)
            _request.Headers.TryAddWithoutValidation(key, value);

        if (_content != null)
            _request.Content = _content;

        return _request;
    }
}