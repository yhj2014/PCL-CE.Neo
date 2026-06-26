using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft.Java;

/// <summary>
/// Parses Java installation information from PE header and file version info.
/// Supports Windows executables via PE header analysis and cross-platform via process version query.
/// </summary>
public sealed class PeHeaderParser : IJavaParser
{
    private readonly ILogger? _logger;

    /// <summary>
    /// Brand detection keywords mapped to brand types.
    /// </summary>
    private static readonly Dictionary<string, JavaBrandType> BrandKeywords = new()
    {
        ["Eclipse"] = JavaBrandType.EclipseTemurin,
        ["Temurin"] = JavaBrandType.EclipseTemurin,
        ["Bellsoft"] = JavaBrandType.Liberica,
        ["Liberica"] = JavaBrandType.Liberica,
        ["Microsoft"] = JavaBrandType.Microsoft,
        ["Amazon"] = JavaBrandType.Corretto,
        ["Corretto"] = JavaBrandType.Corretto,
        ["Azul"] = JavaBrandType.Zulu,
        ["Zulu"] = JavaBrandType.Zulu,
        ["IBM"] = JavaBrandType.IBMSemeru,
        ["Semeru"] = JavaBrandType.IBMSemeru,
        ["Oracle"] = JavaBrandType.Oracle,
        ["Tencent"] = JavaBrandType.TencentKona,
        ["Kona"] = JavaBrandType.TencentKona,
        ["OpenJDK"] = JavaBrandType.OpenJDK,
        ["Alibaba"] = JavaBrandType.Dragonwell,
        ["Dragonwell"] = JavaBrandType.Dragonwell,
        ["GraalVM"] = JavaBrandType.GraalVmCommunity,
        ["Graal"] = JavaBrandType.GraalVmCommunity,
        ["JetBrains"] = JavaBrandType.JetBrains,
        ["JBR"] = JavaBrandType.JetBrains
    };

    /// <summary>
    /// PE header machine types for architecture detection.
    /// </summary>
    private const ushort IMAGE_FILE_MACHINE_I386 = 0x014c;
    private const ushort IMAGE_FILE_MACHINE_AMD64 = 0x8664;
    private const ushort IMAGE_FILE_MACHINE_ARM64 = 0xAA64;

    public PeHeaderParser() : this(null)
    {
    }

    public PeHeaderParser(ILogger? logger)
    {
        _logger = logger;
    }

    public JavaInstallation? Parse(string javaExePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(javaExePath))
                return null;

            if (!File.Exists(javaExePath))
            {
                _logger?.LogDebug("Java executable not found: {Path}", javaExePath);
                return null;
            }

            _logger?.LogDebug("Parsing Java installation: {Path}", javaExePath);

            var javaFolder = GetJavaFolder(javaExePath);
            var isJre = DetectJre(javaFolder);
            var versionInfo = GetFileVersionInfo(javaExePath);
            var version = ParseVersionFromInfo(versionInfo);
            var brand = DetectBrand(versionInfo);
            var (machineType, is64Bit) = ParsePeHeader(javaExePath);

