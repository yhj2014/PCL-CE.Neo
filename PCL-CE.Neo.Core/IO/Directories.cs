using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.IO;

public static class Directories
{
    public static async Task<bool> CheckPermissionAsync(string? path, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            if (!Directory.Exists(path))
            {
                return false;
            }

            await Task.Run(() => Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly).Any(), cancellationToken);
            return true;
        }
        catch (OperationCanceledException)
        {
            LogWrapper.Warn("权限检查被取消");
            return false;
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, $"没有对文件夹 {path} 的权限，请尝试以管理员权限运行。");
            return false;
        }
    }

    public static async Task CheckPermissionWithExceptionAsync(string? path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(nameof(path), "文件夹路径不能为空！");
        }

        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"文件夹不存在：{path}");
        }

        try
        {
            await Task.Run(() => Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly).Any(), cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException($"无法访问文件夹 {path}：{ex.Message}", ex);
        }
    }

    public static async Task<int> DeleteDirectoryAsync(string? path, bool ignoreIssue = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return 0;
        }

        var deletedCount = 0;

        try
        {
            foreach (var filePath in Directory.EnumerateFiles(path))
            {
                cancellationToken.ThrowIfCancellationRequested();

                for (var attempt = 0; attempt < 2; attempt++)
                {
                    try
                    {
                        await FileDeleteAsync(filePath, cancellationToken).ConfigureAwait(false);
                        deletedCount++;
                        break;
                    }
                    catch (Exception ex) when (attempt == 0)
                    {
                        LogWrapper.Error(ex, $"删除文件失败，将在 0.3s 后重试（{filePath}）");
                        await Task.Delay(300, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        if (ignoreIssue)
                        {
                            LogWrapper.Error(ex, "删除单个文件可忽略地失败");
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            foreach (var subDir in Directory.EnumerateDirectories(path))
            {
                cancellationToken.ThrowIfCancellationRequested();
                deletedCount += await DeleteDirectoryAsync(subDir, ignoreIssue, cancellationToken).ConfigureAwait(false);
            }

            for (var attempt = 0; attempt < 2; attempt++)
            {
                try
                {
                    Directory.Delete(path, true);
                    break;
                }
                catch (Exception ex) when (attempt == 0)
                {
                    LogWrapper.Error(ex, $"删除文件夹失败，将在 0.3s 后重试（{path}）");
                    await Task.Delay(300, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (ignoreIssue)
                    {
                        LogWrapper.Error(ex, "删除单个文件夹可忽略地失败");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
        catch (DirectoryNotFoundException ex)
        {
            LogWrapper.Error(ex, $"疑似为孤立符号链接，尝试直接删除（{path}）", "Developer");
            try
            {
                Directory.Delete(path);
            }
            catch (Exception deleteEx)
            {
                if (!ignoreIssue)
                {
                    throw;
                }
                LogWrapper.Error(deleteEx, $"删除符号链接文件夹失败（{path}）");
            }
        }

        return deletedCount;
    }

    public static async Task CopyDirectoryAsync(string? fromPath, string? toPath, Action<double>? progressIncrementHandler = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(fromPath))
        {
            throw new ArgumentNullException(nameof(fromPath), "源文件夹路径为空");
        }

        if (string.IsNullOrEmpty(toPath))
        {
            throw new ArgumentNullException(nameof(toPath), "目标文件夹路径为空");
        }

        fromPath = Path.GetFullPath(fromPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        toPath = Path.GetFullPath(toPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        var allFiles = (await EnumerateFilesAsync(fromPath, cancellationToken).ConfigureAwait(false)).ToList();
        var totalFiles = allFiles.Count;
        long copiedFiles = 0;

        foreach (var file in allFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = file.FullName[fromPath.Length..];
            var destFilePath = Path.Combine(toPath, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(destFilePath)!);

            for (var attempt = 0; attempt < 2; attempt++)
            {
                try
                {
                    await FileCopyAsync(file.FullName, destFilePath, overwrite: true, cancellationToken).ConfigureAwait(false);
                    copiedFiles++;
                    progressIncrementHandler?.Invoke((double)copiedFiles / totalFiles);
                    break;
                }
                catch (Exception ex) when (attempt == 0)
                {
                    LogWrapper.Error(ex, $"复制文件失败，将在 0.3s 后重试（{file.FullName} 到 {destFilePath}）");
                    await Task.Delay(300, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    LogWrapper.Error(ex, $"复制文件失败（{file.FullName} 到 {destFilePath}）");
                    throw;
                }
            }
        }
    }

    public static async Task<IEnumerable<FileInfo>> EnumerateFilesAsync(string? directory, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"目录不存在：{directory}");
        }

        try
        {
            return await Task.Run(() => new DirectoryInfo(directory).EnumerateFiles("*", SearchOption.AllDirectories).ToList(), cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            LogWrapper.Warn("文件夹遍历被取消");
            return [];
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"遍历文件夹失败（{directory}）");
            return [];
        }
    }

    private static async Task FileDeleteAsync(string path, CancellationToken cancellationToken)
    {
        await Task.Run(() => File.Delete(path), cancellationToken).ConfigureAwait(false);
    }

    private static async Task FileCopyAsync(string sourceFileName, string destFileName, bool overwrite, CancellationToken cancellationToken)
    {
        await using FileStream sourceStream = new(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        await using FileStream destStream = new(destFileName, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, useAsync: true);
        await sourceStream.CopyToAsync(destStream, cancellationToken).ConfigureAwait(false);
    }
}