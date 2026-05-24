namespace PCL_CE.Neo.Core.Abstractions;

public interface INetworkAdapter
{
    event Action<NetworkLogEntry>? LogReceived;

    Task<string> GetAsync(string url, Dictionary<string, string>? headers = null);
    Task<byte[]> GetBytesAsync(string url, Dictionary<string, string>? headers = null);
    Task<HttpResponse> PostAsync(string url, string? body = null, Dictionary<string, string>? headers = null);

    void SetProxy(string? address, int? port = null, string? username = null, string? password = null);
    void EnableDoH(bool enabled);
    IWebServer CreateWebServer(int port);
}

public record HttpResponse
{
    public int StatusCode { get; init; }
    public string? ContentType { get; init; }
    public byte[] Body { get; init; } = [];
    public Dictionary<string, string> Headers { get; init; } = [];
    public string? ErrorMessage { get; init; }
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;

    public string BodyAsString => System.Text.Encoding.UTF8.GetString(Body);
}

public record NetworkLogEntry
{
    public DateTime Timestamp { get; init; }
    public required string Method { get; init; }
    public required string Url { get; init; }
    public int? StatusCode { get; init; }
    public long? ElapsedMs { get; init; }
    public string? Error { get; init; }
}

public interface IWebServer : IDisposable
{
    int Port { get; }
    bool IsRunning { get; }

    event Action<IWebServerRequest, Action<IWebServerResponse>>? RequestReceived;

    Task StartAsync();
    void Stop();
}

public interface IWebServerRequest
{
    string HttpMethod { get; }
    string RawUrl { get; }
    Uri? Url { get; }
}

public interface IWebServerResponse
{
    int StatusCode { get; set; }
    string? ContentType { get; set; }
    string? Body { get; set; }
}
