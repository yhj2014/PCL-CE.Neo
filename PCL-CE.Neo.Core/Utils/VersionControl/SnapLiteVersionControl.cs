using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using PCL_CE.Neo.Core.IO.Storage;
using PCL_CE.Neo.Core.Logging;
using PCL_CE.Neo.Core.Utils.Exts;
using PCL_CE.Neo.Core.Utils.Hash;

namespace PCL_CE.Neo.Core.Utils.VersionControl;

public class SnapLiteVersionControl : IVersionControl, IDisposable
{
    private const string ModuleName = "SnapLite";
    private readonly string _rootPath;
    private readonly LiteDatabase _database;
    private static readonly IHashProvider _HashProvider = SHA512Provider.Instance;
    private readonly HashStorage _storage;

    private const string ConfigFolderName = ".litesnap";
    private const string DatabaseName = "index.db";
    private const string DatabaseIndexTableName = "index";
    private const string ObjectsFolderName = "objects";

    public SnapLiteVersionControl(string rootPath)
    {
        try
        {
            _rootPath = rootPath;
            var dbFile = Path.Combine(_rootPath, ConfigFolderName, DatabaseName);
            var objFolder = Path.Combine(_rootPath, ConfigFolderName, ObjectsFolderName);
            if (!Directory.Exists(objFolder))
                Directory.CreateDirectory(objFolder);
            _storage = new HashStorage(objFolder, _HashProvider, true);
            _database = new LiteDatabase($"Filename={dbFile}");
        }
        catch (Exception e)
        {
            LogWrapper.Error(e, ModuleName, $"无法加载位于 {_rootPath} 处的 SnapLite 数据");
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
                scannedPaths.Add(new FileVersionObjects()
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
                return new FileVersionObjects()
                {
                    CreationTime = file.CreationTime,
                    Hash = (await _HashProvider.ComputeHashAsync(fs).ConfigureAwait(false)).ToHexString(),
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

    public async Task<string> CreateNewVersion(string? name = null, string? desc = null)
    {
        try
        {
            var nodeId = Guid.NewGuid().ToString("N");
            var allFiles = await _GetAllTrackedObjectsAsync().ConfigureAwait(false);
            LogWrapper.Info(ModuleName, $"已获取到全部文件，总数量为 {allFiles.Length}");
            
            var newAddFiles = allFiles
                .Distinct(FileVersionObjectsComparer.Instance)
                .Where(x => x.ObjectType.Equals(ObjectType.Directory) ||
                            (x.ObjectType.Equals(ObjectType.File) && !_storage.Exists(x.Hash)))
                .ToList();
            
            LogWrapper.Info(ModuleName, $"新增对象总数量为 {newAddFiles.Count}");
            var nodeObjects = _database.GetCollection<FileVersionObjects>(_GetNodeTableNameById(nodeId));
            nodeObjects.InsertBulk(allFiles);
            LogWrapper.Info(ModuleName, "记录已压入数据库，开始存储文件");

            await newAddFiles
                .Where(x => x.ObjectType == ObjectType.File)
                .ForEachAsync(x => Task.Run(async () =>
                {
                    var filePath = Path.Combine(_rootPath, x.Path);
                    try
                    {
                        LogWrapper.Info(ModuleName, $"将 {filePath} 放入哈希仓库");
                        await _storage.PutAsync(filePath, x.Hash).ConfigureAwait(false);
                        LogWrapper.Info(ModuleName, $"已完成 {filePath} 的存储");
                    }
                    catch (Exception e)
                    {
                        LogWrapper.Error(e, ModuleName, $"存储 {filePath} 文件过程中出现错误");
                        throw;
                    }
                }), 12).ConfigureAwait(false);

            LogWrapper.Info(ModuleName, "文件存储任务完成");
            
            var nodeList = _database.GetCollection<VersionData>(DatabaseIndexTableName);
            var currentNodeInfo = new VersionData()
            {
                Created = DateTime.Now,
                Desc = desc ?? "Backup made by SnapLite",
                Name = name ?? $"{DateTime.Now:yyyy/dd/MM-HH:mm:ss}",
                NodeId = nodeId,
                Version = 1
            };
            nodeList.Insert(currentNodeInfo);
            LogWrapper.Info(ModuleName, "数据库记录更新完成");
            return nodeId;
        }
        catch (Exception e)
        {
            LogWrapper.Error(e, ModuleName, "创建快照出错");
            throw;
        }
    }

    private static string _GetNodeTableNameById(string nodeId)
    {
        return $"node_{nodeId}";
    }

    public VersionData? GetVersion(string nodeId)
    {
        var nodeList = _database.GetCollection<VersionData>(DatabaseIndexTableName);
        return nodeList.FindOne(x => x.NodeId == nodeId);
    }

    public List<VersionData> GetVersions()
    {
        var nodeList = _database.GetCollection<VersionData>(DatabaseIndexTableName);
        return nodeList.Query().ToList();
    }

    public List<FileVersionObjects>? GetNodeObjects(string nodeId)
    {
        var objectList = _database.GetCollection<FileVersionObjects>(_GetNodeTableNameById(nodeId));
        return objectList?.Query().ToList();
    }

    public void DeleteVersion(string nodeId)
    {
        try
        {
            var nodeList = _database.GetCollection<VersionData>(DatabaseIndexTableName);
            nodeList.DeleteMany(x => x.NodeId == nodeId);
            _database.DropCollection(_GetNodeTableNameById(nodeId));
        }
        catch (Exception e)
        {
            LogWrapper.Error(e, ModuleName, $"删除 {nodeId} 时出现错误");
            throw;
        }
    }

    public Stream? GetObjectContent(string objectId)
    {
        try
        {
            return _storage.Get(objectId);
        }
        catch (Exception e)
        {
            LogWrapper.Error(e, ModuleName, $"获取 {objectId} 的流失败");
            throw;
        }
    }

    public async Task ApplyPastVersion(string nodeId)
    {
        LogWrapper.Info(ModuleName, $"开始应用 {nodeId} 的快照数据");
        var applyObjects = GetNodeObjects(nodeId) ?? throw new NullReferenceException("无法获取记录");
        var currentObjects = await _GetAllTrackedObjectsAsync().ConfigureAwait(false);
        LogWrapper.Info(ModuleName, $"获取到 {nodeId} 的对象数为 {applyObjects.Count}，当前文件夹对象数为 {currentObjects.Length}");
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
        LogWrapper.Info(ModuleName, $"统计出总共需要删除文件 {toDelete.Count} 个，新增文件 {toAdd.Count} 个，修改文件元数据 {toUpdate.Count} 个");

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
                        LogWrapper.Warn(e, ModuleName, $"目录 {curDir} 可能已受到外部修改，存在不属于此版本的内容且删除操作失败");
                    }
                }
            }
            catch (Exception e)
            {
                LogWrapper.Error(e, ModuleName, $"删除 {deleteFile.Path} 对象时出现错误，对象类型: {deleteFile.ObjectType}，对象 SHA512: {deleteFile.Hash}，对象大小: {deleteFile.Length}");
                throw;
            }
        }), 25).ConfigureAwait(false);

        LogWrapper.Info(ModuleName, "已完成文件的删除");

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
                    LogWrapper.Error(e, ModuleName,
                        $"修改/增添 {addFile.Path} 对象时出现错误，对象类型: {addFile.ObjectType}，对象 SHA512: {addFile.Hash}，对象大小: {addFile.Length}");
                    throw;
                }
            }), 12).ConfigureAwait(false);

        LogWrapper.Info(ModuleName, "已完成文件的增添");

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
                            LogWrapper.Warn(ModuleName, $"欲修改的文件不存在 {updateObject.Path}");
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
                            LogWrapper.Warn(ModuleName, $"欲修改的文件夹不存在 {updateObject.Path}");
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
                LogWrapper.Error(e, ModuleName, "更新文件元数据时出错");
                throw;
            }
        }), 20).ConfigureAwait(false);
        LogWrapper.Info(ModuleName, "已完成文件元数据修改");
    }

    public async Task<bool> CheckVersion(string nodeId, bool deepCheck = false)
    {
        var fileObjects = GetNodeObjects(nodeId)?.Distinct(FileVersionObjectsComparer.Instance);
        if (fileObjects is null) return false;

        var checkTasks = fileObjects
            .SelectAsync(async x =>
        {
            var filePath = Path.Combine(_rootPath, ConfigFolderName, ObjectsFolderName, x.Hash);
            if (!deepCheck) return File.Exists(filePath);

            if (!File.Exists(filePath)) return false;
            using var ctx = GetObjectContent(x.Hash);
            if (ctx != null) return (await _HashProvider.ComputeHashAsync(ctx).ConfigureAwait(false)).ToHexString() == x.Hash;
            LogWrapper.Warn(ModuleName, $"无法打开指定对象的文件流：{x.Hash}");
            return false;
        }, deepCheck ? 12 : 25);
        return (await checkTasks.ConfigureAwait(false)).ToArray().Any(x => !x);
    }

    public async Task CleanUnrecordObjects()
    {
        var nodeList = _database.GetCollection<VersionData>(DatabaseIndexTableName).Query().ToArray();

        List<string> objectsInRecord = [];
        foreach (var node in nodeList)
        {
            var nodeTable = _GetNodeTableNameById(node.NodeId);
            objectsInRecord.AddRange(_database.GetCollection<FileVersionObjects>(nodeTable).Query().ToEnumerable().Select(x => x.Hash));
        }
        objectsInRecord = [..objectsInRecord.Distinct()];

        var allObjects = Directory.EnumerateFiles(Path.Combine(_rootPath, ConfigFolderName, ObjectsFolderName))
            .Select(path => Path.GetFileName(path))
            .Where(x => !string.IsNullOrEmpty(x));

        var uselessObjects = allObjects.Except(objectsInRecord).ToArray();
        LogWrapper.Info(ModuleName, $"寻找到 {uselessObjects.Length} 个可清理对象");

        var deleteTask = uselessObjects.ForEachAsync(x => Task.Run(async () =>
        {
            try
            {
                await _storage.DeleteAsync(x).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                LogWrapper.Error(e, ModuleName, $"删除文件 {x} 失败。");
                throw;
            }
        }), 20);
        await deleteTask.ConfigureAwait(false);
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
        _database.Dispose();
        GC.SuppressFinalize(this);
    }
}