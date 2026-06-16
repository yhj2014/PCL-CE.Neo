using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using PCL_CE.Neo.Core.Lifecycle;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.App.Essentials;

public class RpcException(string reason) : Exception
{
    public string Reason => reason;
}

public sealed class RegisterRpcAttribute(string name) : Attribute
{
    public string Name => name;
}

public sealed class RpcService : ServiceBase
{
    private NamedPipeServerStream? _pipe;
    private Thread? _pipeThread;

    public override string Identifier => "rpc";
    public override string Name => "远程执行服务";

    public RpcService(IServiceProvider services) : base(services)
    {
    }

    public const string PipePrefix = "PCLCE_RPC";
    private static readonly string _EchoPipeName = $"{PipePrefix}@{Basics.CurrentProcessId}";
    private static readonly string[] _RequestTypeArray = ["GET", "SET", "REQ"];
    private static readonly HashSet<string> _RequestType = [.._RequestTypeArray];

    private static readonly Dictionary<string, RpcProperty> _PropertyMap = new();
    private static readonly Dictionary<string, RpcFunction> _FunctionMap = new() {
        ["ping"] = ((_, _, _) => RpcResponse.EmptySuccess),
        ["activate"] = ((_, _, _) =>
        {
            LogWrapper.Info("RpcService", "激活主窗口请求");
            return RpcResponse.EmptySuccess;
        })
    };

    public static bool AddProperty(RpcProperty prop) => _PropertyMap.TryAdd(prop.Name, prop);
    public static bool RemoveProperty(string name) => _PropertyMap.Remove(name);
    public static bool RemoveProperty(RpcProperty prop)
    {
        var key = prop.Name;
        var result = _PropertyMap.TryGetValue(key, out var value);
        if (!result || value != prop) return false;
        _PropertyMap.Remove(key);
        return true;
    }

    public static bool AddFunction(string name, RpcFunction func) => _FunctionMap.TryAdd(name, func);
    public static bool RemoveFunction(string name) => _FunctionMap.Remove(name);

    public override Task StartAsync()
    {
        LogWrapper.Info("RpcService", "启动 RPC 服务");
        _pipeThread = new Thread(_PipeServerThread)
        {
            Name = "RpcPipeServer",
            IsBackground = true
        };
        _pipeThread.Start();
        return Task.CompletedTask;
    }

    public override async Task StopAsync()
    {
        LogWrapper.Info("RpcService", "停止 RPC 服务");
        if (_pipe != null)
        {
            try
            {
                _pipe.Close();
                await _pipe.DisposeAsync();
            }
            catch (Exception ex)
            {
                LogWrapper.Warn(ex, "RpcService", "关闭管道时出错");
            }
            _pipe = null;
        }
        if (_pipeThread != null && _pipeThread.IsAlive)
        {
            _pipeThread.Join(3000);
        }
    }

    private void _PipeServerThread()
    {
        try
        {
            while (true)
            {
                using var pipe = new NamedPipeServerStream(
                    _EchoPipeName,
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous | PipeOptions.WriteThrough);

                _pipe = pipe;
                LogWrapper.Trace("RpcService", "等待客户端连接...");
                pipe.WaitForConnection();
                LogWrapper.Trace("RpcService", "客户端已连接");

                try
                {
                    using var reader = new StreamReader(pipe, Encoding.UTF8);
                    using var writer = new StreamWriter(pipe, Encoding.UTF8) { AutoFlush = true };

                    while (pipe.IsConnected)
                    {
                        if (!_EchoPipeCallback(reader, writer, null))
                            break;
                    }
                }
                catch (IOException)
                {
                    LogWrapper.Trace("RpcService", "客户端断开连接");
                }
                catch (Exception ex)
                {
                    LogWrapper.Error(ex, "RpcService", "处理客户端请求时出错");
                }
            }
        }
        catch (ObjectDisposedException)
        {
            LogWrapper.Trace("RpcService", "管道服务已关闭");
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "RpcService", "管道服务线程异常");
        }
    }

    private static bool _EchoPipeCallback(StreamReader reader, StreamWriter writer, Process? client)
    {
        try
        {
            var header = reader.ReadLine();
            if (header == null) return false;

            LogWrapper.Info("RpcService", $"客户端请求: {header}");

            var args = header.Split([' '], 2);
            if (args.Length < 2 || args[1].Length == 0) throw new RpcException("请求参数过少");
            var type = args[0].ToUpperInvariant();
            if (!_RequestType.Contains(type)) throw new RpcException($"请求类型必须为 {string.Join("/", _RequestTypeArray)} 其中之一");
            var target = args[1];

            var buffer = new StringBuilder();
            var tmp = reader.Read();
            while (tmp != -1 && tmp != '\0')
            {
                buffer.Append((char)tmp);
                tmp = reader.Read();
            }
            var content = buffer.Length == 0 ? null : buffer.ToString();

            switch (type)
            {
                case "GET":
                case "SET":
                    target = target.ToLowerInvariant();
                    var propResult = _PropertyMap.TryGetValue(target, out var prop);
                    if (!propResult) throw new RpcException($"不存在属性 {target}");
                    RpcResponse propResponse;
                    if (type == "GET")
                    {
                        try
                        {
                            var value = prop!.Value;
                            propResponse = new RpcResponse(RpcResponseStatus.Success, RpcResponseType.Text, value, target);
                            LogWrapper.Trace("RpcService", $"返回值: {value}");
                        }
                        catch (RpcPropertyOperationFailedException)
                        {
                            propResponse = RpcResponse.EmptyFailure;
                            LogWrapper.Debug("RpcService", "获取失败: 只写属性或请求被拒绝");
                        }
                    }
                    else if (prop!.Settable)
                    {
                        try
                        {
                            prop.Value = content;
                            propResponse = RpcResponse.EmptySuccess;
                            LogWrapper.Trace("RpcService", $"设置成功: {content}");
                        }
                        catch (RpcPropertyOperationFailedException)
                        {
                            propResponse = RpcResponse.EmptyFailure;
                            LogWrapper.Debug("RpcService", "设置失败: 请求被拒绝");
                        }
                    }
                    else
                    {
                        propResponse = RpcResponse.EmptyFailure;
                        LogWrapper.Debug("RpcService", "设置失败: 只读属性");
                    }
                    propResponse.Response(writer);
                    break;

                case "REQ":
                    var targetArgs = target.Split([' '], 2);
                    var funcName = targetArgs[0].ToLowerInvariant();
                    var indent = false;
                    if (funcName.EndsWith('$'))
                    {
                        indent = true;
                        funcName = funcName[..^1];
                    }
                    var funcResult = _FunctionMap.TryGetValue(funcName, out var func);
                    if (!funcResult) throw new RpcException($"不存在函数 {funcName}");
                    string? argument = targetArgs.Length > 1 ? targetArgs[1] : null;
                    LogWrapper.Trace("RpcService", $"正在调用函数 {funcName} {argument}");
                    var funcResponse = func!(argument, content, indent);
                    funcResponse.Response(writer);
                    LogWrapper.Trace("RpcService", $"函数已退出，返回状态 {funcResponse.Status}");
                    break;
            }
        }
        catch (RpcException rpcEx)
        {
            var reason = rpcEx.Reason;
            RpcResponse.Err(reason).Response(writer);
            LogWrapper.Info("RpcService", $"出错: {reason}");
        }
        catch (Exception ex)
        {
            RpcResponse.Err(ex.ToString(), "stacktrace").Response(writer);
            LogWrapper.Error(ex, "RpcService", "处理请求时发生异常");
        }
        return true;
    }
}