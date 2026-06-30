using PCL_CE.Neo.Core.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.IO;

public static class Files
{
    private const string ModuleName = "Files";

    public static async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        try
        {
            LogWrapper.Debug($"Reading file: {path}", ModuleName);
            var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
            LogWrapper.Debug($"Read {bytes.Length} bytes from: {path}", ModuleName);
            return bytes;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to read file: {path}");
            throw;
        }
    }

    public static async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));
        if (bytes == null)
            throw new ArgumentNullException(nameof(bytes));

        try
        {
            LogWrapper.Debug($"Writing {bytes.Length} bytes to: {path}", ModuleName);
            await File.WriteAllBytesAsync(path, bytes, cancellationToken);
            LogWrapper.Debug($"Successfully wrote to: {path}", ModuleName);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to write file: {path}");
            throw;
        }
    }

    public static async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        try
        {
            LogWrapper.Debug($"Reading text file: {path}", ModuleName);
            var text = await File.ReadAllTextAsync(path, cancellationToken);
            LogWrapper.Debug($"Read {text.Length} characters from: {path}", ModuleName);
            return text;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to read text file: {path}");
            throw;
        }
    }

    public static async Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        try
        {
            LogWrapper.Debug($"Writing {content?.Length ?? 0} characters to: {path}", ModuleName);
            await File.WriteAllTextAsync(path, content, cancellationToken);
            LogWrapper.Debug($"Successfully wrote text to: {path}", ModuleName);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to write text file: {path}");
            throw;
        }
    }

    public static async Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        try
        {
            LogWrapper.Debug($"Reading lines from: {path}", ModuleName);
            var lines = await File.ReadAllLinesAsync(path, cancellationToken);
            LogWrapper.Debug($"Read {lines.Length} lines from: {path}", ModuleName);
            return lines;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to read lines from: {path}");
            throw;
        }
    }

    public static async Task WriteAllLinesAsync(string path, string[] lines, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));
        if (lines == null)
            throw new ArgumentNullException(nameof(lines));

        try
        {
            LogWrapper.Debug($"Writing {lines.Length} lines to: {path}", ModuleName);
            await File.WriteAllLinesAsync(path, lines, cancellationToken);
            LogWrapper.Debug($"Successfully wrote lines to: {path}", ModuleName);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to write lines to: {path}");
            throw;
        }
    }

    public static bool Exists(string path)
    {
        var result = File.Exists(path);
        LogWrapper.Debug($"File exists check: {path} = {result}", ModuleName);
        return result;
    }

    public static void Delete(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        try
        {
            LogWrapper.Debug($"Deleting file: {path}", ModuleName);
            File.Delete(path);
            LogWrapper.Debug($"Deleted file: {path}", ModuleName);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to delete file: {path}");
            throw;
        }
    }

    public static FileInfo GetInfo(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        return new FileInfo(path);
    }

    public static long GetSize(string path)
    {
        if (!Exists(path))
            throw new FileNotFoundException("File not found", path);

        return new FileInfo(path).Length;
    }

    public static DateTime GetCreationTime(string path)
    {
        if (!Exists(path))
            throw new FileNotFoundException("File not found", path);

        return File.GetCreationTime(path);
    }

    public static DateTime GetLastWriteTime(string path)
    {
        if (!Exists(path))
            throw new FileNotFoundException("File not found", path);

        return File.GetLastWriteTime(path);
    }

    public static void Copy(string sourcePath, string destinationPath, bool overwrite = false)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new ArgumentNullException(nameof(sourcePath));
        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentNullException(nameof(destinationPath));

        try
        {
            LogWrapper.Debug($"Copying file: {sourcePath} -> {destinationPath}", ModuleName);
            File.Copy(sourcePath, destinationPath, overwrite);
            LogWrapper.Debug($"Copied file: {sourcePath} -> {destinationPath}", ModuleName);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to copy file: {sourcePath} -> {destinationPath}");
            throw;
        }
    }

    public static void Move(string sourcePath, string destinationPath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new ArgumentNullException(nameof(sourcePath));
        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentNullException(nameof(destinationPath));

        try
        {
            LogWrapper.Debug($"Moving file: {sourcePath} -> {destinationPath}", ModuleName);
            File.Move(sourcePath, destinationPath);
            LogWrapper.Debug($"Moved file: {sourcePath} -> {destinationPath}", ModuleName);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to move file: {sourcePath} -> {destinationPath}");
            throw;
        }
    }

    public static async Task AppendAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        try
        {
            LogWrapper.Debug($"Appending {content?.Length ?? 0} characters to: {path}", ModuleName);
            await File.AppendAllTextAsync(path, content, cancellationToken);
            LogWrapper.Debug($"Successfully appended to: {path}", ModuleName);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to append to file: {path}");
            throw;
        }
    }
}