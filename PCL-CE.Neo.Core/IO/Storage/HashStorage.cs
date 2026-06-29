using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Utils.Exts;
using PCL_CE.Neo.Core.Utils.Hash;

namespace PCL_CE.Neo.Core.IO.Storage;

public class HashStorage(string folder, IHashProvider hashProvider, bool compressObjects = false, bool correctMisplacedFile = true, int prefixLength = 2)
{
    private readonly string _folder = folder;
    private readonly IHashProvider _hashProvider = hashProvider;
    private readonly bool _compressObjects = compressObjects;
    private readonly bool _correctMisplacedFile = correctMisplacedFile;
    private readonly int _prefixLength = prefixLength;

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

        if (hash is not null && hash.Length != _hashProvider.Length)
            throw new ArgumentException("Provide hash is not correct", nameof(hash));

        if (input.CanSeek) input.Position = 0;

        var fileHash = hash ?? (await _hashProvider.ComputeHashAsync(input).ConfigureAwait(false)).ToHexString();
        var destPath = _GetDestPath(fileHash);
        if (_correctMisplacedFile && _CorrectMisplacedFile(fileHash)) { }
        if (File.Exists(destPath)) return fileHash;
        await using var destinationFs = _GetSaveStream(destPath);
        if (input.CanSeek) input.Position = 0;
        await input.CopyToAsync(destinationFs).ConfigureAwait(false);
        return fileHash;
    }

    public Task<bool> DeleteAsync(string hash)
    {
        ArgumentNullException.ThrowIfNull(hash);

        var filePath = _GetDestPath(hash);
        if (!File.Exists(filePath) && _correctMisplacedFile)
            filePath = _GetMisplacedFilePath(hash);

        try
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.FromResult(true);
        }
        catch (IOException)
        {
            return Task.FromResult(false);
        }
        catch (UnauthorizedAccessException)
        {
            return Task.FromResult(false);
        }
    }

    public Stream? Get(string hash)
    {
        ArgumentNullException.ThrowIfNull(hash);
        var destPath = _GetDestPath(hash);
        if (_correctMisplacedFile && _CorrectMisplacedFile(hash)) { }

        return File.Exists(destPath) ? _GetReadStream(destPath) : null;
    }

    public bool Exists(string hash)
    {
        ArgumentNullException.ThrowIfNull(hash);
        return File.Exists(_GetDestPath(hash)) || (_correctMisplacedFile && File.Exists(_GetMisplacedFilePath(hash)));
    }

    private string _GetDestPath(string hash)
    {
        var prefix = hash.Length >= _prefixLength ? hash[.._prefixLength] : hash;
        var dir = Path.Combine(_folder, prefix);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        return Path.Combine(dir, hash);
    }

    private string _GetMisplacedFilePath(string hash)
    {
        return Path.Combine(_folder, hash);
    }

    private bool _CorrectMisplacedFile(string hash)
    {
        var misplacedPath = _GetMisplacedFilePath(hash);
        if (!File.Exists(misplacedPath)) return false;

        var correctPath = _GetDestPath(hash);
        if (File.Exists(correctPath))
        {
            File.Delete(misplacedPath);
            return false;
        }

        File.Move(misplacedPath, correctPath);
        return true;
    }

    private Stream _GetSaveStream(string destPath)
    {
        var fs = File.Open(destPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        if (_compressObjects) return new DeflateStream(fs, CompressionMode.Compress);
        return fs;
    }

    private Stream _GetReadStream(string destPath)
    {
        var fs = File.Open(destPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (_compressObjects) return new DeflateStream(fs, CompressionMode.Decompress);
        return fs;
    }
}