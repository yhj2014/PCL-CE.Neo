using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Minecraft.Java;
using PCL_CE.Neo.Core.Minecraft.Java.Parser;
using PCL_CE.Neo.Core.Minecraft.Java.Scanner;

namespace PCL_CE.Neo.Core.Minecraft;

public class JavaManager
{
    private readonly Dictionary<string, JavaEntry> _javaEntries = new();
    private readonly IJavaParser _parser;
    private readonly IJavaScanner[] _scanners;
    private readonly SemaphoreSlim _scanLock = new(1, 1);
    private DateTime _lastScanTime = DateTime.MinValue;
    private static readonly TimeSpan _MinScanInterval = TimeSpan.FromSeconds(13);

    public JavaManager(IJavaParser parser, params IJavaScanner[] scanners)
    {
        _parser = parser;
        _scanners = scanners;
    }

    public void SaveConfig(string configPath)
    {
        try
        {
            var items = _javaEntries
                .Select(x => new JavaStorageItem
                {
                    Path = x.Value.Installation.JavaExePath,
                    IsEnable = x.Value.IsEnabled,
                    Source = x.Value.Source
                })
                .ToArray();

            var dir = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(configPath, JsonSerializer.Serialize(items));
        }
        catch (Exception)
        {
        }
    }

    public void ReadConfig(string configPath)
    {
        try
        {
            if (!File.Exists(configPath))
                return;

            var json = File.ReadAllText(configPath);
            var items = JsonSerializer.Deserialize<JavaStorageItem[]>(json);
            if (items == null)
                return;

            var itemsAdded = new List<JavaEntry>();

            foreach (var item in items)
            {
                var parserResult = _parser.Parse(item.Path);
                if (parserResult == null)
                    continue;

                itemsAdded.Add(new JavaEntry
                {
                    Installation = parserResult,
                    IsEnabled = item.IsEnable,
                    Source = item.Source ?? JavaSource.AutoScanned
                });
            }

            lock (_javaEntries)
            {
                foreach (var item in itemsAdded)
                {
                    var key = _NormalizePath(item.Installation.JavaExePath);
                    if (_javaEntries.TryGetValue(key, out var existingRecord))
                    {
                        existingRecord.IsEnabled = item.IsEnabled;
                        existingRecord.Source = item.Source;
                    }
                    else
                    {
                        _javaEntries.Add(key, item);
                    }
                }
            }
        }
        catch (Exception)
        {
        }
    }

    public async Task ScanJavaAsync(bool force = false)
    {
        if (ShouldSkip())
            return;

        if (!await _scanLock.WaitAsync(TimeSpan.FromSeconds(7)))
            return;

        try
        {
            if (ShouldSkip())
                return;

            await Task.Run(_ScanInternal);
            _lastScanTime = DateTime.Now;
        }
        finally
        {
            _scanLock.Release();
        }

        bool ShouldSkip()
        {
            return !force && (DateTime.Now - _lastScanTime) < _MinScanInterval;
        }
    }

    private void _ScanInternal()
    {
        var pathSet = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        Parallel.ForEach(_scanners, scanner =>
        {
            var temp = new List<string>();
            scanner.Scan(temp);
            foreach (var path in temp)
            {
                var normalized = _NormalizePath(path);
                if (!_ShouldExcludePath(normalized))
                    pathSet.TryAdd(normalized, true);
            }
        });

        var scannedEntries = pathSet.Keys
            .Select(_parser.Parse)
            .Where(inst => inst != null)
            .Select(inst => new JavaEntry
            {
                Installation = inst!,
                IsEnabled = _javaEntries.TryGetValue(_NormalizePath(inst!.JavaExePath), out var existingJava)
                    ? existingJava.IsEnabled
                    : _ShouldEnableByDefault(inst!),
                Source = JavaSource.AutoScanned
            })
            .ToList();

        lock (_javaEntries)
        {
            foreach (var entry in scannedEntries)
            {
                _javaEntries[_NormalizePath(entry.Installation.JavaExePath)] = entry;
            }
        }
    }

