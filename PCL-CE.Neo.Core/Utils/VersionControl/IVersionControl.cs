namespace PCL_CE.Neo.Core.Utils.VersionControl;

public interface IVersionControl<T>
{
    bool IsNewer(T current, T target);
    bool IsOlder(T current, T target);
    bool IsEqual(T current, T target);
    int Compare(T current, T target);
    T? Latest(params T[] versions);
    T? Oldest(params T[] versions);
}

public interface IVersionData
{
    string Version { get; }
    DateTime ReleaseDate { get; }
    string? Changelog { get; }
}

public interface ISnapLiteVersionControl : IDisposable
{
    Task<string> CreateNewVersion(string? name = null, string? desc = null);
    VersionData? GetVersion(string nodeId);
    List<VersionData> GetVersions();
    List<FileVersionObjects>? GetNodeObjects(string nodeId);
    void DeleteVersion(string nodeId);
    Stream? GetObjectContent(string objectId);
    Task ApplyPastVersion(string nodeId);
    Task<bool> CheckVersion(string nodeId, bool deepCheck = false);
    Task CleanUnrecordObjects();
    Task Export(string nodeId, string saveFilePath);
}