using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.VersionControl;

public interface IVersionControl
{
    List<VersionData> GetVersions();
    VersionData? GetVersion(string nodeId);
    List<FileVersionObjects>? GetNodeObjects(string nodeId);
    Task<string> CreateNewVersion(string? name = null, string? desc = null);
    Task ApplyPastVersion(string nodeId);
    void DeleteVersion(string nodeId);
    Task<bool> CheckVersion(string nodeId, bool deepCheck = false);
    Task CleanUnrecordObjects();
    Stream? GetObjectContent(string objectId);
    Task Export(string nodeId, string saveFilePath);
}