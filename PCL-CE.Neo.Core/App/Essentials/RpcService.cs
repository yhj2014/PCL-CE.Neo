using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.App;

namespace PCL_CE.Neo.Core.App.Essentials;

public sealed class RpcService : IDisposable
{
    private readonly ILogger<RpcService> _logger;
    private NamedPipeServerStream? _pipe;
    private bool _disposed;

    public const string PipePrefix = "PCLCE_RPC";
    private readonly string _EchoPipeName = $"{PipePrefix}@{Basics.CurrentProcessId}";
    private static readonly string[] _RequestTypeArray = ["GET", "SET", "REQ"];
    private static readonly HashSet<string> _RequestType = [.._RequestTypeArray];

    private static readonly Dictionary<string, RpcProperty> _PropertyMap = [];
    private static readonly Dictionary<string, RpcFunction> _FunctionMap = new() {
        ["ping"] = ((_, _, _) => RpcResponse.EmptySuccess),
        ["activate"] = ((_, _, _) =>
        {
            return RpcResponse.EmptySuccess;
        })
    };

    public RpcService(ILogger<RpcService> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync()
    {
        try
        {
            _pipe = new NamedPipeServerStream(_EchoPipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances);
            _logger.LogDebug("RPC server started on pipe: {PipeName}", _EchoPipeName);
            
            while (!_disposed)
            {
                try
                {
                    await _pipe.WaitForConnectionAsync();
                    _ = Task.Run(async () => await _HandleClientAsync(_pipe));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error accepting RPC connection");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start RPC service");
        }
    }

    private async Task _HandleClientAsync(NamedPipeServerStream pipe)
    {
        try
        {
            using var reader = new StreamReader(pipe, Encoding.UTF8);
            using var writer = new StreamWriter(pipe, Encoding.UTF8);
            
            var header = await reader.ReadLineAsync();
            _logger.LogInformation("Client request: {Header}", header);

            var args = header?.Split([' '], 2) ?? [];
            if (args.Length < 2 || args[1].Length == 0)
            {
                RpcResponse.Err("请求参数过少").Response(writer);
                return;
            }

            var type = args[0].ToUpperInvariant();
            if (!_RequestType.Contains(type))
            {
                RpcResponse.Err($"请求类型必须为 {string.Join("/", _RequestTypeArray)} 其中之一").Response(writer);
                return;
            }

            var target = args[1];
            var buffer = new StringBuilder();
            int tmp;
            while ((tmp = reader.Read()) != -1 && tmp != '\0')
            {
                buffer.Append((char)tmp);
            }
            var content = buffer.Length == 0 ? null : buffer.ToString();

            switch (type)
            {
                case "GET":
                case "SET":
                    _HandlePropertyRequest(type, target, content, writer);
                    break;
                case "REQ":
                    _HandleFunctionRequest(target, content, writer);
                    break;
            }
        }
        catch (RpcException rpcEx)
        {
            RpcResponse.Err(rpcEx.Reason).Response(new StreamWriter(pipe, Encoding.UTF8));
            _logger.LogInformation("RPC error: {Reason}", rpcEx.Reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RPC request");
        }
        finally
        {
            if (pipe.IsConnected)
            {
                pipe.Disconnect();
            }
        }
    }

    private void _HandlePropertyRequest(string type, string target, string? content, StreamWriter writer)
    {
        target = target.ToLowerInvariant();
        var result = _PropertyMap.TryGetValue(target, out var prop);
        if (!result) throw new RpcException($"不存在属性 {target}");

        RpcResponse response;
        if (type == "GET")
        {
            try
            {
                var value = prop!.Value;
                response = new RpcResponse(RpcResponseStatus.Success, RpcResponseType.Text, value, target);
                _logger.LogTrace("返回值: {Value}", value);
            }
            catch (RpcPropertyOperationFailedException)
            {
                response = RpcResponse.EmptyFailure;
                _logger.LogDebug("设置失败: 只写属性或请求被拒绝");
            }
        }
        else if (prop!.Settable)
        {
            try
            {
                prop.Value = content;
                response = RpcResponse.EmptySuccess;
                _logger.LogTrace("设置成功: {Content}", content);
            }
            catch (RpcPropertyOperationFailedException)
            {
                response = RpcResponse.EmptyFailure;
                _logger.LogDebug("设置失败: 请求被拒绝");
            }
        }
        else
        {
            response = RpcResponse.EmptyFailure;
            _logger.LogDebug("设置失败: 只读属性");
        }
        response.Response(writer);
    }

    private void _HandleFunctionRequest(string target, string? content, StreamWriter writer)
    {
        var targetArgs = target.Split([' '], 2);
        var name = targetArgs[0].ToLowerInvariant();
        var indent = false;
        if (name.EndsWith('$'))
        {
            indent = true;
            name = name[..^1];
        }

        var result = _FunctionMap.TryGetValue(name, out var func);
        if (!result) throw new RpcException($"不存在函数 {name}");

        string? argument = null;
        if (targetArgs.Length > 1)
            argument = targetArgs[1];

        _logger.LogTrace("正在调用函数 {Name} {Argument}", name, argument);
        var response = func!(argument, content, indent);
        response.Response(writer);
        _logger.LogTrace("函数已退出，返回状态 {Status}", response.Status);
    }

    public static bool AddProperty(RpcProperty prop) => _PropertyMap.TryAdd(prop.Name, prop);
    public static bool RemoveProperty(string name) => _PropertyMap.Remove(name);
    public static bool AddFunction(string name, RpcFunction func) => _FunctionMap.TryAdd(name, func);
    public static bool RemoveFunction(string name) => _FunctionMap.Remove(name);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _pipe?.Dispose();
    }
}