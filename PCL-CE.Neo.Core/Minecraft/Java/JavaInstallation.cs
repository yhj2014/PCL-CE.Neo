using System;
using System.IO;

namespace PCL_CE.Neo.Core.Minecraft.Java;

public enum JavaBrandType
{
    Unknown,
    Oracle,
    AdoptOpenJDK,
    EclipseTemurin,
    AmazonCorretto,
    Microsoft,
    AzulZulu,
    OpenJ9,
    IBM,
    Apple
}

public enum MachineType
{
    Unknown,
    X86,
    X64,
    ARM,
    ARM64
}

public sealed record JavaInstallation(
    string JavaFolder,
    Version Version,
    JavaBrandType Brand,
    MachineType Architecture,
    bool Is64Bit,
    bool IsJre)
{
    public string JavaExePath => Path.Combine(JavaFolder, 
        Environment.OSVersion.Platform == PlatformID.Win32NT ? "java.exe" : "java");

    public string? JavawExePath
    {
        get
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return null;
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