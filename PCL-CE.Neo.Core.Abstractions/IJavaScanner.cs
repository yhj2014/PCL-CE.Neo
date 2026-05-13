namespace PCL.CE.Neo.Core.Abstractions;

public interface IJavaScanner
{
    IEnumerable<string> ScanJavaPaths();
    IEnumerable<string> ScanDirectory(string directory);
    bool IsValidJavaPath(string path);
}
