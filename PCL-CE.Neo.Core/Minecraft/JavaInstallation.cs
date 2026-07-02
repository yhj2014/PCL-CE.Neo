using System;
using System.IO;
using PCL_CE.Neo.Core.Utils;

namespace PCL_CE.Neo.Core.Minecraft;

/// <summary>
/// Java 安装信息，包含详细的版本、架构和品牌信息
/// </summary>
public sealed record JavaInstallation(
    string JavaFolder,
    Version Version,
    JavaBrandType Brand,
    MachineType Architecture,
    bool Is64Bit,
    bool IsJre)
{
    /// <summary>
    /// java.exe 可执行文件完整路径
    /// </summary>
    public string JavaExePath => Path.Combine(JavaFolder, "bin", "java.exe");
    
    /// <summary>
    /// javaw.exe 可执行文件完整路径（如果存在）
    /// </summary>
    public string? JavawExePath
    {
        get
        {
            var javaw = Path.Combine(JavaFolder, "bin", "javaw.exe");
            return File.Exists(javaw) ? javaw : null;
        }
    }

    /// <summary>
    /// Java 主版本号（自动处理 1.8.0 → 8 的映射）
    /// </summary>
    public int MajorVersion => Version.Major == 1 ? Version.Minor : Version.Major;

    /// <summary>
    /// 检查物理文件是否仍然存在（实时检查，非缓存状态）
    /// </summary>
    public bool IsStillAvailable => File.Exists(JavaExePath);

    /// <summary>
    /// 简洁的显示字符串
    /// </summary>
    public override string ToString() =>
        $"{(IsJre ? "JRE" : "JDK")} {MajorVersion} {Brand} {(Is64Bit ? "64-bit" : "32-bit")} | {JavaFolder}";

    /// <summary>
    /// 详细的显示字符串
    /// </summary>
    public string ToDetailedString() =>
        $"{(IsJre ? "JRE" : "JDK")} {Version} {Brand} {(Is64Bit ? "64-bit" : "32-bit")} | {JavaFolder}";
}