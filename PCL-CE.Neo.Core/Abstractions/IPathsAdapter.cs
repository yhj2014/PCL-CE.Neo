namespace PCL_CE.Neo.Core.Abstractions;

public interface IPathsAdapter
{
    string Data { get; }
    string SharedData { get; }
    string SharedLocalData { get; }
    string Temp { get; }
    string OldSharedData { get; }

    string GetSpecialPath(Environment.SpecialFolder folder, string relative);
    void EnsureDirectories();
}
