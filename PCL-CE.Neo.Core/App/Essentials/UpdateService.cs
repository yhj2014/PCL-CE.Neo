using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.App;

namespace PCL_CE.Neo.Core.App.Essentials;

public sealed class UpdateService
{
    private readonly ILogger<UpdateService> _logger;

    public UpdateService(ILogger<UpdateService> logger)
    {
        _logger = logger;
    }

    public bool ProcessUpdateCommand(string[] args)
    {
        if (args is not ["update", _, _, _, _])
        {
            switch (args)
            {
                case ["update_finished", _]:
                {
                    var toDelete = args[1];
                    File.Delete(toDelete);
                    _logger.LogDebug("更新来源文件已删除");
                    break;
                }
                case ["update_failed", _]:
                {
                    var reason = args[1];
                    _logger.LogError($"更新失败: {reason}\n你可以手动将 exe 文件替换为 PCL 目录中的新版本" +
                        $"或再次尝试更新，若再次尝试仍然失败，请尽快反馈这个问题");
                    break;
                }
                default: _logger.LogDebug("无更新任务"); break;
            }
            return false;
        }

        try
        {
            _logger.LogInformation("开始更新");
            var oldProcessId = int.Parse(args[1]);
            _logger.LogDebug($"旧版本进程 ID: {oldProcessId}");
            
            try
            {
                var oldProcess = Process.GetProcessById(oldProcessId);
                _logger.LogDebug("正在等待旧版本进程退出");
                oldProcess.WaitForExit();
                _logger.LogTrace("旧版本进程已退出");
            }
            catch
            {
            }

            _logger.LogDebug("正在替换文件");
            var target = args[2];
            var source = args[3];
            var ex = UpdateHelper.Replace(source, target);
            if (ex == null) _logger.LogTrace("替换完成");
            else _logger.LogError(ex, "替换文件出错");

            var restart = bool.Parse(args[4]);
            if (restart)
            {
                var restartArgs = (ex == null) ? $"finished \"{source}\"" : $"failed \"{ex.Message}\"";
                restartArgs = $"update_{restartArgs}";
                _logger.LogDebug($"重启中，使用参数: {restartArgs}");
                Process.Start(target, restartArgs);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新过程出错");
        }
        
        return true;
    }
}