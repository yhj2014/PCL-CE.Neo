namespace PCL_CE.Neo.Core.Utils.VersionControl;

public interface IVersionControl
{
    void SaveSnapshot(string snapshotName);
    void LoadSnapshot(string snapshotName);
    void DeleteSnapshot(string snapshotName);
    IEnumerable<string> ListSnapshots();
    bool SnapshotExists(string snapshotName);
    void RestoreToSnapshot(string snapshotName);
    void CompareSnapshots(string snapshotName1, string snapshotName2);
}