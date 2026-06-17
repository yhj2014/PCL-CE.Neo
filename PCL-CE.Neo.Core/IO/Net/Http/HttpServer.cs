using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.IO.Net.Http;

public abstract class HttpServer : IDisposable
{
    private readonly HttpListener _server = new();
    public readonly ushort Port;
    public readonly string[] Host;

    private Task? _handleLoop;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly Dictionary<(HttpMethod method, string path), Func<HttpListenerRequest, Task<HttpRouteResponse>>> _handlers = new();
    private bool _initialized = false;

    protected HttpServer(IPAddress[] listenAddr, ushort port = 0)
    {
        ArgumentNullException.ThrowIfNull(listenAddr);

        if (port == 0) port = (ushort)NetworkHelper.NewTcpPort();
        Port = port;

        if (listenAddr.Length == 0)
            listenAddr = [IPAddress.Loopback, IPAddress.IPv6Loopback];

        var hosts = new List<string>();
        foreach (var address in listenAddr)
        {
            _server.Prefixes.Add($"http://{address}:{port}/");
            hosts.Add(address.ToString());
        }
        Host = hosts.ToArray();
    }

    protected abstract void Init();

    protected void Register(HttpMethod method, string path, Func<HttpListenerRequest, Task<HttpRouteResponse>> handler)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(handler);

        _handlers[(method, path)] = handler;
    }

    public void Start()
    {
        if (!_initialized && _handlers.Count == 0)
        {
            Init();
            _initialized = true;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _server.Start();
        _handleLoop = _HandleRequest();
    }

    private async Task _HandleRequest()
    {
        var cancellationToken = _cancellationTokenSource?.Token ?? CancellationToken.None;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var context = await _server.GetContextAsync();
                _ = Task.Run(async () => await _ProcessRequest(context), cancellationToken);
            }
            catch (OperationCanceledException) { break; }
            catch (ObjectDisposedException) { break; }
            catch (HttpListenerException) { break; }
        }
    }

    private async Task _ProcessRequest(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;
            var response = context.Response;
            var path = request.Url?.AbsolutePath ?? string.Empty;
            var method = new HttpMethod(request.HttpMethod);

            if (_handlers.TryGetValue((method, path), out var handler))
            {
                await _ExecuteHandler(handler, request, response);
                return;
            }

            if (_handlers.TryGetValue((method, "*"), out var wildcardHandler))
            {
                await _ExecuteHandler(wildcardHandler, request, response);
                return;
            }

            response.StatusCode = (int)HttpStatusCode.NotFound;
        }
        finally
        {
            try
            {
                context.Response.Close();
            }
            catch
            {
            }
        }
    }

    private static async Task _ExecuteHandler(Func<HttpListenerRequest, Task<HttpRouteResponse>> handler, HttpListenerRequest request, HttpListenerResponse response)
    {
        try
        {
            var routeResponse = await handler(request);
            routeResponse.Pour(response);
        }
        catch (Exception ex)
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.ContentEncoding = System.Text.Encoding.UTF8;
            response.ContentType = "text/plain";
            var errorResponse =
                HttpRouteResponse.Text($"Internal Server Error:\n{ex}", "text/plain", System.Text.Encoding.UTF8);
            errorResponse.Pour(response);
        }
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        _server.Stop();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Stop();
        _server.Close();
        _cancellationTokenSource?.Dispose();
    }
}