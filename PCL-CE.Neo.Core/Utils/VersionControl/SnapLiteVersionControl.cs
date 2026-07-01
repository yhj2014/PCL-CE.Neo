using System.Text.Json;
using PCL_CE.Neo.Core.Utils.FileSystem;
using PCL_CE.Neo.Core.Utils.Hash;

namespace PCL_CE.Neo.Core.Utils.VersionControl;

public class SnapLiteVersionControl : IVersionControl
{
    private readonly string _baseDirectory;
    private readonly string _snapshotDirectory;
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { WriteIndented = true };

    public SnapLiteVersionControl(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
        _snapshotDirectory = Path.Combine(baseDirectory, ".snapshots");
        FileUtils.EnsureDirectoryExists(_snapshotDirectory);
    }

    public void SaveSnapshot(string snapshotName)
    {
        var files = ScanDirectory(_baseDirectory);
        var versionData = new VersionData
        {
            SnapshotName = snapshotName,
            CreatedAt = DateTime.Now,
            Files = files
        };

        string snapshotPath = GetSnapshotPath(snapshotName);
        string json = JsonSerializer.Serialize(versionData, _jsonOptions);
        FileUtils.WriteAllText(snapshotPath, json);
    }

    public void LoadSnapshot(string snapshotName)
    {
        string snapshotPath = GetSnapshotPath(snapshotName);
        if (!FileUtils.Exists(snapshotPath))
            throw new FileNotFoundException($"快照 {snapshotName} 不存在。");

        string json = FileUtils.ReadAllText(snapshotPath);
        var versionData = JsonSerializer.Deserialize<VersionData>(json, _jsonOptions);
        if (versionData == null)
            throw new InvalidOperationException("快照数据无效。");

        foreach (var file in versionData.Files)
        {
            string filePath = Path.Combine(_baseDirectory, file.Path);
            if (file.Type == ObjectType.Directory)
            {
                FileUtils.EnsureDirectoryExists(filePath);
            }
            else if (file.Type == ObjectType.File && file.Content != null)
            {
                FileUtils.EnsureParentDirectoryExists(filePath);
                FileUtils.WriteAllText(filePath, file.Content);
            }
        }
    }

    public void DeleteSnapshot(string snapshotName)
    {
        string snapshotPath = GetSnapshotPath(snapshotName);
        if (FileUtils.Exists(snapshotPath))
            FileUtils.Delete(snapshotPath);
    }

    public IEnumerable<string> ListSnapshots()
    {
        if (!Directory.Exists(_snapshotDirectory))
            return Enumerable.Empty<string>();

        return Directory.EnumerateFiles(_snapshotDirectory, "*.json")
            .Select(f => Path.GetFileNameWithoutExtension(f));
    }

    public bool SnapshotExists(string snapshotName)
    {
        return FileUtils.Exists(GetSnapshotPath(snapshotName));
    }

    public void RestoreToSnapshot(string snapshotName)
    {
        LoadSnapshot(snapshotName);
    }

    public void CompareSnapshots(string snapshotName1, string snapshotName2)
    {
        var snapshot1 = LoadSnapshotData(snapshotName1);
        var snapshot2 = LoadSnapshotData(snapshotName2);

        var files1 = snapshot1.Files.ToDictionary(f => f.Path);
        var files2 = snapshot2.Files.ToDictionary(f => f.Path);

        foreach (var path in files1.Keys.Union(files2.Keys))
        {
            bool existsIn1 = files1.ContainsKey(path);
            bool existsIn2 = files2.ContainsKey(path);

            if (!existsIn1)
                Console.WriteLine($"新增: {path}");
            else if (!existsIn2)
                Console.WriteLine($"删除: {path}");
            else if (files1[path].Hash != files2[path].Hash)
                Console.WriteLine($"修改: {path}");
        }
    }

    private List<FileVersionObjects> ScanDirectory(string directory)
    {
        var files = new List<FileVersionObjects>();

        foreach (var file in Directory.EnumerateFiles(directory))
        {
            string relativePath = Path.GetRelativePath(_baseDirectory, file);
            if (relativePath.StartsWith(".snapshots"))
                continue;

            try
            {
                var fileInfo = new FileInfo(file);
                string content = File.ReadAllText(file);
                string hash = Convert.ToHexString(SHA256Provider.Instance.ComputeHash(content));

                files.Add(new FileVersionObjects
                {
                    Path = relativePath,
                    Type = ObjectType.File,
                    Hash = hash,
                    Size = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTime,
                    Created = fileInfo.CreationTime,
                    Content = content
                });
            }
            catch
            {
            }
        }

        foreach (var subDir in Directory.EnumerateDirectories(directory))
        {
            string relativePath = Path.GetRelativePath(_baseDirectory, subDir);
            if (relativePath.StartsWith(".snapshots"))
                continue;

            var subFiles = ScanDirectory(subDir);
            files.Add(new FileVersionObjects
            {
                Path = relativePath,
                Type = ObjectType.Directory,
                Children = subFiles.Select(f => f.Path).ToList()
            });
            files.AddRange(subFiles);
        }

        return files;
    }

    private string GetSnapshotPath(string snapshotName)
    {
        string sanitizedName = FileUtils.SanitizeFileName(snapshotName);
        return Path.Combine(_snapshotDirectory, $"{sanitizedName}.json");
    }

    private VersionData LoadSnapshotData(string snapshotName)
    {
        string snapshotPath = GetSnapshotPath(snapshotName);
        if (!FileUtils.Exists(snapshotPath))
            throw new FileNotFoundException($"快照 {snapshotName} 不存在。");

        string json = FileUtils.ReadAllText(snapshotPath);
        return JsonSerializer.Deserialize<VersionData>(json, _jsonOptions)
            ?? throw new InvalidOperationException("快照数据无效。");
    }
}