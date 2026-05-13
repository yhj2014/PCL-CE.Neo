using PCL.CE.Neo.Core.Abstractions;

namespace PCL.CE.Neo.Platform.macOS;

public class MacOSJavaScanner : IJavaScanner
{
    private static readonly string[] CommonJavaPaths =
    [
        "/Library/Java/JavaVirtualMachines",
        "/Library/Java/JavaVirtualMachines/jdk",
        "/Library/Java/JavaVirtualMachines/openjdk",
        "/System/Library/Java/JavaVirtualMachines",
        "/usr/libexec/java_home"
    ];

    public IEnumerable<string> ScanJavaPaths()
    {
        var results = new List<string>();

        foreach (var path in CommonJavaPaths)
        {
            results.AddRange(ScanDirectory(path));
        }

        TryScanJavaHome(results);

        return results.Distinct();
    }

    public IEnumerable<string> ScanDirectory(string directory)
    {
        var results = new List<string>();
        if (!Directory.Exists(directory)) return results;

        try
        {
            foreach (var dir in Directory.GetDirectories(directory))
            {
                var javaExePath = Path.Combine(dir, "Contents", "Home", "bin", "java");
                if (File.Exists(javaExePath))
                {
                    results.Add(javaExePath);
                }

                var javaHomeBinPath = Path.Combine(dir, "bin", "java");
                if (File.Exists(javaHomeBinPath))
                {
                    results.Add(javaHomeBinPath);
                }
            }
        }
        catch
        {
        }

        return results;
    }

    public bool IsValidJavaPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        if (!File.Exists(path)) return false;
        return Path.GetFileName(path).Equals("java", StringComparison.OrdinalIgnoreCase) ||
               Path.GetFileName(path).Equals("java.exe", StringComparison.OrdinalIgnoreCase);
    }

    private void TryScanJavaHome(List<string> results)
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "/usr/libexec/java_home",
                    Arguments = "-v 1.8+",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            var javaHome = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(javaHome) && Directory.Exists(javaHome))
            {
                var javaPath = Path.Combine(javaHome, "bin", "java");
                if (File.Exists(javaPath))
                {
                    results.Add(javaPath);
                }
            }
        }
        catch
        {
        }
    }
}
