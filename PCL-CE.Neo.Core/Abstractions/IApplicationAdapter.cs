namespace PCL_CE.Neo.Core.Abstractions;

public interface IApplicationAdapter
{
    string VersionName { get; }
    int VersionCode { get; }
    string VersionBranch { get; }
    bool IsAprilFool { get; }

    int ProcessId { get; }
    string ExecutablePath { get; }
    string ExecutableDirectory { get; }
    string ExecutableName { get; }
    string[] CommandLineArguments { get; }
    string CurrentDirectory { get; }

    Thread RunInNewThread(Action action, string? name = null, ThreadPriority priority = ThreadPriority.Normal);
    void OpenPath(string path, string? workingDirectory = null);
    Stream? GetResourceStream(string path);
    string GetAppImagePath(string imageName);
}
