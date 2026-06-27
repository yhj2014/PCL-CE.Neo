using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Network;

/// <summary>
/// 网络状态
/// </summary>
public enum NetworkStatus
{
    Connected,
    Disconnected,
    Limited,
    Unknown
}

/// <summary>
/// 网络连接信息
/// </summary>
public class NetworkConnectionInfo
{
    public NetworkStatus Status { get; set; } = NetworkStatus.Unknown;
    public string? NetworkName { get; set; }
    public long BytesReceived { get; set; }
    public long BytesSent { get; set; }
    public long SpeedReceived { get; set; }
    public long SpeedSent { get; set; }
    public DateTime LastChecked { get; set; } = DateTime.Now;
}

/// <summary>
/// 重试策略配置
/// </summary>
public class RetryPolicyConfig
{
    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;
    
    /// <summary>
    /// 重试延迟基数（毫秒）
    /// </summary>
    public int BaseRetryDelayMs { get; set; } = 1000;
    
    /// <summary>
    /// 最大重试延迟（毫秒）
    /// </summary>
    public int MaxRetryDelayMs { get; set; } = 30000;
    
    /// <summary>
    /// 是否使用指数退避
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;
}

/// <summary>
/// 网络服务接口
/// </summary>
public interface INetworkService
{
    HttpClient HttpClient { get; }
    
    /// <summary>
    /// 获取网络连接状态
    /// </summary>
    NetworkStatus NetworkStatus { get; }
    
    /// <summary>
    /// 网络连接信息
    /// </summary>
    NetworkConnectionInfo ConnectionInfo { get; }
    
    /// <summary>
    /// 检查网络连接
    /// </summary>
    Task<bool> CheckConnectionAsync(string? testUrl = null);
    
