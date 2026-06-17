using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Minecraft.Java.Parser;

public class JavaParser : IJavaParser
{
    private static readonly Regex VersionRegex = new(@"version ""([\d.]+)""", RegexOptions.Compiled);
    private static readonly Regex BitsRegex = new(@"64-Bit|amd64|x86_64", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex JreRegex = new(@"Runtime Environment", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public JavaInstallation? Parse(string javaExePath)
    {
        if (!File.Exists(javaExePath))
            return null;

        var javaFolder = Path.GetDirectoryName(javaExePath) ?? string.Empty;
        if (string.IsNullOrEmpty(javaFolder))
            return null;

        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = javaExePath,
                Arguments = "-version",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
                return null;

            process.WaitForExit(5000);
            var output = process.StandardError.ReadToEnd() + process.StandardOutput.ReadToEnd();

            var versionMatch = VersionRegex.Match(output);
            if (!versionMatch.Success)
                return null;

            if (!Version.TryParse(versionMatch.Groups[1].Value, out var version))
                version = new Version(1, 8);

            var is64Bit = BitsRegex.IsMatch(output);
            var isJre = JreRegex.IsMatch(output);
            var brand = _DetectBrand(javaFolder, output);

            return new JavaInstallation(javaFolder, version, brand, is64Bit, isJre);
        }
        catch
        {
            return null;
        }
    }

    private static JavaBrandType _DetectBrand(string folder, string output)
    {
        var folderLower = folder.ToLower();
        var outputLower = output.ToLower();

        if (folderLower.Contains("temurin") || outputLower.Contains("temurin"))
            return JavaBrandType.EclipseTemurin;
        if (folderLower.Contains("liberica") || outputLower.Contains("liberica"))
            return JavaBrandType.Liberica;
        if (folderLower.Contains("zulu") || outputLower.Contains("zulu"))
            return JavaBrandType.Zulu;
        if (folderLower.Contains("corretto") || outputLower.Contains("corretto"))
            return JavaBrandType.Corretto;
        if (folderLower.Contains("microsoft"))
            return JavaBrandType.Microsoft;
        if (folderLower.Contains("semeru") || outputLower.Contains("semeru"))
            return JavaBrandType.IBMSemeru;
        if (folderLower.Contains("oracle") || outputLower.Contains("oracle"))
            return JavaBrandType.Oracle;
        if (folderLower.Contains("dragonwell"))
            return JavaBrandType.Dragonwell;
        if (folderLower.Contains("kona"))
            return JavaBrandType.TencentKona;
        if (folderLower.Contains("graalvm"))
            return JavaBrandType.GraalVmCommunity;
        if (folderLower.Contains("jetbrains"))
            return JavaBrandType.JetBrains;
        if (outputLower.Contains("openjdk"))
            return JavaBrandType.OpenJDK;

        return JavaBrandType.Unknown;
    }
}