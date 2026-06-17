using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Utils.Exts;
using PCL_CE.Neo.Core.Utils.Hash;

namespace PCL_CE.Neo.Core.Utils.VersionControl;

public class SnapLiteVersionControl : IVersionControl, IDisposable
{
    private readonly string _rootPath;
    private readonly IHashProvider _hashProvider = SHA512Provider.Instance;
    private readonly string _objectsFolder;
    private readonly string _indexFile;
    private readonly ILogger<SnapLiteVersionControl>? _logger;

    private const string ConfigFolderName = ".litesnap";
    private const string IndexFileName = "index.json";
    private const string ObjectsFolderName = "objects";

    public SnapLiteVersionControl(string rootPath, ILogger<SnapLiteVersionControl>? logger = null)
    {
        try
        {
            _rootPath = rootPath;
            _logger = logger;
            var configFolder = Path.Combine(_rootPath, ConfigFolderName);
            _objectsFolder = Path.Combine(configFolder, ObjectsFolderName);
            _indexFile = Path.Combine(configFolder, IndexFileName);

            if (!Directory.Exists(_objectsFolder))
                Directory.CreateDirectory(_objectsFolder);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "[SnapLite] 无法加载位于 {RootPath} 处的 SnapLite 数据", _rootPath);
            throw;
        }
    }

    private async Task<FileVersionObjects[]> _GetAllTrackedObjectsAsync()
    {
        List<FileVersionObjects> scannedPaths = [];
        Queue<string> scanQueue = new();
        scanQueue.Enqueue(_rootPath);
        string[] excludePath = [Path.Combine(_rootPath, ConfigFolderName)];

        while (scanQueue.Count != 0)
        {
            var curDir = new DirectoryInfo(scanQueue.Dequeue());
            var filesInCurDir = curDir.EnumerateFiles().ToArray();
            var dirsInCurDir = curDir.EnumerateDirectories().ToArray();

            if (filesInCurDir.Length == 0 && dirsInCurDir.Length == 0)
            {
                scannedPaths.Add(new FileVersionObjects
                {
                    CreationTime = curDir.CreationTime,
                    Hash = string.Empty,
                    LastWriteTime = curDir.LastWriteTime,
                    Length = 0,
                    ObjectType = ObjectType.Directory,
                    Path = curDir.FullName.Replace(_rootPath, string.Empty).TrimStart(Path.DirectorySeparatorChar)
                });
                continue;
            }

            var fileComputesResult = await filesInCurDir.SelectAsync(async file =>
            {
                using var fs = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                return new FileVersionObjects
                {
                    CreationTime = file.CreationTime,
                    Hash = (await _hashProvider.ComputeHashAsync(fs).ConfigureAwait(false)).ToHexString(),
                    LastWriteTime = file.LastWriteTime,
                    Length = fs.Length,
                    Path = file.FullName.Replace(_rootPath, string.Empty).TrimStart(Path.DirectorySeparatorChar),
                    ObjectType = ObjectType.File
                };
            }, 15).ConfigureAwait(false);

            scannedPaths.AddRange(fileComputesResult);

            foreach (var directory in dirsInCurDir)
                if (!excludePath.Contains(directory.FullName))
                    scanQueue.Enqueue(directory.FullName);
        }

        return scannedPaths.ToArray();
    }

    private async Task<List<VersionData>> _LoadIndexAsync()
    {
        if (!File.Exists(_indexFile))
            return [];

        try
        {
            using var fs = File.OpenRead(_indexFile);
            return await JsonSerializer.DeserializeAsync<List<VersionData>>(fs) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private async Task _SaveIndexAsync(List<VersionData> index)
    {
        using var fs = File.OpenWrite(_indexFile);
        await JsonSerializer.SerializeAsync(fs, index);
    }

    private string _GetNodeFolderPath(string nodeId) => Path.Combine(Path.GetDirectoryName(_indexFile)!, $"node_{nodeId}");

    private async Task<List<FileVersionObjects>> _LoadNodeObjectsAsync(string nodeId)
    {
        var nodeFolder = _GetNodeFolderPath(nodeId);
        var objectsFile = Path.Combine(nodeFolder, "objects.json");

        if (!File.Exists(objectsFile))
            return [];

        try
        {
            using var fs = File.OpenRead(objectsFile);
            return await JsonSerializer.DeserializeAsync<List<FileVersionObjects>>(fs) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private async Task _SaveNodeObjectsAsync(string nodeId, List<FileVersionObjects> objects)
    {
        var nodeFolder = _GetNodeFolderPath(nodeId);
        if (!Directory.Exists(nodeFolder))
            Directory.CreateDirectory(nodeFolder);

        var objectsFile = Path.Combine(nodeFolder, "objects.json");
        using var fs = File.OpenWrite(objectsFile);
        await JsonSerializer.SerializeAsync(fs, objects);
    }

    public async Task<string> CreateNewVersion(string? name = null, string? desc = null)
    {
        try
        {
            var nodeId = Guid.NewGuid().ToString("N");

            var allFiles = await _GetAllTrackedObjectsAsync().ConfigureAwait(false);
            _logger?.LogInformation("[SnapLite] 已获取到全部文件，总数量为 {Count}", allFiles.Length);

            var newAddFiles = allFiles
                .Distinct(FileVersionObjectsComparer.Instance)
                .Where(x => x.ObjectType.Equals(ObjectType.Directory) ||
                            (x.ObjectType.Equals(ObjectType.File) && !_ObjectExists(x.Hash)))
                .ToList();

            _logger?.LogInformation("[SnapLite] 新增对象总数量为 {Count}", newAddFiles.Count);

            await _SaveNodeObjectsAsync(nodeId, allFiles.ToList());
            _logger?.LogInformation("[SnapLite] 记录已压入存储，开始存储文件");

            await newAddFiles
                .Where(x => x.ObjectType == ObjectType.File)
                .ForEachAsync(x => Task.Run(async () =>
                {
                    var filePath = Path.Combine(_rootPath, x.Path);
                    try
                    {
                        _logger?.LogInformation("[SnapLite] 将 {FilePath} 放入哈希仓库", filePath);
                        await _PutObjectAsync(filePath, x.Hash).ConfigureAwait(false);
                        _logger?.LogInformation("[SnapLite] 已完成 {FilePath} 的存储", filePath);
                    }
                    catch (Exception e)
                    {
                        _logger?.LogError(e, "[SnapLite] 存储 {FilePath} 文件过程中出现错误", filePath);
                        throw;
                    }
                }), 12).ConfigureAwait(false);

            _logger?.LogInformation("[SnapLite] 文件存储任务完成");

            var nodeList = await _LoadIndexAsync();
            var currentNodeInfo = new VersionData
            {
                Created = DateTime.Now,
                Desc = desc ?? "Backup made by SnapLite",
                Name = name ?? $"{DateTime.Now:yyyy/dd/MM-HH:mm:ss}",
                NodeId = nodeId,
                Version = nodeList.Count + 1
            };

            nodeList.Add(currentNodeInfo);
            await _SaveIndexAsync(nodeList);
            _logger?.LogInformation("[SnapLite] 索引记录更新完成");

            return nodeId;
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "[SnapLite] 创建快照出错");
            throw;
        }
    }

    private bool _ObjectExists(string hash)
    {
        var objectPath = Path.Combine(_objectsFolder, hash);
        return File.Exists(objectPath);
    }

    private async Task _PutObjectAsync(string filePath, string hash)
    {
        var objectPath = Path.Combine(_objectsFolder, hash);
        await File.CopyAsync(filePath, objectPath, true);
    }

    private async Task _DeleteObjectAsync(string hash)
    {
        var objectPath = Path.Combine(_objectsFolder, hash);
        if (File.Exists(objectPath))
            File.Delete(objectPath);
    }

    public VersionData? GetVersion(string nodeId)
    {
        var nodeList = _LoadIndexAsync().GetAwaiter().GetResult();
        return nodeList.FirstOrDefault(x => x.NodeId == nodeId);
    }

    public List<VersionData> GetVersions()
    {
        return _LoadIndexAsync().GetAwaiter().GetResult();
    }

    public List<FileVersionObjects>? GetNodeObjects(string nodeId)
    {
        return _LoadNodeObjectsAsync(nodeId).GetAwaiter().GetResult();
    }

    public void DeleteVersion(string nodeId)
    {
        try
        {
            var nodeList = _LoadIndexAsync().GetAwaiter().GetResult();
            nodeList.RemoveAll(x => x.NodeId == nodeId);
            _SaveIndexAsync(nodeList).GetAwaiter().GetResult();

            var nodeFolder = _GetNodeFolderPath(nodeId);
            if (Directory.Exists(nodeFolder))
                Directory.Delete(nodeFolder, true);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "[SnapLite] 删除 {NodeId} 时出现错误", nodeId);
            throw;
        }
    }

    public Stream? GetObjectContent(string objectId)
    {
        try
        {
            var objectPath = Path.Combine(_objectsFolder, objectId);
            if (!File.Exists(objectPath))
                return null;

            return File.OpenRead(objectPath);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "[SnapLite] 获取 {ObjectId} 的流失败", objectId);
            throw;
        }
    }

    public async Task ApplyPastVersion(string nodeId)
    {
        _logger?.LogInformation("[SnapLite] 开始应用 {NodeId} 的快照数据", nodeId);
        var applyObjects = GetNodeObjects(nodeId) ?? throw new NullReferenceException("无法获取记录");
        var currentObjects = await _GetAllTrackedObjectsAsync().ConfigureAwait(false);
        _logger?.LogInformation("[SnapLite] 获取到 {NodeId} 的对象数为 {ApplyCount}，当前文件夹对象数为 {CurrentCount}",
            nodeId, applyObjects.Count, currentObjects.Length);

        var curDict = currentObjects.ToDictionary(x => x.Path);

        List<FileVersionObjects> toDelete = [];
        List<FileVersionObjects> toAdd = [];
        List<FileVersionObjects> toUpdate = [];

        foreach (var applyObject in applyObjects)
        {
            if (curDict.TryGetValue(applyObject.Path, out var existingObject))
            {
                var isSameContent = existingObject.ObjectType == applyObject.ObjectType
                                    && existingObject.Length == applyObject.Length
                                    && existingObject.Hash == applyObject.Hash;
                var isSameMetadata = existingObject.CreationTime == applyObject.CreationTime
                                     && existingObject.LastWriteTime == applyObject.LastWriteTime;

                if (!isSameContent && !isSameMetadata) toAdd.Add(applyObject);
                if (isSameContent && !isSameMetadata) toUpdate.Add(applyObject);
            }
            else
            {
                toAdd.Add(applyObject);
            }
        }

        toDelete.AddRange(from currentObject in currentObjects
                          let existsInApply = applyObjects.Any(x => x.Path == currentObject.Path)
                          where !existsInApply
                          select currentObject);

        _logger?.LogInformation("[SnapLite] 统计出总共需要删除文件 {DeleteCount} 个，新增文件 {AddCount} 个，修改文件元数据 {UpdateCount} 个",
            toDelete.Count, toAdd.Count, toUpdate.Count);

        await toDelete
            .OrderByDescending(x => x.Path.Count(c => c == Path.DirectorySeparatorChar))
            .ThenBy(x => (int)(x.ObjectType))
            .ForEachAsync(deleteFile => Task.Run(() =>
            {
                try
                {
                    if (deleteFile.ObjectType == ObjectType.File)
                    {
                        var curFile = new FileInfo(Path.Combine(_rootPath, deleteFile.Path));
                        if (curFile.Exists) curFile.Delete();
                    }
                    else if (deleteFile.ObjectType == ObjectType.Directory)
                    {
                        var curDir = new DirectoryInfo(Path.Combine(_rootPath, deleteFile.Path));
                        try
                        {
                            if (curDir.Exists) curDir.Delete(true);
                        }
                        catch (IOException e)
                        {
                            _logger?.LogWarning(e, "[SnapLite] 目录 {Directory} 可能已受到外部修改，存在不属于此版本的内容且删除操作失败", curDir);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger?.LogError(e, "[SnapLite] 删除 {Path} 对象时出现错误，对象类型: {ObjectType}，对象 SHA512: {Hash}，对象大小: {Length}",
                        deleteFile.Path, deleteFile.ObjectType, deleteFile.Hash, deleteFile.Length);
                    throw;
                }
            }), 25).ConfigureAwait(false);

        _logger?.LogInformation("[SnapLite] 已完成文件的删除");

        await toAdd
            .OrderByDescending(x => (int)(x.ObjectType))
            .ForEachAsync(addFile => Task.Run(async () =>
            {
                try
                {
                    switch (addFile.ObjectType)
                    {
                        case ObjectType.File:
                        {
                            var curFilePath = Path.Combine(_rootPath, addFile.Path);
                            var fileFolder = Path.GetDirectoryName(curFilePath);
                            if (fileFolder is null) throw new NullReferenceException($"无法获取 {curFilePath} 的目录信息");
                            if (!Directory.Exists(fileFolder)) Directory.CreateDirectory(fileFolder);

                            var curFile = new FileInfo(curFilePath);
                            if (curFile.Exists) curFile.Delete();

                            using var ctx = GetObjectContent(addFile.Hash) ??
                                            throw new NullReferenceException("获取记录文件信息出现错误");
                            using (var fs = curFile.Create())
                            {
                                await ctx.CopyToAsync(fs).ConfigureAwait(false);
                            }

                            curFile.CreationTime = addFile.CreationTime;
                            curFile.LastWriteTime = addFile.LastWriteTime;
                            break;
                        }
                        case ObjectType.Directory:
                        {
                            var curDir = new DirectoryInfo(Path.Combine(_rootPath, addFile.Path));
                            if (!curDir.Exists) curDir.Create();
                            curDir.CreationTime = addFile.CreationTime;
                            curDir.LastWriteTime = addFile.LastWriteTime;
                            break;
                        }
                        default:
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger?.LogError(e, "[SnapLite] 修改/增添 {Path} 对象时出现错误，对象类型: {ObjectType}，对象 SHA512: {Hash}，对象大小: {Length}",
                        addFile.Path, addFile.ObjectType, addFile.Hash, addFile.Length);
                    throw;
                }
            }), 12).ConfigureAwait(false);

        _logger?.LogInformation("[SnapLite] 已完成文件的增添");

        await toUpdate
            .ForEachAsync(updateObject => Task.Run(() =>
            {
                try
                {
                    switch (updateObject.ObjectType)
                    {
                        case ObjectType.File:
                        {
                            var curFile = new FileInfo(Path.Combine(_rootPath, updateObject.Path));
                            if (!curFile.Exists)
                            {
                                _logger?.LogWarning("[SnapLite] 欲修改的文件不存在 {Path}", updateObject.Path);
                                return;
                            }
                            curFile.LastWriteTime = updateObject.LastWriteTime;
                            curFile.CreationTime = updateObject.CreationTime;
                            break;
                        }
                        case ObjectType.Directory:
                        {
                            var curDir = new DirectoryInfo(Path.Combine(_rootPath, updateObject.Path));
                            if (!curDir.Exists)
                            {
                                _logger?.LogWarning("[SnapLite] 欲修改的文件夹不存在 {Path}", updateObject.Path);
                                return;
                            }
                            curDir.LastWriteTime = updateObject.LastWriteTime;
                            curDir.CreationTime = updateObject.CreationTime;
                            break;
                        }
                        default:
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger?.LogError(e, "[SnapLite] 更新文件元数据时出错");
                    throw;
                }
            }), 20).ConfigureAwait(false);

        _logger?.LogInformation("[SnapLite] 已完成文件元数据修改");
    }

    public async Task<bool> CheckVersion(string nodeId, bool deepCheck = false)
    {
        var fileObjects = GetNodeObjects(nodeId)?.Distinct(FileVersionObjectsComparer.Instance);
        if (fileObjects is null) return false;

        var checkTasks = fileObjects
            .SelectAsync(async x =>
            {
                var filePath = Path.Combine(_objectsFolder, x.Hash);
                if (!deepCheck) return File.Exists(filePath);

                if (!File.Exists(filePath)) return false;
                using var ctx = GetObjectContent(x.Hash);
                if (ctx != null) return (await _hashProvider.ComputeHashAsync(ctx).ConfigureAwait(false)).ToHexString() == x.Hash;
                _logger?.LogWarning("[SnapLite] 无法打开指定对象的文件流：{Hash}", x.Hash);
                return false;
            }, deepCheck ? 12 : 25);

        return !(await checkTasks.ConfigureAwait(false)).ToArray().Any(x => !x);
    }

    public async Task CleanUnrecordObjects()
    {
        var nodeList = (await _LoadIndexAsync()).ToArray();

        List<string> objectsInRecord = [];
        foreach (var node in nodeList)
        {
            var nodeObjects = await _LoadNodeObjectsAsync(node.NodeId);
            objectsInRecord.AddRange(nodeObjects.Select(x => x.Hash));
        }
        objectsInRecord = [..objectsInRecord.Distinct()];

        var allObjects = Directory.EnumerateFiles(_objectsFolder)
            .Select(path => Path.GetFileName(path))
            .Where(x => !string.IsNullOrEmpty(x));

        var uselessObjects = allObjects.Except(objectsInRecord).ToArray();
        _logger?.LogInformation("[SnapLite] 寻找到 {Count} 个可清理对象", uselessObjects.Length);

        await uselessObjects.ForEachAsync(x => Task.Run(async () =>
        {
            try
            {
                await _DeleteObjectAsync(x).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "[SnapLite] 删除文件 {File} 失败。", x);
                throw;
            }
        }), 20);
    }

    public async Task Export(string nodeId, string saveFilePath)
    {
        var fileObjects = GetNodeObjects(nodeId) ?? throw new NullReferenceException("获取记录失败");
        if (File.Exists(saveFilePath))
            File.Delete(saveFilePath);

        await using var fs = new FileStream(saveFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        using var targetZip = new ZipArchive(fs, ZipArchiveMode.Update);

        fileObjects = fileObjects
            .OrderByDescending(x => (int)x.ObjectType)
            .ToList();

        foreach (var fileObject in fileObjects)
        {
            switch (fileObject.ObjectType)
            {
                case ObjectType.File:
                {
                    var entry = targetZip.CreateEntry(fileObject.Path);
                    entry.LastWriteTime = fileObject.LastWriteTime;
                    await using var writer = entry.Open();
                    await using var reader = GetObjectContent(fileObject.Hash) ?? throw new Exception("无法找到存储的文件");
                    await reader.CopyToAsync(writer).ConfigureAwait(false);
                    break;
                }
                case ObjectType.Directory:
                {
                    var entry = targetZip.CreateEntry($"{fileObject.Path}{Path.DirectorySeparatorChar}");
                    entry.LastWriteTime = fileObject.LastWriteTime;
                    break;
                }
                default:
                {
                    throw new NotSupportedException();
                }
            }
        }
    }

    private bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}