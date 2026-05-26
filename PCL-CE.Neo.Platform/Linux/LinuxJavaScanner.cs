namespace PCL_CE.Neo.Platform.Linux;

public class LinuxJavaScanner : Core.Abstractions.IJavaScanner
{
    private static readonly string[] LinuxJavaPaths = new[]
    {
        "/usr/lib/jvm",
        "/usr/java",
        "/opt/java",
        "/opt/jdk",
    };

    public IEnumerable<string> ScanJavaPaths()
    {
        var javaPaths = new List<string>();

        foreach (var basePath in LinuxJavaPaths)
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

            foreach (var dir in Directory.GetDirectories(directory))
            {
                var javaExe = Path.Combine(dir, "bin", "java");
                if (File.Exists(javaExe))
                {
                    paths.Add(dir);
                }
            }
        }
        catch { }

        return paths;
    }

    public bool IsValidJavaPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        try
        {
            var javaExe = Path.Combine(path, "bin", "java");
            return File.Exists(javaExe);
        }
        catch
        {
            return false;
        }
    }
}