    /// <summary>
    /// 获取字符串内容
    /// </summary>
    Task<string> GetStringAsync(string url, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取字节数据
    /// </summary>
    Task<byte[]> GetBytesAsync(string url, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取 JSON 数据并反序列化
    /// </summary>
    Task<T?> GetJsonAsync<T>(string url, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 发送 POST 请求
    /// </summary>
    Task<string> PostStringAsync(string url, string content, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 发送 JSON POST 请求
    /// </summary>
    Task<T?> PostJsonAsync<T>(string url, object data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 设置代理
    /// </summary>
    void SetProxy(string? host, int? port);
    
    /// <summary>
    /// 清除代理
    /// </summary>
    void ClearProxy();
    
    /// <summary>
    /// 使用重试策略发送请求
    /// </summary>
    Task<T> SendWithRetryAsync<T>(Func<Task<T>> action, RetryPolicyConfig? config = null);
    
    /// <summary>
    /// 网络状态改变事件
    /// </summary>
    event Action<NetworkStatus>? NetworkStatusChanged;
}

/// <summary>
/// 网络服务实现
/// </summary>
public sealed class NetworkService : INetworkService, IDisposable
{
    private readonly ILogger<NetworkService> _logger;
    private readonly HttpClient _httpClient;
    private readonly HttpClientHandler? _handler;
    private readonly ConcurrentQueue<(DateTime Time, long Bytes)> _receivedHistory = new();
    private readonly ConcurrentQueue<(DateTime Time, long Bytes)> _sentHistory = new();
    private readonly Timer _networkCheckTimer;
    private readonly Timer _speedUpdateTimer;
    private readonly SemaphoreSlim _requestLock = new(1, 1);
    
    private string? _proxyHost;
    private int? _proxyPort;
    private NetworkStatus _networkStatus = NetworkStatus.Unknown;
    private NetworkConnectionInfo _connectionInfo = new();
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private const string DefaultTestUrl = "https://www.google.com/favicon.ico";

    public event Action<NetworkStatus>? NetworkStatusChanged;

    public HttpClient HttpClient => _httpClient;
    
    public NetworkStatus NetworkStatus => _networkStatus;
    
    public NetworkConnectionInfo ConnectionInfo => _connectionInfo;

    public NetworkService(ILogger<NetworkService> logger)
    {
        _logger = logger;
        
        _handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            UseCookies = true,
            CookieContainer = new CookieContainer(),
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 20
        };
        
        _httpClient = new HttpClient(_handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "PCL-CE.Neo/2.0");
        
        // 启动网络状态检测定时器
        _networkCheckTimer = new Timer(async _ => await CheckNetworkStatusAsync(), null, 
            TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        
        // 启动速度更新定时器
        _speedUpdateTimer = new Timer(_ => UpdateNetworkSpeed(), null,
            TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        
        _logger.LogInformation("网络服务已初始化");
    }

    public NetworkService() : this(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<NetworkService>.Instance)
    {
    }

    /// <summary>
    /// 检查网络连接状态
    /// </summary>
    public async Task<bool> CheckConnectionAsync(string? testUrl = null)
    {
        var url = testUrl ?? DefaultTestUrl;
        
        try
        {
            var ping = new Ping();
            var reply = await ping.SendPingAsync("8.8.8.8", 3000);
            
            if (reply.Status != IPStatus.Success)
            {
                _logger.LogWarning("Ping 失败: {Status}", reply.Status);
                UpdateNetworkStatus(NetworkStatus.Disconnected);
                return false;
            }
            
            // 尝试 HTTP 请求
            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            
            if (response.IsSuccessStatusCode)
            {
                UpdateNetworkStatus(NetworkStatus.Connected);
                return true;
            }
            
            _logger.LogWarning("HTTP 检测失败: {StatusCode}", response.StatusCode);
            UpdateNetworkStatus(NetworkStatus.Limited);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "网络连接检查失败");
            UpdateNetworkStatus(NetworkStatus.Disconnected);
            return false;
        }
    }

    private async Task CheckNetworkStatusAsync()
    {
        await CheckConnectionAsync();
    }

    private void UpdateNetworkStatus(NetworkStatus status)
    {
        if (_networkStatus != status)
        {
            _networkStatus = status;
            _connectionInfo.Status = status;
            _connectionInfo.LastChecked = DateTime.Now;
            
            NetworkStatusChanged?.Invoke(status);
            _logger.LogInformation("网络状态改变: {Status}", status);
        }
    }

    private void UpdateNetworkSpeed()
    {
        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up && 
                            ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .ToList();

            if (interfaces.Count == 0)
            {
                _connectionInfo.SpeedReceived = 0;
                _connectionInfo.SpeedSent = 0;
                return;
            }

            var totalReceived = interfaces.Sum(ni => ni.GetIPStatistics().BytesReceived);
            var totalSent = interfaces.Sum(ni => ni.GetIPStatistics().BytesSent);

            // 记录历史数据
            _receivedHistory.Enqueue((DateTime.Now, totalReceived));
            _sentHistory.Enqueue((DateTime.Now, totalSent));

            // 清理旧数据（只保留最近 10 秒）
            while (_receivedHistory.Count > 10)
                _receivedHistory.TryDequeue(out _);
            while (_sentHistory.Count > 10)
                _sentHistory.TryDequeue(out _);

            // 计算速度
            if (_receivedHistory.TryPeek(out var oldReceived) && _sentHistory.TryPeek(out var oldSent))
            {
                var timeDiff = DateTime.Now - oldReceived.Time;
                if (timeDiff.TotalSeconds > 0)
                {
                    _connectionInfo.SpeedReceived = (long)((totalReceived - oldReceived.Bytes) / timeDiff.TotalSeconds);
                    _connectionInfo.SpeedSent = (long)((totalSent - oldSent.Bytes) / timeDiff.TotalSeconds);
                    _connectionInfo.BytesReceived = totalReceived;
                    _connectionInfo.BytesSent = totalSent;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "更新网络速度时出错");
        }
    }

    /// <summary>
    /// 获取字符串内容
    /// </summary>
    public async Task<string> GetStringAsync(string url, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("GET {Url}", url);
        
        await _requestLock.WaitAsync(cancellationToken);
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "GET 请求失败: {Url}", url);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "GET 请求超时: {Url}", url);
            throw new TimeoutException($"Request to {url} timed out", ex);
        }
        finally
        {
            _requestLock.Release();
        }
    }

    /// <summary>
    /// 获取字节数据
    /// </summary>
    public async Task<byte[]> GetBytesAsync(string url, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("GET bytes from {Url}", url);
        
        await _requestLock.WaitAsync(cancellationToken);
        try
        {
            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "GET bytes 请求失败: {Url}", url);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "GET bytes 请求超时: {Url}", url);
            throw new TimeoutException($"Request to {url} timed out", ex);
        }
        finally
        {
            _requestLock.Release();
        }
    }

