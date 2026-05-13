using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class NetworkAdapter : INetworkAdapter
{
    private readonly ILogger<NetworkAdapter> _logger;
    private readonly IPathsAdapter _pathsAdapter;
    private HttpClient? _httpClient;
    private HttpClientHandler? _handler;
    private bool _doHEnabled = true;
    private readonly object _lock = new();

    public event Action<NetworkLogEntry>? LogReceived;

    public NetworkAdapter(ILogger<NetworkAdapter> logger, IPathsAdapter pathsAdapter)
    {
        _logger = logger;
        _pathsAdapter = pathsAdapter;
        InitializeHttpClient();
    }

    public async Task<string> GetAsync(string url, Dictionary<string, string>? headers = null)
    {
        var bytes = await GetBytesAsync(url, headers);
        return Encoding.UTF8.GetString(bytes);
    }

    public async Task<byte[]> GetBytesAsync(string url, Dictionary<string, string>? headers = null)
    {
        var response = await SendRequestAsync(HttpMethod.Get, url, null, headers);
        return response.Body;
    }

    public async Task<HttpResponse> PostAsync(string url, string? body = null, Dictionary<string, string>? headers = null)
    {
        return await SendRequestAsync(HttpMethod.Post, url, body, headers);
    }

    public void SetProxy(string? address, int? port = null, string? username = null, string? password = null)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(address))
            {
                _handler!.Proxy = null;
                _handler!.UseDefaultCredentials = true;
            }
            else
            {
                var proxyUri = new Uri($"{(port.HasValue ? $"{address}:{port}" : address)}");
                var proxy = new WebProxy(proxyUri);

                if (!string.IsNullOrEmpty(username))
                {
                    proxy.Credentials = new NetworkCredential(username, password);
                }

                _handler!.Proxy = proxy;
            }

            _httpClient?.Dispose();
            _httpClient = new HttpClient(_handler!);
            _logger.LogInformation("代理设置已更新: {Proxy}", address ?? "无");
        }
    }

    public void EnableDoH(bool enabled)
    {
        _doHEnabled = enabled;
        _logger.LogInformation("DoH 设置已更新: {Enabled}", enabled);
    }

    // public IWebServer CreateWebServer(int port)
    // {
    //     return new SimpleWebServer(port, _logger);
    // }

    private void InitializeHttpClient()
    {
        _handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10
        };

        // if (_doHEnabled)
        // {
        //     ConfigureDoH();
        // }

        _httpClient = new HttpClient(_handler);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "PCL-CE.Neo/1.0");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    // private void ConfigureDoH()
    // {
    //     try
    //     {
    //         var dohUri = new Uri("https://dns.google/dns-query");
    //         _handler!.Dns = new System.Net.Http.DohHttpClientHandler(dohUri);
    //         _logger.LogDebug("DoH 已启用: {Provider}", dohUri);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogWarning(ex, "DoH 配置失败，使用默认 DNS");
    //     }
    // }

    private async Task<HttpResponse> SendRequestAsync(
        HttpMethod method,
        string url,
        string? body,
        Dictionary<string, string>? headers)
    {
        var startTime = DateTime.Now;
        HttpResponseMessage? response = null;
        Exception? error = null;

        try
        {
            var request = new HttpRequestMessage(method, url);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (!string.IsNullOrEmpty(body))
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            response = await _httpClient!.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseHeaders = new Dictionary<string, string>();
            foreach (var header in response.Headers)
            {
                responseHeaders[header.Key] = string.Join(", ", header.Value);
            }
            foreach (var header in response.Content.Headers)
            {
                responseHeaders[header.Key] = string.Join(", ", header.Value);
            }

            var bodyBytes = await response.Content.ReadAsByteArrayAsync();
            var elapsed = (long)(DateTime.Now - startTime).TotalMilliseconds;

            LogReceived?.Invoke(new NetworkLogEntry
            {
                Timestamp = startTime,
                Method = method.Method,
                Url = url,
                StatusCode = (int)response.StatusCode,
                ElapsedMs = elapsed
            });

            return new HttpResponse
            {
                StatusCode = (int)response.StatusCode,
                ContentType = response.Content.Headers.ContentType?.MediaType,
                Body = bodyBytes,
                Headers = responseHeaders
            };
        }
        catch (Exception ex)
        {
            error = ex;
            var elapsed = (long)(DateTime.Now - startTime).TotalMilliseconds;

            LogReceived?.Invoke(new NetworkLogEntry
            {
                Timestamp = startTime,
                Method = method.Method,
                Url = url,
                Error = ex.Message,
                ElapsedMs = elapsed
            });

            return new HttpResponse
            {
                StatusCode = 0,
                Body = [],
                Headers = new Dictionary<string, string>(),
                ErrorMessage = ex.Message
            };
        }
        finally
        {
            response?.Dispose();
        }
    }
}

// public class SimpleWebServer : IWebServer
// {
//     private readonly ILogger _logger;
//     private readonly HttpListener _listener;
//     private CancellationTokenSource? _cts;
//     private Task? _runningTask;
//
//     public int Port { get; }
//     public bool IsRunning => _runningTask != null && !_runningTask.IsCompleted;
//
//     public event Action<HttpListenerRequest, Action<HttpListenerResponse>>? RequestReceived;
//
//     public SimpleWebServer(int port, ILogger logger)
//     {
//         Port = port;
//         _logger = logger;
//         _listener = new HttpListener();
//         _listener.Prefixes.Add($"http://localhost:{port}/");
//     }
//
//     public Task StartAsync()
//     {
//         _listener.Start();
//         _cts = new CancellationTokenSource();
//         _runningTask = ListenAsync(_cts.Token);
//         _logger.LogInformation("Web 服务器已启动，端口: {Port}", Port);
//         return Task.CompletedTask;
//     }
//
//     public void Stop()
//     {
//         _cts?.Cancel();
//         _listener.Stop();
//         _logger.LogInformation("Web 服务器已停止");
//     }
//
//     private async Task ListenAsync(CancellationToken token)
//     {
//         while (!token.IsCancellationRequested && _listener.IsListening)
//         {
//             try
//             {
//                 var context = await _listener.GetContextAsync();
//                 _ = Task.Run(() => HandleContext(context), token);
//             }
//             catch (HttpListenerException) when (token.IsCancellationRequested)
//             {
//                 break;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Web 服务器监听异常");
//             }
//         }
//     }
//
//     private void HandleContext(HttpListenerContext context)
//     {
//         var request = context.Request;
//         var response = context.Response;
//
//         RequestReceived?.Invoke(
//             new SimpleHttpListenerRequest(request),
//             r =>
//             {
//                 response.StatusCode = r.StatusCode;
//                 response.ContentType = r.ContentType;
//                 var buffer = Encoding.UTF8.GetBytes(r.Body ?? "");
//                 response.ContentLength64 = buffer.Length;
//                 response.OutputStream.Write(buffer);
//                 response.Close();
//             });
//     }
//
//     public void Dispose()
//     {
//         Stop();
//         _listener.Close();
//         _cts?.Dispose();
//     }
// }
//
// public class SimpleHttpListenerRequest
// {
//     private readonly HttpListenerRequest _request;
//
//     public string HttpMethod => _request.HttpMethod;
//     public string RawUrl => _request.RawUrl ?? "";
//     public Uri? Url => _request.Url;
//
//     public SimpleHttpListenerRequest(HttpListenerRequest request)
//     {
//         _request = request;
//     }
// }
