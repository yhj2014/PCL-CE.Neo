using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.App;
using PCL_CE.Neo.Core.Logging;
using PCL.CE.Neo.Core.Utils.Hash;

namespace PCL_CE.Neo.Core.IO;

public static class Files
{
    public static readonly JsonSerializerOptions PrettierJsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static bool ArePathsEqual(string path1, string path2)
    {
        var fullPath1 = Path.GetFullPath(path1).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fullPath2 = Path.GetFullPath(path2).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return string.Equals(fullPath1, fullPath2, StringComparison.OrdinalIgnoreCase);
    }

    public static async Task CopyFileAsync(string fromPath, string toPath, CancellationToken cancelToken = default)
    {
        try
        {
            var fullFromPath = GetFullPath(fromPath);
            var fullToPath = GetFullPath(toPath);
            if (fullFromPath == fullToPath) return;

            var directoryName = Path.GetDirectoryName(fullToPath);
            if (directoryName is null)
            {
                throw new InvalidOperationException("无法获取目标目录");
            }
            Directory.CreateDirectory(directoryName);

            const int bufferSize = 4096;
            await using var sourceStream = new FileStream(fullFromPath, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
            await using var destinationStream = new FileStream(fullToPath, FileMode.Create, FileAccess.Write,
                FileShare.Read, bufferSize, FileOptions.Asynchronous);
            await sourceStream.CopyToAsync(destinationStream, cancelToken);
        }
        catch (Exception ex)
        {
            throw new IOException($"复制文件出错：{fromPath} -> {toPath}", ex);
        }
    }

    public static async Task CopyDirectoryAsync(string sourceDir, string destDir, CancellationToken cancelToken = default)
    {
        Directory.CreateDirectory(destDir);

        var files = Directory.GetFiles(sourceDir);
        var directories = Directory.GetDirectories(sourceDir);

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(2, Environment.ProcessorCount),
            CancellationToken = cancelToken
        };

        await Parallel.ForEachAsync(files, parallelOptions, async (file, ct) =>
        {
            var destFile = Path.Combine(destDir, Path.GetFileName(file));
            await CopyFileAsync(file, destFile, ct);
        });

        await Parallel.ForEachAsync(directories, parallelOptions, async (subDir, ct) =>
        {
            var destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
            await CopyDirectoryAsync(subDir, destSubDir, ct);
        });
    }

