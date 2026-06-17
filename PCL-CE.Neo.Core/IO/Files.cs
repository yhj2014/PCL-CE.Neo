using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Utils.Hash;

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
            var fullFromPath = Path.GetFullPath(fromPath);
            var fullToPath = Path.GetFullPath(toPath);
            if (fullFromPath == fullToPath) return;

            var directoryName = Path.GetDirectoryName(fullToPath);
            if (directoryName is null)
                throw new InvalidOperationException("无法获取目标目录");

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
            var fullPath = Path.GetFullPath(filePath);
            if (File.Exists(fullPath))
                return await File.ReadAllBytesAsync(fullPath, cancelToken);

            throw new FileNotFoundException(fullPath);
        }
        catch (Exception)
        {
            return [];
        }
    }

    public static async Task<string> ReadAllTextOrEmptyAsync(string filePath, Encoding? encoding = null, CancellationToken cancelToken = default)
    {
        try
        {
            var fullPath = Path.GetFullPath(filePath);
            if (!File.Exists(fullPath)) throw new FileNotFoundException(fullPath);
            if (encoding == null) return await File.ReadAllTextAsync(fullPath, cancelToken);
            return await File.ReadAllTextAsync(fullPath, encoding, cancelToken);
        }
        catch (Exception)
        {
            return "";
        }
    }

    public static async Task<MemoryStream> ReadFileToStreamOrEmptyAsync(string filePath, CancellationToken cancelToken = default)
    {
        try
        {
            var fullPath = Path.GetFullPath(filePath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException(fullPath);

            await using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream, cancelToken);
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception)
        {
            return new MemoryStream();
        }
    }

    public static async Task WriteFileAsync(string filePath, string text, bool append = false, Encoding? encoding = null, CancellationToken cancelToken = default)
    {
        var fullPath = Path.GetFullPath(filePath);
        var directoryName = Path.GetDirectoryName(fullPath);
        if (directoryName is null)
            throw new InvalidOperationException("无法获取目标目录");

        Directory.CreateDirectory(directoryName);

        encoding ??= new UTF8Encoding(false);

        if (append)
            await File.AppendAllTextAsync(fullPath, text, encoding, cancelToken);
        else
            await File.WriteAllTextAsync(fullPath, text, encoding, cancelToken);
    }

    public static async Task WriteFileAsync(string filePath, byte[] content, bool append = false, CancellationToken cancelToken = default)
    {
        var fullPath = Path.GetFullPath(filePath);
        var directoryName = Path.GetDirectoryName(fullPath);
        if (directoryName is null)
            throw new InvalidOperationException("无法获取目标目录");

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
            var fullPath = Path.GetFullPath(filePath);
            var directoryName = Path.GetDirectoryName(fullPath);
            if (directoryName is null)
                throw new InvalidOperationException("无法获取目标目录");

            Directory.CreateDirectory(directoryName);

            await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
            fileStream.SetLength(0);
            await stream.CopyToAsync(fileStream, cancelToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static async Task<string> ComputeFileHashAsync(string? filePath, IHashProvider hashProvider, bool ignoreIfBusy = false)
    {
        if (string.IsNullOrEmpty(filePath))
            return string.Empty;

        if (ignoreIfBusy && await CheckFileBusyAsync(filePath).ConfigureAwait(false))
            return string.Empty;

        for (var attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                return (await hashProvider.ComputeHashAsync(fs).ConfigureAwait(false)).ToHexString();
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                return string.Empty;
            }
            catch (Exception)
            {
                if (attempt == 0)
                {
                    await Task.Delay(Random.Shared.Next(200, 500)).ConfigureAwait(false);
                    continue;
                }
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
        return Path.IsPathRooted(filePath) ? filePath : Path.Combine(AppContext.BaseDirectory, filePath);
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

    public static bool IsPathWithinDirectory(string childPath, string baseDirectory)
    {
        var baseDir = Path.GetFullPath(baseDirectory);
        var child = Path.GetFullPath(childPath);

        return child.StartsWith(
            baseDir.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal
        );
    }
}