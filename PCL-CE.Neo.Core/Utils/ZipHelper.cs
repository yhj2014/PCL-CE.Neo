using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Utils;

public class ZipHelper
{
    private readonly ILogger<ZipHelper> _logger;

    public ZipHelper(ILogger<ZipHelper> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> CreateZipAsync(string sourceDirectory, string zipFilePath, 
        IEnumerable<string>? fileFilter = null, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            _logger.LogError("Source directory does not exist: {SourceDirectory}", sourceDirectory);
            return false;
        }

        try
        {
            var targetDir = Path.GetDirectoryName(zipFilePath);
            if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            using var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create);

            var allFiles = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories);

            if (fileFilter != null)
            {
                var filters = fileFilter.ToHashSet();
                allFiles = allFiles.Where(f => filters.Any(filter => f.EndsWith(filter, StringComparison.OrdinalIgnoreCase))).ToArray();
            }

            foreach (var file in allFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var entryName = file.Substring(sourceDirectory.Length).TrimStart(Path.DirectorySeparatorChar);
                archive.CreateEntryFromFile(file, entryName);
            }

            _logger.LogInformation("Successfully created zip file: {ZipFilePath}", zipFilePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create zip file: {ZipFilePath}", zipFilePath);
            return false;
        }
    }

    public bool CreateZip(string sourceDirectory, string zipFilePath, IEnumerable<string>? fileFilter = null)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            _logger.LogError("Source directory does not exist: {SourceDirectory}", sourceDirectory);
            return false;
        }

        try
        {
            var targetDir = Path.GetDirectoryName(zipFilePath);
            if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            using var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create);

            var allFiles = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories);

            if (fileFilter != null)
            {
                var filters = fileFilter.ToHashSet();
                allFiles = allFiles.Where(f => filters.Any(filter => f.EndsWith(filter, StringComparison.OrdinalIgnoreCase))).ToArray();
            }

            foreach (var file in allFiles)
            {
                var entryName = file.Substring(sourceDirectory.Length).TrimStart(Path.DirectorySeparatorChar);
                archive.CreateEntryFromFile(file, entryName);
            }

            _logger.LogInformation("Successfully created zip file: {ZipFilePath}", zipFilePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create zip file: {ZipFilePath}", zipFilePath);
            return false;
        }
    }

    public async Task<bool> ExtractZipAsync(string zipFilePath, string targetDirectory, 
        bool overwrite = true, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(zipFilePath))
        {
            _logger.LogError("Zip file does not exist: {ZipFilePath}", zipFilePath);
            return false;
        }

        try
        {
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            using var archive = ZipFile.OpenRead(zipFilePath);

            foreach (var entry in archive.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var entryPath = Path.Combine(targetDirectory, entry.FullName);

                if (entry.FullName.EndsWith("/"))
                {
                    Directory.CreateDirectory(entryPath);
                    continue;
                }

                var entryDir = Path.GetDirectoryName(entryPath);
                if (!string.IsNullOrEmpty(entryDir) && !Directory.Exists(entryDir))
                {
                    Directory.CreateDirectory(entryDir);
                }

                if (!overwrite && File.Exists(entryPath))
                {
                    continue;
                }

                entry.ExtractToFile(entryPath, overwrite);
            }

            _logger.LogInformation("Successfully extracted zip file: {ZipFilePath} to {TargetDirectory}", 
                zipFilePath, targetDirectory);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract zip file: {ZipFilePath}", zipFilePath);
            return false;
        }
    }

    public bool ExtractZip(string zipFilePath, string targetDirectory, bool overwrite = true)
    {
        if (!File.Exists(zipFilePath))
        {
            _logger.LogError("Zip file does not exist: {ZipFilePath}", zipFilePath);
            return false;
        }

        try
        {
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            using var archive = ZipFile.OpenRead(zipFilePath);

            foreach (var entry in archive.Entries)
            {
                var entryPath = Path.Combine(targetDirectory, entry.FullName);

                if (entry.FullName.EndsWith("/"))
                {
                    Directory.CreateDirectory(entryPath);
                    continue;
                }

                var entryDir = Path.GetDirectoryName(entryPath);
                if (!string.IsNullOrEmpty(entryDir) && !Directory.Exists(entryDir))
                {
                    Directory.CreateDirectory(entryDir);
                }

                if (!overwrite && File.Exists(entryPath))
                {
                    continue;
                }

                entry.ExtractToFile(entryPath, overwrite);
            }

            _logger.LogInformation("Successfully extracted zip file: {ZipFilePath} to {TargetDirectory}", 
                zipFilePath, targetDirectory);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract zip file: {ZipFilePath}", zipFilePath);
            return false;
        }
    }

    public async Task<bool> ExtractFileFromZipAsync(string zipFilePath, string entryName, 
        string targetFilePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(zipFilePath))
        {
            _logger.LogError("Zip file does not exist: {ZipFilePath}", zipFilePath);
            return false;
        }

        try
        {
            using var archive = ZipFile.OpenRead(zipFilePath);

            var entry = archive.GetEntry(entryName);
            if (entry == null)
            {
                _logger.LogError("Entry not found in zip: {EntryName}", entryName);
                return false;
            }

            var targetDir = Path.GetDirectoryName(targetFilePath);
            if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            await Task.Run(() => entry.ExtractToFile(targetFilePath, true), cancellationToken);

            _logger.LogInformation("Successfully extracted entry: {EntryName} to {TargetFilePath}", 
                entryName, targetFilePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract entry from zip: {EntryName}", entryName);
            return false;
        }
    }

    public bool ExtractFileFromZip(string zipFilePath, string entryName, string targetFilePath)
    {
        if (!File.Exists(zipFilePath))
        {
            _logger.LogError("Zip file does not exist: {ZipFilePath}", zipFilePath);
            return false;
        }

        try
        {
            using var archive = ZipFile.OpenRead(zipFilePath);

            var entry = archive.GetEntry(entryName);
            if (entry == null)
            {
                _logger.LogError("Entry not found in zip: {EntryName}", entryName);
                return false;
            }

            var targetDir = Path.GetDirectoryName(targetFilePath);
            if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            entry.ExtractToFile(targetFilePath, true);

            _logger.LogInformation("Successfully extracted entry: {EntryName} to {TargetFilePath}", 
                entryName, targetFilePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract entry from zip: {EntryName}", entryName);
            return false;
        }
    }

    public bool IsValidZip(string zipFilePath)
    {
        if (!File.Exists(zipFilePath))
            return false;

        try
        {
            using var archive = ZipFile.OpenRead(zipFilePath);
            return archive.Entries.Any();
        }
        catch
        {
            return false;
        }
    }

    public List<string> GetZipEntries(string zipFilePath)
    {
        var entries = new List<string>();

        if (!File.Exists(zipFilePath))
            return entries;

        try
        {
            using var archive = ZipFile.OpenRead(zipFilePath);
            entries.AddRange(archive.Entries.Select(e => e.FullName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read zip entries: {ZipFilePath}", zipFilePath);
        }

        return entries;
    }

    public long GetZipSize(string zipFilePath)
    {
        if (!File.Exists(zipFilePath))
            return 0;

        try
        {
            using var archive = ZipFile.OpenRead(zipFilePath);
            return archive.Entries.Sum(e => e.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get zip size: {ZipFilePath}", zipFilePath);
            return 0;
        }
    }

    public int GetZipEntryCount(string zipFilePath)
    {
        if (!File.Exists(zipFilePath))
            return 0;

        try
        {
            using var archive = ZipFile.OpenRead(zipFilePath);
            return archive.Entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get zip entry count: {ZipFilePath}", zipFilePath);
            return 0;
        }
    }

    public bool ContainsEntry(string zipFilePath, string entryName)
    {
        if (!File.Exists(zipFilePath))
            return false;

        try
        {
            using var archive = ZipFile.OpenRead(zipFilePath);
            return archive.GetEntry(entryName) != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check zip entry: {ZipFilePath}", zipFilePath);
            return false;
        }
    }
}