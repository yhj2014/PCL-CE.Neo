using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Update;

/// <summary>
/// 更新辅助类，用于启动器自动更新
/// </summary>
public static class UpdateHelper
{
    /// <summary>
    /// 更新启动器（替换文件）
    /// </summary>
    /// <param name="source">用于替换的来源文件路径</param>
    /// <param name="target">目标文件路径</param>
    /// <param name="logger">日志记录器</param>
    /// <returns>异常，如果成功则为 null</returns>
    public static Exception? Replace(string source, string target, ILogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentNullException(nameof(source));
        }
        if (string.IsNullOrWhiteSpace(target))
        {
            throw new ArgumentNullException(nameof(target));
        }

        try
        {
            source = Path.GetFullPath(source);
            target = Path.GetFullPath(target);

            logger?.LogInformation("开始更新: 从 {Source} 替换到 {Target}", source, target);

            if (!File.Exists(source))
            {
                throw new FileNotFoundException("来源文件不存在", source);
            }

            // 创建备份
            var backup = $"{target}.bak.{DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture)}";
            
            if (File.Exists(target))
            {
                logger?.LogDebug("备份目标文件到 {Backup}", backup);
                File.Copy(target, backup, overwrite: true);
                if (!File.Exists(backup))
                {
                    throw new IOException("备份目标文件失败");
                }
            }

            try
            {
                // 跨平台文件替换逻辑
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows: 使用文件监视器等待删除完成
                    ReplaceWindows(source, target, backup, logger);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                         RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // Linux/macOS: 直接替换
                    ReplaceUnix(source, target, backup, logger);
                }
                else
                {
                    // 其他平台: 使用通用方法
                    ReplaceGeneric(source, target, backup, logger);
                }

                logger?.LogInformation("更新完成");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "更新失败，正在恢复备份");
                
                // 出错时恢复备份
                if (File.Exists(backup) && !File.Exists(target))
                {
                    File.Move(backup, target);
                    logger?.LogInformation("已恢复备份");
                }
                
                return ex;
            }
            finally
            {
                // 删除备份文件
                if (File.Exists(backup))
                {
                    try
                    {
                        File.Delete(backup);
                        logger?.LogDebug("已删除备份文件");
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "删除备份文件失败");
                    }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "更新过程中发生异常");
            return ex;
        }
    }

    private static void ReplaceWindows(string source, string target, string backup, ILogger? logger)
    {
        var parentPath = Path.GetDirectoryName(target) ?? string.Empty;
        var fileName = Path.GetFileName(target);

        if (File.Exists(target))
        {
            // 使用文件监视器等待删除完成
            using var watcher = new FileSystemWatcher(parentPath, fileName);
            var deletedEvent = new ManualResetEventSlim(false);
            watcher.Deleted += (_, _) => deletedEvent.Set();
            watcher.EnableRaisingEvents = true;

            File.Delete(target);

            if (!deletedEvent.Wait(TimeSpan.FromSeconds(3)))
            {
                watcher.EnableRaisingEvents = false;
                throw new TimeoutException("删除目标文件超时");
            }

            watcher.EnableRaisingEvents = false;
        }

        File.Copy(source, target, overwrite: true);
        
        if (!File.Exists(target))
        {
            throw new IOException("复制到目标文件失败");
        }
    }

    private static void ReplaceUnix(string source, string target, string backup, ILogger? logger)
    {
        // Linux/macOS: 直接替换文件
        if (File.Exists(target))
        {
            File.Delete(target);
        }

        File.Copy(source, target, overwrite: true);

        // 设置可执行权限（Linux/macOS）
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            try
            {
                // 使用 chmod 设置可执行权限
                var chmodResult = RunCommand("chmod", $"+x \"{target}\"");
                if (chmodResult != 0)
                {
                    logger?.LogWarning("设置可执行权限失败");
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "设置可执行权限时发生异常");
            }
        }

        if (!File.Exists(target))
        {
            throw new IOException("复制到目标文件失败");
        }
    }

    private static void ReplaceGeneric(string source, string target, string backup, ILogger? logger)
    {
        // 通用方法：先删除再复制
        if (File.Exists(target))
        {
            File.Delete(target);
        }

        File.Copy(source, target, overwrite: true);

        if (!File.Exists(target))
        {
            throw new IOException("复制到目标文件失败");
        }
    }

    private static int RunCommand(string command, string arguments)
    {
        try
        {
            using var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            
            process.Start();
            process.WaitForExit();
            return process.ExitCode;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// 检查是否需要更新
    /// </summary>
    /// <param name="currentVersion">当前版本</param>
    /// <param name="latestVersion">最新版本</param>
    /// <returns>是否需要更新</returns>
    public static bool NeedsUpdate(string currentVersion, string latestVersion)
    {
        if (string.IsNullOrWhiteSpace(currentVersion) || string.IsNullOrWhiteSpace(latestVersion))
        {
            return false;
        }

        try
        {
            var current = ParseVersion(currentVersion);
            var latest = ParseVersion(latestVersion);
            return latest > current;
        }
        catch
        {
            return false;
        }
    }

    private static Version ParseVersion(string version)
    {
        // 移除 'v' 前缀
        version = version.TrimStart('v', 'V');
        
        // 处理预发布版本（如 1.0.0-alpha）
        var dashIndex = version.IndexOf('-');
        if (dashIndex > 0)
        {
            version = version.Substring(0, dashIndex);
        }

        return new Version(version);
    }
}