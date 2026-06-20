using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace PCL_CE.Neo.Core.Network;

public sealed class NetworkService : IDisposable
{
    private readonly ILogger<NetworkService> _logger;
    private ServiceProvider? _provider;
    private IHttpClientFactory? _factory;
    private bool _disposed;

    public NetworkService(ILogger<NetworkService> logger)
    {
        _logger = logger;
    }

    public void Initialize()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("default")
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                UseProxy = true,
                AutomaticDecompression = DecompressionMethods.All,
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 20,
                UseCookies = false
            });

        _provider = services.BuildServiceProvider();
        _factory = _provider.GetRequiredService<IHttpClientFactory>();
        _logger.LogInformation("网络服务初始化完成");
    }

    public HttpClient GetClient(string wantClientType = "default")
    {
        return _factory?.CreateClient(wantClientType) ??
               throw new InvalidOperationException("在初始化完成前的意外调用");
    }

    private const int BaseRetryDelayMs = 1000;
    private const int MaxRetryDelayMs = 30000;

    private static TimeSpan _DefaultSleepDurationProvider(int attempt)
    {
        var delayMs = Math.Pow(2, attempt - 1) * BaseRetryDelayMs;
        delayMs = Math.Min(delayMs, MaxRetryDelayMs);
        return TimeSpan.FromMilliseconds(delayMs);
    }

    public AsyncPolicy GetRetryPolicy(int retry = 3, Func<int, TimeSpan>? retryPolicy = null)
    {
        retryPolicy ??= _DefaultSleepDurationProvider;

        return Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(
                retry,
                attempt => retryPolicy.Invoke(attempt),
                onRetry: (exception, timeSpan, retryAttempt, context) =>
                {
                    _logger.LogError(exception,
                        $"HTTP 请求失败，正在进行第 {retryAttempt} 次重试，等待 {timeSpan.TotalMilliseconds} 毫秒。");
                });
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _provider?.Dispose();
        _logger.LogDebug("网络服务已关闭");
    }
}