    public static async Task<byte[]> ReadAllBytesOrEmptyAsync(string filePath, CancellationToken cancelToken = default)
    {
        try
        {
            var fullPath = GetFullPath(filePath);
            if (File.Exists(fullPath))
            {
                return await File.ReadAllBytesAsync(fullPath, cancelToken);
            }
            throw new FileNotFoundException(fullPath);
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, $"读取文件出错：{filePath}");
            return [];
        }
    }

    public static async Task<string> ReadAllTextOrEmptyAsync(string filePath, Encoding? encoding = null, CancellationToken cancelToken = default)
    {
        try
        {
            var fullPath = GetFullPath(filePath);
            if (!File.Exists(fullPath)) throw new FileNotFoundException(fullPath);
            if (encoding == null) return await File.ReadAllTextAsync(fullPath, cancelToken);
            return await File.ReadAllTextAsync(fullPath, encoding, cancelToken);
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, $"读取文件出错：{filePath}");
            return "";
        }
    }

    public static async Task<MemoryStream> ReadFileToStreamOrEmptyAsync(string filePath, CancellationToken cancelToken = default)
    {
        try
        {
            var fullPath = GetFullPath(filePath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException(fullPath);

            await using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream, cancelToken);
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, $"读取文件到流出错：{filePath}");
            return new MemoryStream();
        }
    }

    public static async Task WriteFileAsync(string filePath, string text, bool append = false, Encoding? encoding = null, CancellationToken cancelToken = default)
    {
        var fullPath = GetFullPath(filePath);
        var directoryName = Path.GetDirectoryName(fullPath);
        if (directoryName is null)
        {
            throw new InvalidOperationException("无法获取目标目录");
        }
        Directory.CreateDirectory(directoryName);

        if (append)
        {
            encoding ??= Encoding.UTF8;
            await File.AppendAllTextAsync(fullPath, text, encoding, cancelToken);
        }
        else
        {
            encoding ??= new UTF8Encoding(false);
            await File.WriteAllTextAsync(fullPath, text, encoding, cancelToken);
        }
    }

    public static async Task WriteFileAsync(string filePath, byte[] content, bool append = false, CancellationToken cancelToken = default)
    {
        var fullPath = GetFullPath(filePath);
        var directoryName = Path.GetDirectoryName(fullPath);
        if (directoryName is null)
        {
            throw new InvalidOperationException("无法获取目标目录");
        }
        Directory.CreateDirectory(directoryName);

        var fileMode = append ? FileMode.Append : FileMode.Create;
        await using var fileStream = new FileStream(fullPath, fileMode, FileAccess.Write, FileShare.Read);
        await fileStream.WriteAsync(content.AsMemory(), cancelToken);
    }

    public static async Task<bool> WriteFileAsync(string filePath, Stream? stream, CancellationToken cancelToken = default)
    {
        if (stream == null) return false;
        try
        {
            var fullPath = GetFullPath(filePath);
            var directoryName = Path.GetDirectoryName(fullPath);
            if (directoryName is null)
            {
                throw new InvalidOperationException("无法获取目标目录");
            }
            Directory.CreateDirectory(directoryName);

            await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
            fileStream.SetLength(0);
            await stream.CopyToAsync(fileStream, cancelToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "保存流出错");
            return false;
        }
    }

    public static async Task<string> ComputeFileHashAsync(string? filePath, IHashProvider hashProvider, bool ignoreIfBusy = false)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            LogWrapper.Warn(new ArgumentNullException(nameof(filePath)), "文件路径为空");
            return string.Empty;
        }

        if (ignoreIfBusy && await CheckFileBusyAsync(filePath).ConfigureAwait(false))
        {
            return string.Empty;
        }

        for (var attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                return (await hashProvider.ComputeHashAsync(fs).ConfigureAwait(false)).ToHexString();
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                LogWrapper.Warn(ex, $"计算文件哈希失败：{filePath}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                if (attempt == 0)
                {
                    LogWrapper.Warn(ex, $"计算文件哈希可重试失败：{filePath}");
                    await Task.Delay(Random.Shared.Next(200, 500)).ConfigureAwait(false);
                    continue;
                }
                LogWrapper.Warn(ex, $"计算文件哈希失败：{filePath}");
                return string.Empty;
            }
        }

        return string.Empty;
    }

    public static Task<string> GetFileMD5Async(string? filePath)
        => ComputeFileHashAsync(filePath, MD5Provider.Instance);

    public static Task<string> GetFileSHA1Async(string? filePath)
        => ComputeFileHashAsync(filePath, SHA1Provider.Instance);

    public static Task<string> GetFileSHA256Async(string? filePath, bool ignoreIfBusy = false)
        => ComputeFileHashAsync(filePath, SHA256Provider.Instance, ignoreIfBusy);

    public static Task<string> GetFileSHA512Async(string? filePath, bool ignoreIfBusy = false)
        => ComputeFileHashAsync(filePath, SHA512Provider.Instance, ignoreIfBusy);

    public static string GetFullPath(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        return Path.IsPathRooted(filePath) ? filePath : Path.Combine(Paths.DefaultDirectory, filePath);
    }

    public static async Task<bool> CheckFileBusyAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return false;
            await using FileStream fs = new(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read | FileShare.Delete);
            return false;
        }
        catch (IOException) { return true; }
        catch { return false; }
    }

    public static JsonNode MergeJson(JsonNode target, JsonNode source)
    {
        if (target == null && source == null)
        {
            throw new ArgumentNullException(nameof(target), "目标和源 JSON 不能同时为 null。");
        }

        if (target == null)
        {
            return source.DeepClone();
        }

        if (target is not JsonObject targetObj || source is not JsonObject sourceObj)
        {
            return source.DeepClone();
        }

        var result = (JsonObject)targetObj.DeepClone();

        foreach (var (key, sourceValue) in sourceObj)
        {
            var targetValue = result[key];

            if (sourceValue == null)
            {
                continue;
            }

            if (sourceValue is JsonObject && targetValue is JsonObject)
            {
                result[key] = MergeJson(targetValue, sourceValue);
            }
            else if (sourceValue is JsonArray sourceArray && targetValue is JsonArray targetArray)
            {
                var uniqueValues = new HashSet<string>(StringComparer.Ordinal);
                JsonArray mergedArray = [];

                foreach (var item in targetArray)
                {
                    if (item == null) continue;
                    var itemStr = item.ToJsonString();
                    if (uniqueValues.Add(itemStr))
                    {
                        mergedArray.Add(item.DeepClone());
                    }
                }

                foreach (var item in sourceArray)
                {
                    if (item == null) continue;
                    var itemStr = item.ToJsonString();
                    if (uniqueValues.Add(itemStr))
                    {
                        mergedArray.Add(item.DeepClone());
                    }
                }

                result[key] = mergedArray;
            }
            else
            {
                result[key] = sourceValue.DeepClone();
            }
        }

        return result;
    }

    public static bool IsPathWithinDirectory(string childPath, string baseDirectory)
    {
        var baseDir = Path.GetFullPath(baseDirectory);
        var child = Path.GetFullPath(childPath);

        return child.StartsWith(
            baseDir.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase);
    }
}