namespace PCL_CE.Neo.Core.Abstractions;

public interface INetworkAdapter
{
    event Action<NetworkLogEntry>? LogReceived;

    Task<string> GetAsync(string url, Dictionary<string, string>? headers = null);
    Task<byte[]> GetBytesAsync(string url, Dictionary<string, string>? headers = null);
    Task<HttpResponse> PostAsync(string url, string? body = null, Dictionary<string, string>? headers = null);

    void SetProxy(string? address, int? port = null, string? username = null, string? password = null);
    void EnableDoH(bool enabled);
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
