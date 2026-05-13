namespace PCL.CE.Neo.Core.Abstractions;

public interface IPlatformService
{
    string PlatformName { get; }
    string OSVersion { get; }
    string Architecture { get; }

    void OpenUrl(string url);
    void OpenFolder(string path);
    string GetLocalApplicationDataPath();
    string GetTempPath();
    string GetGameDataPath();
}
