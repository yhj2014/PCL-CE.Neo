using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Minecraft.Java;

namespace PCL_CE.Neo.Core.Minecraft;

/// <summary>
/// Manages Java installations discovery, validation, and selection.
/// </summary>
public sealed partial class JavaManager
{
    private const string ModuleName = "JavaManager";
    private readonly ILogger<JavaManager> _logger;
    private readonly IJavaParser _parser;
    private readonly List<IJavaScanner> _scanners;
    private readonly ConcurrentDictionary<string, JavaEntry> _javaEntries = new();
    private readonly SemaphoreSlim _scanLock = new(1, 1);
    private DateTime _lastScanTime = DateTime.MinValue;
    private static readonly TimeSpan MinScanInterval = TimeSpan.FromSeconds(JavaConsts.MinScanIntervalSeconds);

    /// <summary>
    /// Get all discovered Java entries.
    /// </summary>
    public IReadOnlyDictionary<string, JavaEntry> JavaEntries => _javaEntries;

    /// <summary>
    /// Count of discovered Java installations.
    /// </summary>
    public int Count => _javaEntries.Count;

    public JavaManager() : this(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<JavaManager>.Instance,
        new PeHeaderParser(),
        new DefaultPathsScanner(),
        new PathEnvironmentScanner(),
        new WhereCommandScanner())
    {
    }

    public JavaManager(
        ILogger<JavaManager> logger,
        IJavaParser parser,
        params IJavaScanner[] scanners)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<JavaManager>.Instance;
        _parser = parser ?? new PeHeaderParser();
        _scanners = scanners?.ToList() ?? new List<IJavaScanner>();

        if (_scanners.Count == 0)
        {
            _scanners.Add(new DefaultPathsScanner());
            _scanners.Add(new PathEnvironmentScanner());
            _scanners.Add(new WhereCommandScanner());
        }

