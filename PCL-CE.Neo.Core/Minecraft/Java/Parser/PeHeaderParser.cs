using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PCL_CE.Neo.Core.Minecraft.Java.Parser;

public class PeHeaderParser : IJavaParser
{
    private static readonly Dictionary<string, JavaBrandType> _BrandMap = new()
    {
        ["Eclipse"] = JavaBrandType.EclipseTemurin,
        ["Temurin"] = JavaBrandType.EclipseTemurin,
        ["Bellsoft"] = JavaBrandType.Liberica,
        ["Microsoft"] = JavaBrandType.Microsoft,
        ["Amazon"] = JavaBrandType.Corretto,
        ["Azul"] = JavaBrandType.Zulu,
        ["IBM"] = JavaBrandType.IBMSemeru,
        ["Oracle"] = JavaBrandType.Oracle,
        ["Tencent"] = JavaBrandType.TencentKona,
        ["OpenJDK"] = JavaBrandType.OpenJDK,
        ["Alibaba"] = JavaBrandType.Dragonwell,
        ["GraalVM"] = JavaBrandType.GraalVmCommunity,
        ["JetBrains"] = JavaBrandType.JetBrains
    };

    private readonly ILogger<PeHeaderParser> _logger;

    public PeHeaderParser(ILogger<PeHeaderParser> logger)
    {
        _logger = logger;
    }

    public JavaInstallation? Parse(string javaExePath)
    {
        try
        {
            if (!File.Exists(javaExePath))
                return null;

            _logger.LogInformation("解析 {JavaExePath} 的 Java 程序信息", javaExePath);

            var versionInfo = FileVersionInfo.GetVersionInfo(javaExePath);
            var fileVersion = Version.Parse(versionInfo.FileVersion ?? "0.0.0.0");
            var companyName = _NormalizeCompanyName(versionInfo);
            var brand = _DetermineBrand(companyName);

            var javaFolder = Path.GetDirectoryName(javaExePath)!;
            var isJre = !File.Exists(Path.Combine(javaFolder, "javac.exe"));

            var peData = PEHeaderReader.ReadPEHeader(javaExePath);
            var arch = peData.Machine;
            var is64Bit = PEHeaderReader.IsMachine64Bit(arch);

            return new JavaInstallation(
                javaFolder,
                fileVersion,
                brand,
                arch,
                is64Bit,
                isJre
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解析 {JavaExePath} 时出错", javaExePath);
            return null;
        }
    }

    private static string _NormalizeCompanyName(FileVersionInfo info)
    {
        var name = info.CompanyName ?? info.FileDescription ?? info.ProductName ?? string.Empty;

        if (name.Contains("Oracle", System.StringComparison.OrdinalIgnoreCase) || name == "N/A")
        {
            if ((info.FileDescription?.Contains("Java(TM)", System.StringComparison.OrdinalIgnoreCase) ?? false) ||
                (info.ProductName?.Contains("Java(TM)", System.StringComparison.OrdinalIgnoreCase) ?? false))
                return "Oracle";
            return "OpenJDK";
        }
        return name;
    }

    private static JavaBrandType _DetermineBrand(string output)
    {
        var match = _BrandMap.Keys
            .FirstOrDefault(k => output.Contains(k, System.StringComparison.OrdinalIgnoreCase));
        return match != null ? _BrandMap[match] : JavaBrandType.Unknown;
    }
}