namespace PCL_CE.Neo.Core.Abstractions.Mock;

public class JavaScannerMock : IJavaScanner
{
    public List<string> FoundJavaPaths { get; set; } = new List<string>();
    public Func<string, bool>? OnIsValidJavaPath { get; set; }
    
    public IEnumerable<string> ScanJavaPaths()
    {
        return FoundJavaPaths;
    }

    public IEnumerable<string> ScanDirectory(string directory)
    {
        var javaPath = Path.Combine(directory, "bin", "java.exe");
        if (File.Exists(javaPath))
        {
            return new[] { javaPath };
        }
        return Enumerable.Empty<string>();
    }

    public bool IsValidJavaPath(string path)
    {
        if (OnIsValidJavaPath != null)
        {
            return OnIsValidJavaPath(path);
        }
        return !string.IsNullOrEmpty(path) && File.Exists(path);
    }
}
