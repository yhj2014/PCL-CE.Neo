using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Minecraft.Java.Scanner;

public class JavaParser : IJavaParser
{
    private static readonly Regex VersionRegex = new(@"version ""([^""]+)""", RegexOptions.Compiled);
    private static readonly Regex JreRegex = new(@"Runtime Environment|JRE", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex JdkRegex = new(@"Development Kit|JDK", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex X64Regex = new(@"64\-Bit|amd64|x86_64", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex X86Regex = new(@"32\-Bit|i386|x86", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ArmRegex = new(@"arm|aarch64", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public JavaInstallation? Parse(string javaExePath)
    {
        try
        {
            if (!File.Exists(javaExePath)) return null;

            var javaFolder = Directory.GetParent(javaExePath)?.FullName;
            if (string.IsNullOrEmpty(javaFolder)) return null;

            var result = _RunJavaVersion(javaExePath);
            if (string.IsNullOrEmpty(result)) return null;

            var version = _ParseVersion(result);
            var isJre = _ParseIsJre(result);
            var is64Bit = _ParseIs64Bit(result);
            var brand = _ParseBrand(result);
            var architecture = _ParseArchitecture(result);

            return new JavaInstallation(
                JavaFolder: javaFolder,
                Version: version,
                Brand: brand,
                Architecture: architecture,
                Is64Bit: is64Bit,
                IsJre: isJre
            );
        }
        catch
        {
            return null;
        }
    }

    private string? _RunJavaVersion(string javaExePath)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = javaExePath,
                Arguments = "-version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetTempPath()
            };

            using var process = Process.Start(processStartInfo);
            if (process == null) return null;

            process.WaitForExit(5000);
            return process.StandardError.ReadToEnd() + process.StandardOutput.ReadToEnd();
        }
        catch
        {
            return null;
        }
    }

    private Version _ParseVersion(string output)
    {
        try
        {
            var match = VersionRegex.Match(output);
            if (match.Success)
            {
                var versionStr = match.Groups[1].Value;
                if (Version.TryParse(versionStr, out var version))
                {
                    return version;
                }
            }
        }
        catch
        {
        }

        return new Version(1, 8, 0);
    }

    private bool _ParseIsJre(string output)
    {
        return JreRegex.IsMatch(output) && !JdkRegex.IsMatch(output);
    }

    private bool _ParseIs64Bit(string output)
    {
        return X64Regex.IsMatch(output);
    }

    private MachineType _ParseArchitecture(string output)
    {
        if (X64Regex.IsMatch(output)) return MachineType.X64;
        if (X86Regex.IsMatch(output)) return MachineType.X86;
        if (ArmRegex.IsMatch(output)) return MachineType.ARM64;
        return MachineType.Unknown;
    }

    private JavaBrandType _ParseBrand(string output)
    {
        if (output.Contains("Microsoft", StringComparison.OrdinalIgnoreCase)) return JavaBrandType.Microsoft;
        if (output.Contains("Eclipse Temurin", StringComparison.OrdinalIgnoreCase) || 
            output.Contains("Temurin", StringComparison.OrdinalIgnoreCase)) return JavaBrandType.EclipseTemurin;
        if (output.Contains("AdoptOpenJDK", StringComparison.OrdinalIgnoreCase)) return JavaBrandType.AdoptOpenJDK;
        if (output.Contains("Amazon Corretto", StringComparison.OrdinalIgnoreCase) ||
            output.Contains("Corretto", StringComparison.OrdinalIgnoreCase)) return JavaBrandType.AmazonCorretto;
        if (output.Contains("Azul", StringComparison.OrdinalIgnoreCase) || 
            output.Contains("Zulu", StringComparison.OrdinalIgnoreCase)) return JavaBrandType.AzulZulu;
        if (output.Contains("OpenJ9", StringComparison.OrdinalIgnoreCase)) return JavaBrandType.OpenJ9;
        if (output.Contains("Oracle", StringComparison.OrdinalIgnoreCase)) return JavaBrandType.Oracle;
        if (output.Contains("IBM", StringComparison.OrdinalIgnoreCase)) return JavaBrandType.IBM;
        if (output.Contains("Apple", StringComparison.OrdinalIgnoreCase)) return JavaBrandType.Apple;
        return JavaBrandType.Unknown;
    }
}