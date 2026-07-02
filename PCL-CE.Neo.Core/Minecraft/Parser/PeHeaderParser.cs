using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Utils;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.Minecraft.Parser;

/// <summary>
/// 基于 PE 文件头的 Java 解析器，使用 Windows PE 格式解析精确识别 Java 版本和架构
/// </summary>
public class PeHeaderParser : IJavaParser
{
    private readonly ILogger<PeHeaderParser>? _logger;
    
    /// <summary>
    /// 品牌识别关键词映射表
    /// </summary>
    private static readonly Dictionary<string, JavaBrandType> BrandMap = new()
    {
        ["Eclipse"] = JavaBrandType.EclipseTemurin,
        ["Temurin"] = JavaBrandType.EclipseTemurin,
        ["AdoptOpenJDK"] = JavaBrandType.EclipseTemurin,
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

    public PeHeaderParser() : this(null) { }

    public PeHeaderParser(ILogger<PeHeaderParser>? logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 解析 Java 可执行文件的详细信息
    /// </summary>
    /// <param name="javaExePath">java.exe 或 javaw.exe 的完整路径</param>
    /// <returns>解析结果</returns>
    public JavaInstallation? Parse(string javaExePath)
    {
        try
        {
            if (string.IsNullOrEmpty(javaExePath) || !File.Exists(javaExePath))
            {
                _logger?.LogWarning("Java 文件不存在: {Path}", javaExePath);
                return null;
            }

            _logger?.LogDebug("开始解析 Java 文件: {Path}", javaExePath);

            // 读取文件版本信息
            var versionInfo = FileVersionInfo.GetVersionInfo(javaExePath);
            var fileVersion = ParseVersionSafe(versionInfo.FileVersion ?? "0.0.0.0");
            var companyName = NormalizeCompanyName(versionInfo);
            var brand = DetermineBrand(companyName);

            // 确定 Java 文件夹路径
            var javaFolder = Path.GetDirectoryName(javaExePath)!;
            if (Path.GetFileName(javaFolder) == "bin")
            {
                javaFolder = Directory.GetParent(javaFolder)?.FullName ?? javaFolder;
            }

            // 检查是 JRE 还是 JDK
            var isJre = !File.Exists(Path.Combine(javaFolder, "bin", "javac.exe"));

            // 读取 PE 头获取架构信息
            var peData = PEHeaderReader.ReadPEHeader(javaExePath);
            var arch = peData.Machine;
            var is64Bit = PEHeaderReader.IsMachine64Bit(arch);

            // 可用性检查（不影响模型创建，由调用方决定是否启用）
            var libDir = Path.Combine(javaFolder, "lib");
            var isUsable = (!isJre && File.Exists(Path.Combine(libDir, "jvm.lib"))) ||
                           (isJre && File.Exists(Path.Combine(libDir, "rt.jar")));

            if (!isUsable)
            {
                _logger?.LogDebug("Java 安装可能不完整: {Folder}", javaFolder);
            }

            _logger?.LogInformation("解析成功: {Brand} {Version} {Arch}", 
                brand, fileVersion, is64Bit ? "64-bit" : "32-bit");

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
            _logger?.LogError(ex, "解析 Java 文件失败: {Path}", javaExePath);
            return null;
        }
    }

    /// <summary>
    /// 安全解析版本字符串
    /// </summary>
    private static Version ParseVersionSafe(string versionString)
    {
        try
        {
            // 移除可能的前缀和后缀
            var cleanVersion = versionString.Trim();
            
            // 处理 "1.8.0_xxx" 格式
            if (cleanVersion.StartsWith("1.8.0_"))
            {
                var updateNumber = cleanVersion.Substring(6);
                if (int.TryParse(updateNumber, out var update))
                {
                    return new Version(1, 8, 0, update);
                }
            }

            // 处理标准版本格式
            var parts = cleanVersion.Split('.', '_');
            var numbers = new List<int>();
            
            foreach (var part in parts.Take(4))
            {
                if (int.TryParse(part, out var num))
                {
                    numbers.Add(num);
                }
            }

            while (numbers.Count < 4)
            {
                numbers.Add(0);
            }

            return new Version(numbers[0], numbers[1], numbers[2], numbers[3]);
        }
        catch
        {
            return new Version(0, 0, 0, 0);
        }
    }

    /// <summary>
    /// 规范化公司名称，解决 Oracle/OpenJDK 混淆问题
    /// </summary>
    private static string NormalizeCompanyName(FileVersionInfo info)
    {
        var name = info.CompanyName ?? info.FileDescription ?? info.ProductName ?? string.Empty;

        // 修复 Oracle/OpenJDK 混淆问题
        if (name.Contains("Oracle", StringComparison.OrdinalIgnoreCase) || name == "N/A" || name == "")
        {
            if ((info.FileDescription?.Contains("Java(TM)", StringComparison.OrdinalIgnoreCase) ?? false) ||
                (info.ProductName?.Contains("Java(TM)", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                return "Oracle";
            }
            return "OpenJDK";
        }

        return name;
    }

    /// <summary>
    /// 从公司名称确定 Java 品牌
    /// </summary>
    private static JavaBrandType DetermineBrand(string companyName)
    {
        var match = BrandMap.Keys
            .FirstOrDefault(k => companyName.Contains(k, StringComparison.OrdinalIgnoreCase));
        
        return match != null ? BrandMap[match] : JavaBrandType.Unknown;
    }
}