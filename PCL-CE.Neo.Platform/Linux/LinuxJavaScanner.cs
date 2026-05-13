using PCL.CE.Neo.Core.Abstractions;

namespace PCL.CE.Neo.Platform.Linux;

public class LinuxJavaScanner : IJavaScanner
{
    private static readonly string[] CommonJavaPaths =
    [
        "/usr/lib/jvm",
        "/usr/java",
        "/opt/java",
        "/opt/jdk",
        "/usr/local/java",
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".sdkman", "candidates", "java")
    ];

    public IEnumerable<string> ScanJavaPaths()
    {
        var results = new List<string>();

        foreach (var path in CommonJavaPaths)
        {
            results.AddRange(ScanDirectory(path));
        }

        TryScanWhich(results);

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
                var javaExePath = Path.Combine(dir, "bin", "java");
                if (File.Exists(javaExePath))
                {
                    results.Add(javaExePath);
                }

                var javaHomeBinPath = Path.Combine(dir, "jre", "bin", "java");
                if (File.Exists(javaHomeBinPath))
                {
                    results.Add(javaHomeBinPath);
                }
            }

            var javaPath = Path.Combine(directory, "bin", "java");
            if (File.Exists(javaPath))
            {
                results.Add(javaPath);
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

    private void TryScanWhich(List<string> results)
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = "-a java",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmedPath = line.Trim();
                if (File.Exists(trimmedPath))
                {
                    results.Add(trimmedPath);
                }
            }
        }
        catch
        {
        }
    }
}