    private bool _ShouldEnableByDefault(JavaInstallation inst)
    {
        return !((inst.IsJre && inst.MajorVersion > 8) ||
                 (inst.Is64Bit ^ Environment.Is64BitOperatingSystem));
    }

    public List<JavaEntry> GetSortedJavaList()
    {
        var ret = _javaEntries.Values.ToList();
        ret.Sort((a, b) =>
        {
            var versionCmp = a.Installation.Version.CompareTo(b.Installation.Version);
            if (versionCmp != 0)
                return versionCmp;
            return a.Installation.Brand - b.Installation.Brand;
        });
        ret.Reverse();
        return ret;
    }

    public bool Existing64BitJava()
    {
        lock (_javaEntries)
        {
            return _javaEntries.Any(x => x.Value.Installation.Is64Bit);
        }
    }

    public bool ExistAnyJava()
    {
        return _javaEntries.Count != 0;
    }

    public bool Exist(string javaExePath)
    {
        return _javaEntries.ContainsKey(_NormalizePath(javaExePath));
    }

    public JavaEntry? AddOrGet(string javaExePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(javaExePath) || !File.Exists(javaExePath))
                return null;

            var installation = _parser.Parse(javaExePath);
            if (installation == null)
                return null;

            var exePath = _NormalizePath(installation.JavaExePath);
            lock (_javaEntries)
            {
                if (_javaEntries.TryGetValue(exePath, out var ret))
                    return ret;

                var entry = new JavaEntry
                {
                    Installation = installation,
                    IsEnabled = _ShouldEnableByDefault(installation),
                    Source = JavaSource.ManualAdded
                };

                _javaEntries.Add(exePath, entry);
                return entry;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public JavaEntry? Get(string javaExePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(javaExePath) || !File.Exists(javaExePath))
                return null;

            var installation = _parser.Parse(javaExePath);
            if (installation == null)
                return null;

            var exePath = _NormalizePath(installation.JavaExePath);
            lock (_javaEntries)
            {
                if (_javaEntries.TryGetValue(exePath, out var ret))
                    return ret;

                return new JavaEntry
                {
                    Installation = installation,
                    IsEnabled = _ShouldEnableByDefault(installation),
                    Source = JavaSource.ManualAdded
                };
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<JavaEntry[]> SelectSuitableJavaAsync(Version minVersion, Version maxVersion)
    {
        if (_javaEntries.Count == 0)
            await ScanJavaAsync();

        lock (_javaEntries)
        {
            return _javaEntries
                .Values.ToList()
                .Where(j => j.Installation.IsStillAvailable && j.IsEnabled &&
                            IsVersionSuitable(j.Installation.Version, minVersion, maxVersion))
                .OrderBy(j => j.Installation.MajorVersion)
                .ThenBy(j => j.Installation.IsJre)
                .ThenBy(j => j.Installation.Brand)
                .ThenByDescending(j => j.Installation.Version)
                .ToArray();
        }
    }

    public void CheckAllAvailability()
    {
        lock (_javaEntries)
        {
            var keysToRemove = _javaEntries
                .Where(kv => !kv.Value.Installation.IsStillAvailable)
                .Select(kv => kv.Key)
                .ToArray();
            foreach (var key in keysToRemove)
                _javaEntries.Remove(key);
        }
    }

    private static string _NormalizePath(string path) =>
        Path.GetFullPath(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    private static bool _ShouldExcludePath(string path) =>
        path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(part => JavaConsts.ExcludeFolderNames.Contains(part, StringComparer.OrdinalIgnoreCase));

    public static Version NormalizeVersion(Version version) =>
        version.Major == 1 && version.Minor >= 0
            ? new Version(version.Minor, Math.Max(version.Build, 0), Math.Max(version.Revision, 0))
            : version;

    public static bool IsVersionSuitable(Version javaVersion, Version minVersion, Version maxVersion)
    {
        var normalizedJava = NormalizeVersion(javaVersion);
        var normalizedMin = NormalizeVersion(minVersion);
        var normalizedMax = NormalizeVersion(maxVersion);

        return normalizedJava >= normalizedMin && normalizedJava <= normalizedMax;
    }
}