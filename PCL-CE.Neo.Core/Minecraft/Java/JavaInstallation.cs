using System;
using System.IO;

namespace PCL_CE.Neo.Core.Minecraft.Java;

public sealed record JavaInstallation(
    string JavaFolder,
    Version Version,
    JavaBrandType Brand,
    bool Is64Bit,
    bool IsJre)
{
    public string JavaExePath => Path.Combine(JavaFolder, "java" + (OperatingSystem.IsWindows() ? ".exe" : ""));
    public string? JavawExePath
    {
        get
        {
            if (!OperatingSystem.IsWindows()) return null;
            var javaw = Path.Combine(JavaFolder, "javaw.exe");
            return File.Exists(javaw) ? javaw : null;
        }
    }

    public int MajorVersion => Version.Major == 1 ? Version.Minor : Version.Major;

    public bool IsStillAvailable => File.Exists(JavaExePath);

    public override string ToString() =>
        $"{(IsJre ? "JRE" : "JDK")} {MajorVersion} {Brand} {(Is64Bit ? "64 Bit" : "32 Bit")} | {JavaFolder}";

    public string ToDetailedString() =>
        $"{(IsJre ? "JRE" : "JDK")} {Version} {Brand} {(Is64Bit ? "64 Bit" : "32 Bit")} | {JavaFolder}";
}