    /// <summary>
    /// 获取 JSON 数据并反序列化
    /// </summary>
    public async Task<T?> GetJsonAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        var json = await GetStringAsync(url, cancellationToken);
        
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON 解析失败: {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// 发送 POST 请求
    /// </summary>
    public async Task<string> PostStringAsync(string url, string content, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("POST to {Url}", url);
        
        await _requestLock.WaitAsync(cancellationToken);
        try
        {
            var httpContent = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, httpContent, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "POST 请求失败: {Url}", url);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "POST 请求超时: {Url}", url);
            throw new TimeoutException($"Request to {url} timed out", ex);
        }
        finally
        {
            _requestLock.Release();
        }
    }

    /// <summary>
    /// 发送 JSON POST 请求
    /// </summary>
    public async Task<T?> PostJsonAsync<T>(string url, object data, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        var response = await PostStringAsync(url, json, cancellationToken);
        
        try
        {
            return JsonSerializer.Deserialize<T>(response, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON 解析失败: {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// 使用重试策略发送请求
    /// </summary>
    public async Task<T> SendWithRetryAsync<T>(Func<Task<T>> action, RetryPolicyConfig? config = null)
    {
        config ??= new RetryPolicyConfig();
        
        var retryCount = 0;
        
        while (retryCount <= config.MaxRetryCount)
        {
            try
            {
                return await action();
            }
            catch (HttpRequestException ex)
            {
                retryCount++;
                
                if (retryCount > config.MaxRetryCount)
                {
                    _logger.LogError(ex, "请求失败，超过最大重试次数 ({MaxRetry})", config.MaxRetryCount);
                    throw;
                }
                
                var delayMs = config.UseExponentialBackoff
                    ? Math.Min((int)Math.Pow(2, retryCount) * config.BaseRetryDelayMs, config.MaxRetryDelayMs)
                    : config.BaseRetryDelayMs;
                
                _logger.LogWarning(ex, "请求失败，第 {Retry} 次重试，等待 {Delay}ms", retryCount, delayMs);
                
                await Task.Delay(delayMs);
            }
            catch (Exception ex) when (ex is not HttpRequestException)
            {
                _logger.LogError(ex, "请求出现非网络错误");
                throw;
            }
        }
        
        throw new InvalidOperationException("Unexpected retry loop exit");
    }

    /// <summary>
    /// 设置代理
    /// </summary>
    public void SetProxy(string? host, int? port)
    {
        _proxyHost = host;
        _proxyPort = port;
        UpdateProxy();
        
        _logger.LogInformation("代理已设置: {Host}:{Port}", host, port);
    }

    /// <summary>
    /// 清除代理
    /// </summary>
    public void ClearProxy()
    {
        _proxyHost = null;
        _proxyPort = null;
        UpdateProxy();
        
        _logger.LogInformation("代理已清除");
    }

    private void UpdateProxy()
    {
        if (_handler == null) return;
        
        if (!string.IsNullOrEmpty(_proxyHost) && _proxyPort.HasValue)
        {
            _handler.Proxy = new WebProxy(_proxyHost, _proxyPort.Value);
            _handler.UseProxy = true;
        }
        else
        {
            _handler.UseProxy = false;
            _handler.Proxy = null;
        }
    }

    public void Dispose()
    {
        _networkCheckTimer.Dispose();
        _speedUpdateTimer.Dispose();
        _requestLock.Dispose();
        _httpClient.Dispose();
        _handler?.Dispose();
        
        _logger.LogInformation("网络服务已释放");
    }
}