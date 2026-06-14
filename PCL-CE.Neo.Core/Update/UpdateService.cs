using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Lifecycle;

namespace PCL_CE.Neo.Core.Update;

/// <summary>
/// 更新服务，负责启动器自动更新
/// </summary>
public class UpdateService : IService, IAsyncDisposable
{
    private readonly ILogger<UpdateService> _logger;
    private bool _disposed;

    public string Identifier => "update";
    public string Name => "更新服务";
    public bool SupportAsync => true;

    public UpdateService(ILogger<UpdateService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync()
    {
        _logger.LogInformation("更新服务启动");

        // 检查命令行参数中是否有更新相关参数
        var args = Environment.GetCommandLineArgs();
        
        if (args.Length >= 3)
        {
            var command = args[1];
            
            switch (command)
            {
                case "update":
                    await HandleUpdateCommandAsync(args);
                    break;
                    
                case "update_finished":
                    HandleUpdateFinishedCommand(args);
                    break;
                    
                case "update_failed":
                    HandleUpdateFailedCommand(args);
                    break;
                    
                default:
                    _logger.LogDebug("无更新任务");
                    break;
            }
        }
        else
        {
            _logger.LogDebug("无更新任务");
        }
    }

    public Task StopAsync()
    {
        _logger.LogInformation("更新服务停止");
        return Task.CompletedTask;
    }

    /// <summary>
    /// 处理更新命令
    /// </summary>
    private async Task HandleUpdateCommandAsync(string[] args)
    {
        if (args.Length < 5)
        {
            _logger.LogWarning("更新参数不完整");
            return;
        }

        try
        {
            _logger.LogInformation("开始更新");

            var oldProcessId = int.Parse(args[2]);
            var target = args[3];
            var source = args[4];
            var restart = args.Length >= 6 && bool.Parse(args[5]);

            _logger.LogDebug("旧版本进程 ID: {ProcessId}", oldProcessId);
            _logger.LogDebug("目标文件: {Target}", target);
            _logger.LogDebug("来源文件: {Source}", source);

            // 等待旧版本进程退出
            try
            {
                var oldProcess = Process.GetProcessById(oldProcessId);
                _logger.LogDebug("正在等待旧版本进程退出");
                await Task.Run(() => oldProcess.WaitForExit()).ConfigureAwait(false);
                _logger.LogDebug("旧版本进程已退出");
            }
            catch (ArgumentException)
            {
                _logger.LogDebug("旧版本进程已不存在");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "等待旧版本进程退出时发生异常");
            }

            // 替换文件
            _logger.LogDebug("正在替换文件");
            var error = UpdateHelper.Replace(source, target, _logger);
            
            if (error == null)
            {
                _logger.LogInformation("更新成功");
                
                if (restart)
                {
                    var restartArgs = $"update_finished \"{source}\"";
                    _logger.LogDebug("重启中，使用参数: {Args}", restartArgs);
                    
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = target,
                        Arguments = restartArgs,
                        UseShellExecute = true
                    });
                }
            }
            else
            {
                _logger.LogError(error, "更新失败");
                
                if (restart)
                {
                    var restartArgs = $"update_failed \"{error.Message}\"";
                    _logger.LogDebug("重启中，使用参数: {Args}", restartArgs);
                    
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = target,
                        Arguments = restartArgs,
                        UseShellExecute = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新过程出错");
        }
    }

    /// <summary>
    /// 处理更新完成命令
    /// </summary>
    private void HandleUpdateFinishedCommand(string[] args)
    {
        if (args.Length < 3)
        {
            _logger.LogWarning("更新完成参数不完整");
            return;
        }

        try
        {
            var toDelete = args[2];
            if (File.Exists(toDelete))
            {
                File.Delete(toDelete);
                _logger.LogInformation("已删除更新来源文件: {Path}", toDelete);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "删除更新来源文件失败");
        }
    }

    /// <summary>
    /// 处理更新失败命令
    /// </summary>
    private void HandleUpdateFailedCommand(string[] args)
    {
        if (args.Length < 3)
        {
            _logger.LogWarning("更新失败参数不完整");
            return;
        }

        var reason = args[2];
        _logger.LogError("更新失败: {Reason}\n你可以手动将文件替换为新版本或再次尝试更新", reason);
    }

    /// <summary>
    /// 执行更新
    /// </summary>
    /// <param name="sourcePath">来源文件路径</param>
    /// <param name="targetPath">目标文件路径</param>
    /// <param name="restart">是否重启</param>
    public async Task<bool> PerformUpdateAsync(string sourcePath, string targetPath, bool restart = true)
    {
        try
        {
            _logger.LogInformation("开始执行更新");

            var currentProcessId = Process.GetCurrentProcess().Id;
            var args = $"update {currentProcessId} \"{targetPath}\" \"{sourcePath}\" {restart}";

            // 启动新进程执行更新
            Process.Start(new ProcessStartInfo
            {
                FileName = targetPath,
                Arguments = args,
                UseShellExecute = true
            });

            // 当前进程退出
            await Task.Delay(1000).ConfigureAwait(false);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行更新失败");
            return false;
        }
    }

    /// <summary>
    /// 检查是否需要更新
    /// </summary>
    /// <param name="currentVersion">当前版本</param>
    /// <param name="latestVersion">最新版本</param>
    /// <returns>是否需要更新</returns>
    public bool CheckNeedsUpdate(string currentVersion, string latestVersion)
    {
        return UpdateHelper.NeedsUpdate(currentVersion, latestVersion);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await StopAsync().ConfigureAwait(false);
    }
}