        _logger.LogInformation("JavaManager initialized with {Count} scanners", _scanners.Count);
    }

    /// <summary>
    /// Save Java configuration to storage.
    /// </summary>
    public void SaveConfig(Action<string, string> saveAction)
    {
        try
        {
            var items = _javaEntries.Values
                .Select(entry => new JavaStorageItem
                {
                    Path = entry.Installation.JavaExePath,
                    IsEnable = entry.IsEnabled,
                    Source = entry.Source
                })
                .ToArray();

            var json = JsonSerializer.Serialize(items);
            saveAction("JavaList", json);

            _logger.LogInformation("Saved {Count} Java entries to configuration", items.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save Java configuration");
        }
    }

    /// <summary>
    /// Read Java configuration from storage.
    /// </summary>
    public void ReadConfig(Func<string, string?> getConfig)
    {
        try
        {
            var json = getConfig("JavaList");
            if (string.IsNullOrEmpty(json))
                return;

            var items = JsonSerializer.Deserialize<JavaStorageItem[]>(json);
            if (items == null || items.Length == 0)
                return;

            var itemsAdded = new List<JavaEntry>();

            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.Path))
                    continue;

                var installation = _parser.Parse(item.Path);
                if (installation == null)
                {
                    _logger.LogDebug("Cannot parse Java from saved config: {Path}", item.Path);
                    continue;
                }

                itemsAdded.Add(new JavaEntry
                {
                    Installation = installation,
                    IsEnabled = item.IsEnable,
                    Source = item.Source
                });
            }

            lock (_javaEntries)
            {
                foreach (var item in itemsAdded)
                {
                    var key = NormalizePath(item.Installation.JavaExePath);
                    if (_javaEntries.TryGetValue(key, out var existing))
                    {
                        existing.IsEnabled = item.IsEnabled;
                        existing.Source = item.Source;
                    }
                    else
                    {
                        _javaEntries[key] = item;
                    }
                }
            }

            _logger.LogInformation("Loaded {Count} Java entries from configuration", itemsAdded.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read Java configuration");
        }
    }

    /// <summary>
    /// Scan for Java installations.
    /// </summary>
    /// <param name="force">Force scan regardless of time interval.</param>
    public async Task ScanJavaAsync(bool force = false)
    {
        if (ShouldSkipScan() && !force)
        {
            _logger.LogDebug("Skipping Java scan (minimum interval not reached)");
            return;
        }

        if (!await _scanLock.WaitAsync(TimeSpan.FromSeconds(7)))
        {
            _logger.LogDebug("Skipping Java scan (lock timeout)");
            return;
        }

        try
        {
            if (ShouldSkipScan() && !force)
                return;

            await Task.Run(ScanInternal);
            _lastScanTime = DateTime.Now;

            _logger.LogInformation("Java scan completed, found {Count} installations", _javaEntries.Count);
        }
        finally
        {
            _scanLock.Release();
        }
    }

    private bool ShouldSkipScan()
    {
        return (DateTime.Now - _lastScanTime) < MinScanInterval;
    }

    private void ScanInternal()
    {
        var pathSet = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        Parallel.ForEach(_scanners, scanner =>
        {
            try
            {
                var temp = new List<string>();
                scanner.Scan(temp);

                foreach (var path in temp)
                {
                    var normalized = NormalizePath(path);
                    if (!ShouldExcludePath(normalized))
                    {
                        pathSet.TryAdd(normalized, true);
                    }
                }

                _logger.LogDebug("Scanner {Name} found {Count} Java paths", scanner.Name, temp.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scanner {Name} failed", scanner.Name);
            }
        });

        var scannedEntries = pathSet.Keys
            .Select(path => _parser.Parse(path))
            .Where(inst => inst != null)
            .Select(inst => new JavaEntry
            {
                Installation = inst!,
                IsEnabled = _javaEntries.TryGetValue(NormalizePath(inst!.JavaExePath), out var existing)
                    ? existing.IsEnabled
                    : ShouldEnableByDefault(inst!),
                Source = JavaSource.AutoScanned
            })
            .ToList();

        lock (_javaEntries)
        {
            foreach (var entry in scannedEntries)
            {
                var key = NormalizePath(entry.Installation.JavaExePath);
                _javaEntries[key] = entry;
            }
        }
    }

    private static bool ShouldEnableByDefault(JavaInstallation inst)
    {
        var libDir = Path.Combine(Directory.GetParent(inst.JavaFolder)?.FullName ?? inst.JavaFolder, "lib");
        var isUsable = (!inst.IsJre && File.Exists(Path.Combine(libDir, "jvm.lib"))) ||
                       (inst.IsJre && File.Exists(Path.Combine(libDir, "rt.jar")));

        return !((inst.IsJre && inst.MajorVersion > 8) ||
                 (inst.Is64Bit ^ Environment.Is64BitOperatingSystem) ||
                 !isUsable);
    }

    /// <summary>
    /// Get sorted list of Java entries by version (descending).
    /// </summary>
    public List<JavaEntry> GetSortedJavaList()
    {
        var ret = _javaEntries.Values.ToList();
        ret.Sort((a, b) =>
        {
            var versionCmp = a.Installation.Version.CompareTo(b.Installation.Version);
            if (versionCmp != 0) return versionCmp;
            return a.Installation.Brand.CompareTo(b.Installation.Brand);
        });
        ret.Reverse();
        return ret;
    }

    /// <summary>
    /// Check if any 64-bit Java exists.
    /// </summary>
    public bool Existing64BitJava()
    {
        return _javaEntries.Any(x => x.Value.Installation.Is64Bit);
    }

    /// <summary>
    /// Check if any Java exists.
    /// </summary>
    public bool ExistAnyJava()
    {
        return _javaEntries.Count > 0;
    }

    /// <summary>
    /// Check if specific Java path exists in registry.
    /// </summary>
    public bool Exist(string javaExePath)
    {
        return _javaEntries.ContainsKey(NormalizePath(javaExePath));
    }

    /// <summary>
    /// Add or get Java entry for specified path.
    /// </summary>
    public JavaEntry? AddOrGet(string javaExePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(javaExePath) || !File.Exists(javaExePath))
                return null;

            var installation = _parser.Parse(javaExePath);
            if (installation == null)
                return null;

            var exePath = NormalizePath(installation.JavaExePath);

            lock (_javaEntries)
            {
                if (_javaEntries.TryGetValue(exePath, out var existing))
                    return existing;

                var entry = new JavaEntry
                {
                    Installation = installation,
                    IsEnabled = ShouldEnableByDefault(installation),
                    Source = JavaSource.ManualAdded
                };

                _javaEntries[exePath] = entry;
                _logger.LogInformation("Added Java: {DisplayName}", entry.DisplayName);
                return entry;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add Java: {Path}", javaExePath);
            return null;
        }
    }

    /// <summary>
    /// Get Java entry without adding if not exists.
    /// </summary>
    public JavaEntry? Get(string javaExePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(javaExePath) || !File.Exists(javaExePath))
                return null;

            var normalized = NormalizePath(javaExePath);
            return _javaEntries.TryGetValue(normalized, out var entry) ? entry : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Java: {Path}", javaExePath);
            return null;
        }
    }

    /// <summary>
    /// Select suitable Java for specified version range.
    /// </summary>
    public async Task<JavaEntry[]> SelectSuitableJavaAsync(Version minVersion, Version maxVersion)
    {
        if (_javaEntries.Count == 0)
            await ScanJavaAsync();

        return _javaEntries.Values
            .Where(j => j.Installation.IsStillAvailable && j.IsEnabled &&
                        IsVersionSuitable(j.Installation.Version, minVersion, maxVersion))
            .OrderBy(static j => j.Installation.MajorVersion)
            .ThenBy(static j => j.Installation.IsJre)
            .ThenBy(static j => j.Installation.Brand)
            .ThenByDescending(static j => j.Installation.Version)
            .ToArray();
    }

    /// <summary>
    /// Select suitable Java for Minecraft version.
    /// </summary>
    public JavaEntry? SelectJavaForMcVersion(string mcVersion)
    {
        var requiredVersion = GetRequiredJavaVersion(mcVersion);
        return _javaEntries.Values
            .Where(j => j.IsEnabled && j.Installation.IsStillAvailable && j.Installation.Is64Bit &&
                        j.Installation.MajorVersion >= requiredVersion)
            .OrderByDescending(j => j.Installation.Brand)
            .ThenByDescending(j => !j.Installation.IsJre)
            .ThenByDescending(j => j.Installation.Version)
            .FirstOrDefault();
    }

    private static int GetRequiredJavaVersion(string mcVersion)
    {
        foreach (var kvp in JavaConsts.MinJavaVersionByMcVersion)
        {
            if (mcVersion.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }
        return 8;
    }

    /// <summary>
    /// Check all Java availability and remove unavailable entries.
    /// </summary>
    public void CheckAllAvailability()
    {
        lock (_javaEntries)
        {
            var keysToRemove = _javaEntries
                .Where(kv => !kv.Value.Installation.IsStillAvailable)
                .Select(kv => kv.Key)
                .ToArray();

            foreach (var key in keysToRemove)
            {
                _javaEntries.TryRemove(key, out _);
            }

            if (keysToRemove.Length > 0)
            {
                _logger.LogInformation("Removed {Count} unavailable Java entries", keysToRemove.Length);
            }
        }
    }

    /// <summary>
    /// Normalize version to standard format (1.8.0 -> 8.0.0).
    /// </summary>
    public static Version NormalizeVersion(Version version)
    {
        return version.Major == 1 && version.Minor >= 0
            ? new Version(version.Minor, Math.Max(version.Build, 0), Math.Max(version.Revision, 0))
            : version;
    }

    /// <summary>
    /// Check if version is suitable for specified range.
    /// </summary>
    public static bool IsVersionSuitable(Version javaVersion, Version minVersion, Version maxVersion)
    {
        var normalizedJava = NormalizeVersion(javaVersion);
        var normalizedMin = NormalizeVersion(minVersion);
        var normalizedMax = NormalizeVersion(maxVersion);

        return normalizedJava >= normalizedMin && normalizedJava <= normalizedMax;
    }

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static bool ShouldExcludePath(string path)
    {
        return path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(part => JavaConsts.ExcludeFolderNames.Contains(part, StringComparer.OrdinalIgnoreCase));
    }
}