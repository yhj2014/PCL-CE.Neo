using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft;

public class ForgeVersionHelper
{
    private readonly ILogger<ForgeVersionHelper> _logger;

    public ForgeVersionHelper(ILogger<ForgeVersionHelper> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool TryParseForgeVersion(string forgeVersion, out (string MinecraftVersion, string ForgeBuild)? result)
    {
        result = null;

        if (string.IsNullOrEmpty(forgeVersion))
            return false;

        var patterns = new[]
        {
            new Regex(@"^(\d+\.\d+(\.\d+)?)-forge-(\d+\.\d+\.\d+\.\d+)$"),
            new Regex(@"^(\d+\.\d+(\.\d+)?)-(\d+\.\d+\.\d+\.\d+)$"),
            new Regex(@"^(\d+\.\d+(\.\d+)?)-forge(\d+\.\d+\.\d+\.\d+)$"),
            new Regex(@"^forge-(\d+\.\d+(\.\d+)?)-(\d+\.\d+\.\d+\.\d+)$")
        };

        foreach (var pattern in patterns)
        {
            var match = pattern.Match(forgeVersion);
            if (match.Success)
            {
                var mcVersion = match.Groups[1].Value;
                var forgeBuild = match.Groups[match.Groups.Count - 1].Value;
                result = (mcVersion, forgeBuild);
                _logger.LogDebug("Parsed Forge version {ForgeVersion} -> MC: {MinecraftVersion}, Build: {ForgeBuild}", 
                    forgeVersion, mcVersion, forgeBuild);
                return true;
            }
        }

        _logger.LogWarning("Failed to parse Forge version: {ForgeVersion}", forgeVersion);
        return false;
    }

    public string FormatForgeVersion(string minecraftVersion, string forgeBuild)
    {
        if (string.IsNullOrEmpty(minecraftVersion))
            throw new ArgumentNullException(nameof(minecraftVersion));
        if (string.IsNullOrEmpty(forgeBuild))
            throw new ArgumentNullException(nameof(forgeBuild));

        return $"{minecraftVersion}-forge-{forgeBuild}";
    }

    public bool IsForgeVersion(string version)
    {
        if (string.IsNullOrEmpty(version))
            return false;

        return version.Contains("forge", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsFabricVersion(string version)
    {
        if (string.IsNullOrEmpty(version))
            return false;

        return version.Contains("fabric", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsQuiltVersion(string version)
    {
        if (string.IsNullOrEmpty(version))
            return false;

        return version.Contains("quilt", StringComparison.OrdinalIgnoreCase);
    }

    public string GetModLoader(string version)
    {
        if (string.IsNullOrEmpty(version))
            return "vanilla";

        if (IsForgeVersion(version))
            return "forge";

        if (IsFabricVersion(version))
            return "fabric";

        if (IsQuiltVersion(version))
            return "quilt";

        return "vanilla";
    }

    public string GetMinecraftVersionFromForge(string forgeVersion)
    {
        if (TryParseForgeVersion(forgeVersion, out var result))
        {
            return result.Value.MinecraftVersion;
        }

        return forgeVersion;
    }

    public string GetForgeBuildNumber(string forgeVersion)
    {
        if (TryParseForgeVersion(forgeVersion, out var result))
        {
            return result.Value.ForgeBuild;
        }

        return string.Empty;
    }

    public bool IsValidForgeVersion(string version)
    {
        return TryParseForgeVersion(version, out _);
    }

    public int CompareForgeVersions(string v1, string v2)
    {
        if (!TryParseForgeVersion(v1, out var r1) || !TryParseForgeVersion(v2, out var r2))
        {
            return string.Compare(v1, v2, StringComparison.OrdinalIgnoreCase);
        }

        var mcCompare = CompareVersionStrings(r1.Value.MinecraftVersion, r2.Value.MinecraftVersion);
        if (mcCompare != 0)
            return mcCompare;

        return CompareVersionStrings(r1.Value.ForgeBuild, r2.Value.ForgeBuild);
    }

    private int CompareVersionStrings(string v1, string v2)
    {
        var parts1 = v1.Split('.').Select(int.Parse).ToArray();
        var parts2 = v2.Split('.').Select(int.Parse).ToArray();

        for (int i = 0; i < Math.Max(parts1.Length, parts2.Length); i++)
        {
            var p1 = i < parts1.Length ? parts1[i] : 0;
            var p2 = i < parts2.Length ? parts2[i] : 0;

            if (p1 != p2)
                return p1.CompareTo(p2);
        }

        return 0;
    }

    public bool IsNewerForgeVersion(string v1, string v2)
    {
        return CompareForgeVersions(v1, v2) > 0;
    }

    public bool IsOlderForgeVersion(string v1, string v2)
    {
        return CompareForgeVersions(v1, v2) < 0;
    }

    public bool IsSameForgeVersion(string v1, string v2)
    {
        return CompareForgeVersions(v1, v2) == 0;
    }

    public string NormalizeVersion(string version)
    {
        if (string.IsNullOrEmpty(version))
            return version ?? string.Empty;

        if (TryParseForgeVersion(version, out var result))
        {
            return FormatForgeVersion(result.Value.MinecraftVersion, result.Value.ForgeBuild);
        }

        return version.Trim();
    }

    public IEnumerable<string> SortForgeVersions(IEnumerable<string> versions)
    {
        if (versions == null)
            return Enumerable.Empty<string>();

        return versions.OrderBy(v => v, Comparer<string>.Create(CompareForgeVersions));
    }

    public IEnumerable<string> SortForgeVersionsDescending(IEnumerable<string> versions)
    {
        if (versions == null)
            return Enumerable.Empty<string>();

        return versions.OrderByDescending(v => v, Comparer<string>.Create(CompareForgeVersions));
    }
}