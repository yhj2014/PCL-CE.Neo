using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Lifecycle;

namespace PCL_CE.Neo.Core.SingleInstance;

/// <summary>
/// 单例服务，确保只有一个启动器实例运行
/// </summary>
public class SingleInstanceService : IService, IAsyncDisposable
{
    private readonly ILogger<SingleInstanceService> _logger;
    private FileStream? _lockStream;
    private NamedPipeServerStream? _pipeServer;
    private CancellationTokenSource? _pipeCts;
    private Task? _pipeListenerTask;
    private bool _disposed;

    private static readonly string LockFileName = "instance.lock";
    private static readonly string PipePrefix = "PCLCE_Neo";

    public string Identifier => "single-instance";
    public string Name => "单例服务";
    public bool SupportAsync => true;

    /// <summary>
    /// 当收到其他实例的参数时触发
    /// </summary>
    public event Action<string[]>? ArgumentsReceived;

    /// <summary>
    /// 当收到激活请求时触发
    /// </summary>
    public event Action? ActivateRequested;

    public SingleInstanceService(ILogger<SingleInstanceService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync()
    {
        _logger.LogInformation("单例服务启动");

        var lockFilePath = GetLockFilePath();

        try
        {
            // 尝试创建单例锁
            _lockStream = File.Open(lockFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            
            _logger.LogDebug("未发现重复实例，正在向单例锁写入信息");
            
            using var sw = new StreamWriter(_lockStream, Encoding.ASCII, 8, true);
            await sw.WriteAsync(Environment.ProcessId.ToString()).ConfigureAwait(false);
            await sw.FlushAsync().ConfigureAwait(false);

            // 启动管道监听器
            StartPipeListener();
            
            _logger.LogInformation("单例服务已启动，进程 ID: {ProcessId}", Environment.ProcessId);
        }
        catch (IOException)
        {
            // 发现重复实例
            HandleDuplicateInstance(lockFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "单例服务启动失败");
        }
    }

    public async Task StopAsync()
    {
        _logger.LogInformation("单例服务停止");

        // 停止管道监听器
        if (_pipeCts != null)
        {
            _pipeCts.Cancel();
            if (_pipeListenerTask != null)
            {
                try
                {
                    await _pipeListenerTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // 忽略取消异常
                }
            }
            _pipeCts.Dispose();
            _pipeCts = null;
        }

        if (_pipeServer != null)
        {
            _pipeServer.Dispose();
            _pipeServer = null;
        }

        // 删除单例锁
        if (_lockStream != null)
        {
            _logger.LogDebug("正在删除单例锁");
            await _lockStream.DisposeAsync().ConfigureAwait(false);
            _lockStream = null;

            var lockFilePath = GetLockFilePath();
            if (File.Exists(lockFilePath))
            {
                try
                {
                    File.Delete(lockFilePath);
                    _logger.LogDebug("单例锁已删除");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "删除单例锁失败");
                }
            }
        }
    }

    /// <summary>
    /// 获取单例锁文件路径
    /// </summary>
    private static string GetLockFilePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var pclPath = Path.Combine(appDataPath, "PCL-CE.Neo");
        Directory.CreateDirectory(pclPath);
        return Path.Combine(pclPath, LockFileName);
    }

    /// <summary>
    /// 处理重复实例
    /// </summary>
    private void HandleDuplicateInstance(string lockFilePath)
    {
        try
        {
            using var stream = File.Open(lockFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            var pid = reader.ReadToEnd();

            _logger.LogInformation("发现重复实例 {ProcessId}，尝试传递参数并拉起主窗口", pid);

            try
            {
                // 尝试通过管道通信
                SendRpcToExistingInstance(pid, "activate");
                
                _logger.LogInformation("已向现有实例发送激活请求");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RPC 通信失败");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取单例锁出错");
        }

        // 退出当前实例
        _logger.LogInformation("当前实例将退出");
        Environment.Exit(1);
    }

    /// <summary>
    /// 向现有实例发送 RPC 消息
    /// </summary>
    private void SendRpcToExistingInstance(string processId, string content)
    {
        var pipeName = $"{PipePrefix}@{processId}";
        using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
        pipe.Connect(1000);
        
        using var sw = new StreamWriter(pipe, Encoding.UTF8);
        sw.WriteLine(content);
        sw.Write('\0'); // 结束符
        sw.Flush();
    }

    /// <summary>
    /// 启动管道监听器
    /// </summary>
    private void StartPipeListener()
    {
        _pipeCts = new CancellationTokenSource();
        _pipeListenerTask = Task.Run(async () =>
        {
            var pipeName = $"{PipePrefix}@{Environment.ProcessId}";
            
            while (!_pipeCts.Token.IsCancellationRequested)
            {
                try
                {
                    _pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                    
                    await _pipeServer.WaitForConnectionAsync(_pipeCts.Token).ConfigureAwait(false);
                    
                    using var reader = new StreamReader(_pipeServer, Encoding.UTF8);
                    var content = await reader.ReadToEndAsync().ConfigureAwait(false);
                    
                    _logger.LogDebug("收到管道消息: {Content}", content);
                    
                    HandlePipeMessage(content);
                    
                    _pipeServer.Dispose();
                    _pipeServer = null;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "管道监听异常");
                }
            }
        }, _pipeCts.Token);
    }

    /// <summary>
    /// 处理管道消息
    /// </summary>
    private void HandlePipeMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        // 处理激活请求
        if (content.Contains("activate"))
        {
            _logger.LogInformation("收到激活请求");
            ActivateRequested?.Invoke();
        }

        // 处理命令行参数
        if (content.Contains("cli"))
        {
            try
            {
                var jsonStart = content.IndexOf('{');
                var jsonEnd = content.LastIndexOf('}');
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var json = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    var args = JsonSerializer.Deserialize<string[]>(json);
                    if (args != null && args.Length > 0)
                    {
                        _logger.LogInformation("收到命令行参数: {Args}", string.Join(" ", args));
                        ArgumentsReceived?.Invoke(args);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "解析命令行参数失败");
            }
        }
    }

    /// <summary>
    /// 检查是否是第一个实例
    /// </summary>
    public bool IsFirstInstance => _lockStream != null;

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await StopAsync().ConfigureAwait(false);
    }
}