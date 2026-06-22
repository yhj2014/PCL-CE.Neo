namespace PCL_CE.Neo.Core.Abstractions;

public interface IJavaScanner
{
    IEnumerable<string> ScanJavaPaths();
    bool IsValidJavaPath(string path);
    string? GetJavaVersion(string path);
    int GetJavaBits(string path);
    JavaBrandType GetJavaBrand(string path);
}

public enum JavaBrandType
{
    Unknown,
    Oracle,
    AdoptOpenJDK,
    EclipseTemurin,
    AmazonCorretto,
    Microsoft,
    AzulZulu,
    OpenJ9
}