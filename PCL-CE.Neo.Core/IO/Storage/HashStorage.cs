using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Utils.Exts;
using PCL_CE.Neo.Core.Utils.Hash;

namespace PCL_CE.Neo.Core.IO.Storage;

public class HashStorage(string folder, IHashProvider hashProvider, bool compressObjects = false, bool correctMisplacedFile = true, int prefixLength = 2)
{
    private readonly ILogger<HashStorage> _logger = ServiceLocator.GetService<ILogger<HashStorage>>() ?? new Microsoft.Extensions.Logging.Abstractions.NullLogger<HashStorage>();

    public async Task<string?> PutAsync(string fromPath, string? hash = null)
    {
        ArgumentNullException.ThrowIfNull(fromPath);
        var filePath = Path.GetFullPath(fromPath);
        if (!File.Exists(filePath)) return null;
        await using var originalFs = File.Open(fromPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await PutAsync(originalFs, hash).ConfigureAwait(false);
    }

    public async Task<string?> PutAsync(Stream input, string? hash = null)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (hash is not null && hash.Length != hashProvider.Length)
            throw new ArgumentException("Provide hash is not correct", nameof(hash));

        if (input.CanSeek) input.Position = 0;

        var fileHash = hash ?? (await hashProvider.ComputeHashAsync(input).ConfigureAwait(false)).ToHexString();
        var destPath = _GetDestPath(fileHash);
        if (correctMisplacedFile && _CorrectMisplacedFile(fileHash)) _logger.LogInformation("Move misplaced file into correct folder");
        if (File.Exists(destPath)) return fileHash;
        await using var destinationFs = _GetSaveStream(destPath);
        await input.CopyToAsync(destinationFs).ConfigureAwait(false);
        return fileHash;
    }

    public Task<bool> DeleteAsync(string hash)
    {
        ArgumentNullException.ThrowIfNull(hash);

        var filePath = _GetDestPath(hash);
        if (!File.Exists(filePath) && correctMisplacedFile)
            filePath = _GetMisplacedFilePath(hash);

        if (!File.Exists(filePath)) return Task.FromResult(false);

        try
        {
            File.Delete(filePath);
        }
        catch (FileNotFoundException) { }
        catch (DirectoryNotFoundException ex)
        {
            _logger.LogError(ex, "Unexpected directory not found {FilePath}", filePath);
            return Task.FromResult(false);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to delete file {FilePath}", filePath);
            return Task.FromResult(false);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied when deleting file {FilePath}", filePath);
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }

    public Stream? Get(string hash)
    {
        ArgumentNullException.ThrowIfNull(hash);
        var destPath = _GetDestPath(hash);
        if (correctMisplacedFile && _CorrectMisplacedFile(hash))
            _logger.LogInformation("Move misplaced file into correct folder: {Hash}", hash);

        return File.Exists(destPath) ? _GetReadStream(destPath) : null;
    }

    public bool Exists(string hash)
    {
        ArgumentNullException.ThrowIfNull(hash);
        return File.Exists(_GetDestPath(hash)) || (correctMisplacedFile && File.Exists(_GetMisplacedFilePath(hash)));
    }

    private Stream _GetSaveStream(string destPath)
    {
        var fs = File.Open(destPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        if (compressObjects) return new DeflateStream(fs, CompressionMode.Compress);
        return fs;
    }

    private Stream _GetReadStream(string destPath)
    {
        var fs = File.Open(destPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (compressObjects) return new DeflateStream(fs, CompressionMode.Decompress);
        return fs;
    }

    private string _GetDestPath(string hash)
    {
        return Path.Combine(folder, _GetPrefixFolder(hash), hash);
    }

    private bool _CorrectMisplacedFile(string hash)
    {
        var misplacedPath = _GetMisplacedFilePath(hash);
        if (!File.Exists(misplacedPath)) return false;
        var correctPath = _GetDestPath(hash);
        File.Move(misplacedPath, correctPath);
        return true;
    }

    private string _GetMisplacedFilePath(string hash)
    {
        return Path.Combine(folder, hash);
    }

    private string _GetPrefixFolder(string hash)
    {
        if (hash.Length < prefixLength)
            throw new ArgumentException($"Hash length({hash.Length}) is shorter than required prefix length({prefixLength})", nameof(hash));

        var folderName = hash[..prefixLength];
        var folderPath = Path.Combine(folder, folderName);
        Directory.CreateDirectory(folderPath);
        return folderName;
    }
}