using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Network;

public interface INetworkService
{
    HttpClient HttpClient { get; }
    Task<string> GetStringAsync(string url, CancellationToken cancellationToken = default);
    Task<byte[]> GetBytesAsync(string url, CancellationToken cancellationToken = default);
    Task<T?> GetJsonAsync<T>(string url, CancellationToken cancellationToken = default);
    Task<string> PostStringAsync(string url, string content, CancellationToken cancellationToken = default);
    Task<T?> PostJsonAsync<T>(string url, object data, CancellationToken cancellationToken = default);
    void SetProxy(string? host, int? port);
    void ClearProxy();
}

public class NetworkService : INetworkService, IDisposable
{
    private readonly ILogger<NetworkService> _logger;
    private readonly HttpClient _httpClient;
    private HttpClientHandler? _handler;
    private string? _proxyHost;
    private int? _proxyPort;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public HttpClient HttpClient => _httpClient;

    public NetworkService() : this(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<NetworkService>.Instance)
    {
    }

    public NetworkService(ILogger<NetworkService> logger)
    {
        _logger = logger;
        _handler = new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.All,
            UseCookies = true,
            CookieContainer = new System.Net.CookieContainer()
        };
        _httpClient = new HttpClient(_handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "PCL-CE.Neo/2.0");
    }

    public async Task<string> GetStringAsync(string url, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("GET {Url}", url);
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to GET {Url}", url);
            throw;
        }
    }

    public async Task<byte[]> GetBytesAsync(string url, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("GET bytes from {Url}", url);
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to GET bytes from {Url}", url);
            throw;
        }
    }

    public async Task<T?> GetJsonAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        var json = await GetStringAsync(url, cancellationToken);
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse JSON from {Url}", url);
            throw;
        }
    }

    public async Task<string> PostStringAsync(string url, string content, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("POST to {Url}", url);
        try
        {
            var httpContent = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, httpContent, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to POST to {Url}", url);
            throw;
        }
    }

    public async Task<T?> PostJsonAsync<T>(string url, object data, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        var response = await PostStringAsync(url, json, cancellationToken);
        try
        {
            return JsonSerializer.Deserialize<T>(response, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse JSON response from {Url}", url);
            throw;
        }
    }

    public void SetProxy(string? host, int? port)
    {
        _proxyHost = host;
        _proxyPort = port;
        UpdateProxy();
    }

    public void ClearProxy()
    {
        _proxyHost = null;
        _proxyPort = null;
        UpdateProxy();
    }

    private void UpdateProxy()
    {
        if (_handler == null) return;
        
        if (!string.IsNullOrEmpty(_proxyHost) && _proxyPort.HasValue)
        {
            _handler.Proxy = new System.Net.WebProxy(_proxyHost, _proxyPort.Value);
            _handler.UseProxy = true;
        }
        else
        {
            _handler.UseProxy = false;
            _handler.Proxy = null;
        }
    }

    private bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _httpClient.Dispose();
        _handler?.Dispose();
    }
}

public static class NetworkServiceExtensions
{
    public static IServiceCollection AddNetworkService(this IServiceCollection services)
    {
        services.AddSingleton<INetworkService, NetworkService>();
        return services;
    }
}
