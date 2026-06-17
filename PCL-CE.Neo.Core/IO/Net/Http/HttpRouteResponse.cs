using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Utils;

namespace PCL_CE.Neo.Core.IO.Net.Http;

[Serializable]
public class HttpRouteResponse
{
    public HttpStatusCode? StatusCode = null;
    public Encoding? ContentEncoding = null;
    public string? ContentType = null;
    public string? RedirectLocation = null;
    public bool? SendChunked = null;
    public CookieCollection? Cookies = null;
    public Stream? InputStream = null;

    public void Pour(HttpListenerResponse target)
    {
        target.StatusCode = (int)(StatusCode ?? HttpStatusCode.OK);
        if (ContentType is {} contentType) target.ContentType = contentType;
        if (ContentEncoding is {} contentEncoding) target.ContentEncoding = contentEncoding;
        if (RedirectLocation is {} redirectLocation) target.RedirectLocation = redirectLocation;
        if (SendChunked is {} sendChunked) target.SendChunked = sendChunked;
        if (Cookies is {} cookies) target.Cookies = cookies;
        if (InputStream is {} inputStream) inputStream.CopyTo(target.OutputStream);
    }

    public Task<HttpRouteResponse> AsTask() => Task.FromResult(this);

    public static HttpRouteResponse Empty(HttpStatusCode statusCode) => new() { StatusCode = statusCode };

    public static readonly HttpRouteResponse NoContent = Empty(HttpStatusCode.NoContent);
    public static readonly HttpRouteResponse BadRequest = Empty(HttpStatusCode.BadRequest);
    public static readonly HttpRouteResponse Forbidden = Empty(HttpStatusCode.Forbidden);
    public static readonly HttpRouteResponse NotFound = Empty(HttpStatusCode.NotFound);
    public static readonly HttpRouteResponse InternalServerError = Empty(HttpStatusCode.InternalServerError);
    public static readonly HttpRouteResponse BadGateway = Empty(HttpStatusCode.BadGateway);

    public static HttpRouteResponse Input(Stream stream, string contentType = "application/octet-stream", Encoding? encoding = null) =>
        new() { InputStream = stream, ContentType = contentType, ContentEncoding = encoding ?? Encoding.UTF8 };

    public static HttpRouteResponse Text(string text, string contentType = "text/plain", Encoding? encoding = null) =>
        Input(new StringStream(text, encoding), contentType, encoding);

    public static HttpRouteResponse Redirect(string location, HttpStatusCode statusCode = HttpStatusCode.Found) =>
        new() { StatusCode = statusCode, RedirectLocation = location };

    public static HttpRouteResponse Json(object obj, JsonSerializerOptions? options)
    {
        var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, obj, options);
        stream.Position = 0;
        return new HttpRouteResponse
        {
            ContentEncoding = Encoding.UTF8,
            ContentType = "application/json, charset=utf-8",
            InputStream = stream
        };
    }

    public static HttpRouteResponse Json(object obj) => Json(obj, null);
}