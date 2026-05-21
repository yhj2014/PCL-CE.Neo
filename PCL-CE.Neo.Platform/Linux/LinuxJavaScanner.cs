using System.Runtime.InteropServices;

namespace PCL_CE.Neo.Platform.Linux;

public class LinuxJavaScanner : Core.Abstractions.IJavaScanner
{
    private static readonly string[] WindowsJavaPaths = new[]
    {
        @"C:\Program Files\Java",
        @"C:\Program Files (x86)\Java",
        @"C:\Program Files\Eclipse Adoptium",
        @"C:\Program Files\Amazon Corretto",
    };

    private static readonly string[] UnixJavaPaths = new[]
    {
        "/usr/lib/jvm",
        "/usr/java",
        "/Library/Java/JavaVirtualMachines",
        "/opt/java",
        "/opt/jdk",
    };

    public IEnumerable<string> ScanJavaPaths()
    {
        var javaPaths = new List<string>();
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        var paths = isWindows ? WindowsJavaPaths : UnixJavaPaths;
        foreach (var basePath in paths)
        {
            if (Directory.Exists(basePath))
            {
                javaPaths.AddRange(ScanDirectory(basePath));
            }
        }

        var jdkPath = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(jdkPath) && Directory.Exists(jdkPath))
        {
            javaPaths.Add(jdkPath);
        }

        var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrEmpty(userHome))
        {
            var userJdks = Path.Combine(userHome, ".jdks");
            if (Directory.Exists(userJdks))
            {
                javaPaths.AddRange(ScanDirectory(userJdks));
            }

            var sdkMan = Path.Combine(userHome, ".sdkman", "candidates", "java");
            if (Directory.Exists(sdkMan))
            {
                javaPaths.AddRange(ScanDirectory(sdkMan));
            }
        }

        return javaPaths.Where(IsValidJavaPath).Distinct();
    }

    public IEnumerable<string> ScanDirectory(string directory)
    {
        var paths = new List<string>();

        try
        {
            if (!Directory.Exists(directory))
                return paths;

            var javaExeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "java.exe" : "java";

            foreach (var dir in Directory.GetDirectories(directory))
            {
                var javaExe = Path.Combine(dir, "bin", javaExeName);
                if (File.Exists(javaExe))
                {
                    paths.Add(dir);
                }
            }
        }
        catch
        {
        }

        return paths;
    }

    public bool IsValidJavaPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        try
        {
            var javaExeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "java.exe" : "java";
            var javaExe = Path.Combine(path, "bin", javaExeName);
            if (!File.Exists(javaExe))
                return false;

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = javaExe,
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
