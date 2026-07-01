using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft.Java;

public class JavaService
{
    private readonly ILogger<JavaService> _logger;

    public JavaService()
        : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<JavaService>.Instance)
    {
    }

    public JavaService(ILogger<JavaService> logger)
    {
        _logger = logger;
    }

    public async Task<string?> GetJavaVersionAsync(string javaPath)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = javaPath,
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            var output = await process.StandardError.ReadToEndAsync();
            var version = ParseVersion(output);

            _logger.LogDebug("Java version check: {JavaPath} -> {Version}", javaPath, version);
            return version;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Java version from {JavaPath}", javaPath);
            return null;
        }
    }

    public string? GetJavaVersion(string javaPath)
    {
        return GetJavaVersionAsync(javaPath).GetAwaiter().GetResult();
    }

    public async Task<JavaBrandType> DetectJavaBrandAsync(string javaPath)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = javaPath,
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            var output = await process.StandardError.ReadToEndAsync();
            var brand = DetectBrand(output);

            _logger.LogDebug("Java brand detection: {JavaPath} -> {Brand}", javaPath, brand);
            return brand;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect Java brand from {JavaPath}", javaPath);
            return JavaBrandType.Unknown;
        }
    }

    public JavaBrandType DetectJavaBrand(string javaPath)
    {
        return DetectJavaBrandAsync(javaPath).GetAwaiter().GetResult();
    }

    public bool IsJava64Bit(string javaPath)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = javaPath,
                    Arguments = "-d64 -version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            return process.ExitCode == 0;
        }
        catch
        {
            try
            {
                var fileInfo = new FileInfo(javaPath);
                if (fileInfo.Directory?.Parent?.FullName.Contains("x64") ?? false)
                    return true;

                if (fileInfo.Directory?.Parent?.FullName.Contains("amd64") ?? false)
                    return true;

                return Environment.Is64BitOperatingSystem;
            }
            catch
            {
                return Environment.Is64BitOperatingSystem;
            }
        }
    }

    public async Task<bool> IsJavaValidAsync(string javaPath)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = javaPath,
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public bool IsJavaValid(string javaPath)
    {
        return IsJavaValidAsync(javaPath).GetAwaiter().GetResult();
    }

    public async Task<string?> RunJavaCommandAsync(string javaPath, string arguments, string? workingDirectory = null)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = javaPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory ?? string.Empty
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            _logger.LogDebug("Java command executed: {JavaPath} {Arguments}, ExitCode: {ExitCode}", javaPath, arguments, process.ExitCode);

            return process.ExitCode == 0 ? output : error;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run Java command: {JavaPath} {Arguments}", javaPath, arguments);
            return null;
        }
    }

    public string? RunJavaCommand(string javaPath, string arguments, string? workingDirectory = null)
    {
        return RunJavaCommandAsync(javaPath, arguments, workingDirectory).GetAwaiter().GetResult();
    }

    private string? ParseVersion(string output)
    {
        foreach (var pattern in JavaConsts.JavaVersionPatterns)
        {
            var match = Regex.Match(output, pattern);
            if (match.Success)
            {
                var version = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(version))
                    return version;
            }
        }

        return null;
    }

    private JavaBrandType DetectBrand(string output)
    {
        foreach (var (brand, keywords) in JavaConsts.BrandKeywords)
        {
            if (keywords.Any(keyword => output.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0))
                return brand;
        }

        return JavaBrandType.Unknown;
    }
}