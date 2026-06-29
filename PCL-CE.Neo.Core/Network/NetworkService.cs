using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.Network;

public class NetworkService : INetworkService
{
    private readonly HttpClient _httpClient;
    private readonly ILoggerAdapter _logger;

    public NetworkService(ILoggerAdapter logger)
    {
        _logger = logger;
        _httpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10
        })
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("PCL-CE.Neo/1.0");
        _httpClient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");
    }

    public async Task<string> DownloadStringAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Debug($"Downloading string from: {url}");
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.Warn(ex, $"Failed to download string from {url}");
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.Debug($"Download string from {url} was canceled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Unexpected error downloading string from {url}");
            throw;
        }
    }

    public async Task<byte[]> DownloadDataAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Debug($"Downloading data from: {url}");
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.Warn(ex, $"Failed to download data from {url}");
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.Debug($"Download data from {url} was canceled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Unexpected error downloading data from {url}");
            throw;
        }
    }

    public async Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Debug($"GET request to: {url}");
            return await _httpClient.GetAsync(url, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.Warn(ex, $"GET request failed for {url}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Unexpected error in GET request to {url}");
            throw;
        }
    }

    public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Debug($"POST request to: {url}");
            return await _httpClient.PostAsync(url, content, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.Warn(ex, $"POST request failed for {url}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Unexpected error in POST request to {url}");
            throw;
        }
    }

    public async Task<HttpResponseMessage> PutAsync(string url, HttpContent content, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Debug($"PUT request to: {url}");
            return await _httpClient.PutAsync(url, content, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.Warn(ex, $"PUT request failed for {url}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Unexpected error in PUT request to {url}");
            throw;
        }
    }

    public async Task<HttpResponseMessage> DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Debug($"DELETE request to: {url}");
            return await _httpClient.DeleteAsync(url, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.Warn(ex, $"DELETE request failed for {url}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Unexpected error in DELETE request to {url}");
            throw;
        }
    }

    public void SetTimeout(TimeSpan timeout)
    {
        _httpClient.Timeout = timeout;
        _logger.Debug($"HTTP client timeout set to {timeout.TotalSeconds} seconds");
    }

    public void AddHeader(string name, string value)
    {
        if (!_httpClient.DefaultRequestHeaders.Contains(name))
        {
            _httpClient.DefaultRequestHeaders.Add(name, value);
            _logger.Debug($"Added header: {name} = {value}");
        }
    }

    public void RemoveHeader(string name)
    {
        _httpClient.DefaultRequestHeaders.Remove(name);
        _logger.Debug($"Removed header: {name}");
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _logger.Debug("NetworkService disposed");
    }
}