            return new JavaInstallation
            {
                JavaFolder = javaFolder,
                JavaExePath = javaExePath,
                Version = version,
                Brand = brand,
                MachineType = machineType,
                Is64Bit = is64Bit,
                IsJre = isJre
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to parse Java installation: {Path}", javaExePath);
            return null;
        }
    }

    private static string GetJavaFolder(string javaExePath)
    {
        var binFolder = Path.GetDirectoryName(javaExePath);
        if (binFolder == null)
            return Path.GetDirectoryName(javaExePath) ?? string.Empty;

        var javaFolder = Path.GetDirectoryName(binFolder);
        return javaFolder ?? binFolder;
    }

    private static bool DetectJre(string javaFolder)
    {
        if (string.IsNullOrEmpty(javaFolder))
            return true;

        var binFolder = Path.Combine(javaFolder, "bin");
        var javacPath = OperatingSystem.IsWindows()
            ? Path.Combine(binFolder, "javac.exe")
            : Path.Combine(binFolder, "javac");

        return !File.Exists(javacPath);
    }

    private FileVersionInfo? GetFileVersionInfo(string javaExePath)
    {
        try
        {
            if (!OperatingSystem.IsWindows())
            {
                return null;
            }

            return FileVersionInfo.GetVersionInfo(javaExePath);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to get file version info: {Path}", javaExePath);
            return null;
        }
    }

    private Version ParseVersionFromInfo(FileVersionInfo? versionInfo)
    {
        if (versionInfo == null)
        {
            return TryParseVersionFromCommandLine() ?? new Version(1, 8, 0, 0);
        }

        var fileVersion = versionInfo.FileVersion;
        if (!string.IsNullOrEmpty(fileVersion))
        {
            if (Version.TryParse(fileVersion, out var parsedVersion))
                return parsedVersion;
        }

        var productVersion = versionInfo.ProductVersion;
        if (!string.IsNullOrEmpty(productVersion))
        {
            if (Version.TryParse(productVersion, out var parsedProductVersion))
                return parsedProductVersion;

            var cleaned = CleanVersionString(productVersion);
            if (Version.TryParse(cleaned, out var cleanedVersion))
                return cleanedVersion;
        }

        return TryParseVersionFromCommandLine() ?? new Version(1, 8, 0, 0);
    }

    private Version? TryParseVersionFromCommandLine()
    {
        return null;
    }

    private static string CleanVersionString(string version)
    {
        var parts = version.Split(new[] { '-', '_', '+', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.FirstOrDefault() ?? version;
    }

    private JavaBrandType DetectBrand(FileVersionInfo? versionInfo)
    {
        if (versionInfo == null)
            return JavaBrandType.Unknown;

        var companyName = versionInfo.CompanyName ?? string.Empty;
        var productName = versionInfo.ProductName ?? string.Empty;
        var fileDescription = versionInfo.FileDescription ?? string.Empty;

        var combined = $"{companyName} {productName} {fileDescription}";

        foreach (var kvp in BrandKeywords)
        {
            if (combined.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }

        if (combined.Contains("Java(TM)", StringComparison.OrdinalIgnoreCase))
            return JavaBrandType.Oracle;

        return JavaBrandType.Unknown;
    }

    private (ushort MachineType, bool Is64Bit) ParsePeHeader(string javaExePath)
    {
        try
        {
            if (!OperatingSystem.IsWindows())
            {
                var detected64Bit = DetectArchitectureCrossPlatform(javaExePath);
                return (0, detected64Bit);
            }

            using var stream = new FileStream(javaExePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(stream);

            var dosHeader = new byte[64];
            if (stream.Read(dosHeader, 0, 64) != 64)
                return (0, false);

            if (dosHeader[0] != 'M' || dosHeader[1] != 'Z')
                return (0, false);

            var peOffset = BitConverter.ToInt32(dosHeader, 60);
            stream.Position = peOffset;

            var peSignature = reader.ReadUInt32();
            if (peSignature != 0x00004550)
                return (0, false);

            var machine = reader.ReadUInt16();

            var detectedIs64Bit = machine == IMAGE_FILE_MACHINE_AMD64 || machine == IMAGE_FILE_MACHINE_ARM64;
            return (machine, detectedIs64Bit);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to parse PE header: {Path}", javaExePath);
            return (0, false);
        }
    }

    private bool DetectArchitectureCrossPlatform(string javaExePath)
    {
        try
        {
            var javaFolder = GetJavaFolder(javaExePath);
            var libFolder = Path.Combine(javaFolder, "lib");

            if (Directory.Exists(Path.Combine(libFolder, "amd64")) ||
                Directory.Exists(Path.Combine(libFolder, "x86_64")) ||
                Directory.Exists(Path.Combine(javaFolder, "lib64")))
            {
                return true;
            }

            if (Environment.Is64BitOperatingSystem)
            {
                return true;
            }

            return false;
        }
        catch
        {
            return Environment.Is64BitOperatingSystem;
        }
    }
}