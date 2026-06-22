namespace PCL_CE.Neo.Core.Abstractions.Mock;

public class JavaScannerMock : IJavaScanner
{
    public IEnumerable<string> ScanJavaPaths()
    {
        // Return empty in mock mode
        return Enumerable.Empty<string>();
    }

    public bool IsValidJavaPath(string path)
    {
        // Mock always returns false
        return false;
    }

    public string? GetJavaVersion(string path)
    {
        // Mock always returns null
        return null;
    }

    public int GetJavaBits(string path)
    {
        // Mock always returns 64
        return 64;
    }

    public JavaBrandType GetJavaBrand(string path)
    {
        // Mock always returns Unknown
        return JavaBrandType.Unknown;
    }
}