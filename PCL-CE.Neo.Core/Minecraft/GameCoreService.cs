using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft;

/// <summary>
/// Service for managing Minecraft game core (jar) files.
/// Handles jar modification, library merging, and signature removal.
/// </summary>
public sealed class GameCoreService
{
    private readonly ILogger<GameCoreService> _logger;

    public GameCoreService() : this(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<GameCoreService>.Instance)
    {
    }

    public GameCoreService(ILogger<GameCoreService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Add jar file contents to the game core jar.
    /// Used for merging mod loader components into vanilla jar.
    /// </summary>
    /// <param name="coreJarPath">Path to the core game jar.</param>
    /// <param name="addJarPath">Path to the jar file to add.</param>
    /// <param name="filter">Filter pattern for entry names (empty = include all).</param>
    /// <returns>True if operation succeeded.</returns>
    public async Task<bool> AddToCoreAsync(string coreJarPath, string addJarPath, string filter = "")
    {
        try
        {
            if (!File.Exists(coreJarPath))
            {
                _logger.LogError("Core jar not found: {Path}", coreJarPath);
                return false;
            }

            if (!File.Exists(addJarPath))
            {
                _logger.LogError("Add jar not found: {Path}", addJarPath);
                return false;
            }

            _logger.LogInformation("Adding {AddPath} to core {CorePath} with filter '{Filter}'",
                addJarPath, coreJarPath, filter);

            await using var coreStream = new FileStream(
                coreJarPath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.Read,
                bufferSize: 16384,
                useAsync: true);

            await using var addStream = new FileStream(
                addJarPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 16384,
                useAsync: true);

            using var coreArchive = new ZipArchive(coreStream, ZipArchiveMode.Update);
            using var addArchive = new ZipArchive(addStream, ZipArchiveMode.Read);

            var entriesAdded = 0;

            foreach (var entry in addArchive.Entries)
            {
                if (!string.IsNullOrEmpty(filter) && !entry.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    var existingEntry = coreArchive.GetEntry(entry.FullName);
                    if (existingEntry != null)
                    {
                        existingEntry.Delete();
                    }

                    var newEntry = coreArchive.CreateEntry(entry.FullName);
                    await using var newEntryStream = newEntry.Open();
                    await using var sourceStream = entry.Open();
                    await sourceStream.CopyToAsync(newEntryStream);
                    entriesAdded++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to add entry {Name}: {Message}", entry.FullName, ex.Message);
                }
            }

            RemoveSignatureFiles(coreArchive);

            _logger.LogInformation("Added {Count} entries to core jar", entriesAdded);
            return entriesAdded > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add jar to core: {AddPath}", addJarPath);
            return false;
        }
    }

    /// <summary>
    /// Remove signature files from jar to prevent verification failures.
    /// Oracle JDK requires signature removal for modified jars.
    /// </summary>
    /// <param name="jarPath">Path to jar file.</param>
    /// <returns>True if signatures were removed.</returns>
    public async Task<bool> RemoveSignaturesAsync(string jarPath)
    {
        try
        {
            if (!File.Exists(jarPath))
            {
                _logger.LogError("Jar not found: {Path}", jarPath);
                return false;
            }

            _logger.LogInformation("Removing signatures from jar: {Path}", jarPath);

            await using var stream = new FileStream(
                jarPath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.Read,
                bufferSize: 16384,
                useAsync: true);

            using var archive = new ZipArchive(stream, ZipArchiveMode.Update);

            var removed = RemoveSignatureFiles(archive);

            _logger.LogInformation("Removed {Count} signature entries", removed);
            return removed > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove signatures from jar: {Path}", jarPath);
            return false;
        }
    }

    private int RemoveSignatureFiles(ZipArchive archive)
    {
        var removed = 0;
        var signaturePatterns = new[] { "META-INF/", ".RSA", ".DSA", ".SF", ".EC" };

        var entriesToRemove = archive.Entries
            .Where(entry => signaturePatterns.Any(pattern =>
                entry.FullName.StartsWith(pattern, StringComparison.OrdinalIgnoreCase) ||
                entry.FullName.EndsWith(pattern, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        foreach (var entry in entriesToRemove)
        {
            try
            {
                entry.Delete();
                removed++;
                _logger.LogDebug("Removed signature entry: {Name}", entry.FullName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to remove signature entry {Name}: {Message}", entry.FullName, ex.Message);
            }
        }

        return removed;
    }

    /// <summary>
    /// Check if jar contains valid game core (main class).
    /// </summary>
    /// <param name="jarPath">Path to jar file.</param>
    /// <returns>True if jar appears to be a valid game core.</returns>
    public bool IsValidGameCore(string jarPath)
    {
        try
        {
            if (!File.Exists(jarPath))
                return false;

            using var stream = new FileStream(jarPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            var mainClassEntry = archive.GetEntry("net/minecraft/client/main/Main.class");
            if (mainClassEntry != null)
                return true;

            var serverMainEntry = archive.GetEntry("net/minecraft/server/Main.class");
            if (serverMainEntry != null)
                return true;

            var legacyMainEntry = archive.GetEntry("com/mojang/minecraft/Minecraft.class");
            if (legacyMainEntry != null)
                return true;

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate game core: {Path}", jarPath);
            return false;
        }
    }

    /// <summary>
    /// Get version info from jar manifest.
    /// </summary>
    /// <param name="jarPath">Path to jar file.</param>
    /// <returns>Version string from manifest, or null if not found.</returns>
    public string? GetJarVersion(string jarPath)
    {
        try
        {
            if (!File.Exists(jarPath))
                return null;

            using var stream = new FileStream(jarPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            var manifestEntry = archive.GetEntry("META-INF/MANIFEST.MF");
            if (manifestEntry == null)
                return null;

            using var manifestStream = manifestEntry.Open();
            using var reader = new StreamReader(manifestStream);
            var content = reader.ReadToEnd();

            const string versionKey = "Implementation-Version:";
            var versionLine = content.Split('\n')
                .FirstOrDefault(line => line.StartsWith(versionKey, StringComparison.OrdinalIgnoreCase));

            if (versionLine != null)
            {
                return versionLine.Substring(versionKey.Length).Trim();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get jar version: {Path}", jarPath);
            return null;
        }
    }

    /// <summary>
    /// Extract specific files from jar to destination directory.
    /// </summary>
    /// <param name="jarPath">Path to jar file.</param>
    /// <param name="destination">Destination directory.</param>
    /// <param name="pattern">File pattern to extract (e.g., "*.class").</param>
    /// <returns>Number of files extracted.</returns>
    public async Task<int> ExtractFilesAsync(string jarPath, string destination, string pattern = "*")
    {
        try
        {
            if (!File.Exists(jarPath))
            {
                _logger.LogError("Jar not found: {Path}", jarPath);
                return 0;
            }

            Directory.CreateDirectory(destination);

            await using var stream = new FileStream(
                jarPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 16384,
                useAsync: true);

            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            var extracted = 0;
            foreach (var entry in archive.Entries)
            {
                if (!MatchesPattern(entry.Name, pattern))
                    continue;

                var destPath = Path.Combine(destination, entry.FullName);
                var destDir = Path.GetDirectoryName(destPath);

                if (!string.IsNullOrEmpty(destDir))
                    Directory.CreateDirectory(destDir);

                await using var entryStream = entry.Open();
                await using var destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 16384, true);
                await entryStream.CopyToAsync(destStream);

                extracted++;
            }

            _logger.LogInformation("Extracted {Count} files from jar to {Destination}", extracted, destination);
            return extracted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract files from jar: {Path}", jarPath);
            return 0;
        }
    }

    private static bool MatchesPattern(string name, string pattern)
    {
        if (pattern == "*" || pattern == "")
            return true;

        if (pattern.StartsWith("*") && pattern.EndsWith("*"))
        {
            var middle = pattern.Substring(1, pattern.Length - 2);
            return name.Contains(middle, StringComparison.OrdinalIgnoreCase);
        }

        if (pattern.StartsWith("*"))
        {
            return name.EndsWith(pattern.Substring(1), StringComparison.OrdinalIgnoreCase);
        }

        if (pattern.EndsWith("*"))
        {
            return name.StartsWith(pattern.Substring(0, pattern.Length - 1), StringComparison.OrdinalIgnoreCase);
        }

        return name.Equals(pattern, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Create a backup of the jar file.
    /// </summary>
    /// <param name="jarPath">Path to jar file.</param>
    /// <param name="backupPath">Backup destination path.</param>
    /// <returns>True if backup succeeded.</returns>
    public async Task<bool> CreateBackupAsync(string jarPath, string backupPath)
    {
        try
        {
            if (!File.Exists(jarPath))
            {
                _logger.LogError("Jar not found for backup: {Path}", jarPath);
                return false;
            }

            await using var sourceStream = new FileStream(
                jarPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 16384,
                useAsync: true);

            var backupDir = Path.GetDirectoryName(backupPath);
            if (!string.IsNullOrEmpty(backupDir))
                Directory.CreateDirectory(backupDir);

            await using var destStream = new FileStream(
                backupPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 16384,
                useAsync: true);

            await sourceStream.CopyToAsync(destStream);

            _logger.LogInformation("Created backup: {BackupPath}", backupPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create backup: {Path}", jarPath);
            return false;
        }
    }

    /// <summary>
    /// Restore jar from backup.
    /// </summary>
    /// <param name="backupPath">Backup file path.</param>
    /// <param name="jarPath">Destination jar path.</param>
    /// <returns>True if restore succeeded.</returns>
    public async Task<bool> RestoreFromBackupAsync(string backupPath, string jarPath)
    {
        try
        {
            if (!File.Exists(backupPath))
            {
                _logger.LogError("Backup not found: {Path}", backupPath);
                return false;
            }

            await using var sourceStream = new FileStream(
                backupPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 16384,
                useAsync: true);

            await using var destStream = new FileStream(
                jarPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 16384,
                useAsync: true);

            await sourceStream.CopyToAsync(destStream);

            _logger.LogInformation("Restored from backup: {BackupPath} -> {JarPath}", backupPath, jarPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore from backup: {BackupPath}", backupPath);
            return false;
        }
    }
}