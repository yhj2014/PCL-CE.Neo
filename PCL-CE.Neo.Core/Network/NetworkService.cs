using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.Network;

public static class NetworkService
{
    private static IServiceProvider? _provider;
    private static IHttpClientFactory? _factory;

    public static void Start()
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
    }

    public static void Stop()
    {
        _provider?.Dispose();
    }

    public static HttpClient GetClient(string wantClientType = "default")
    {
        return _factory?.CreateClient(wantClientType) ??
               throw new InvalidOperationException("在初始化完成前的意外调用");
    }

    private const int BaseRetryDelayMs = 1000;
    private const int MaxRetryDelayMs = 30000;

    private static TimeSpan DefaultSleepDurationProvider(int attempt)
    {
        var delayMs = Math.Pow(2, attempt - 1) * BaseRetryDelayMs;
        delayMs = Math.Min(delayMs, MaxRetryDelayMs);
        return TimeSpan.FromMilliseconds(delayMs);
    }

    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> action,
        int retry = 3,
        Func<int, TimeSpan>? retryPolicy = null)
    {
        retryPolicy ??= DefaultSleepDurationProvider;

        for (int attempt = 1; attempt <= retry; attempt++)
        {
            try
            {
                return await action();
            }
            catch (HttpRequestException ex) when (attempt < retry)
            {
                var delay = retryPolicy(attempt);
                LogWrapper.Error(ex, $"HTTP 请求失败，正在进行第 {attempt} 次重试，等待 {delay.TotalMilliseconds} 毫秒。");
                await Task.Delay(delay);
            }
        }

        return await action();
    }
}