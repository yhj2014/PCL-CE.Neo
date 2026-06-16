using System.IO;
using PCL_CE.Neo.Core.Utils;

namespace PCL_CE.Neo.Core.Configuration.Storage;

public abstract class CommonFileProvider(string path) : IKeyValueFileProvider
{
    public string FilePath { get; set; } = path;

    public abstract T Get<T>(string key);
    public abstract void Set<T>(string key, T value);
    public abstract bool Exists(string key);
    public abstract void Delete(string key);

    protected abstract void WriteToStream(Stream stream);

    public void Sync()
    {
        var dir = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        
        var tmpFile = $"{FilePath}.tmp{RandomUtils.NextInt(1, 99999):00000}";
        var bakFile = $"{FilePath}.bak";
        
        using (var stream = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.Read))
        {
            WriteToStream(stream);
            stream.Flush(true);
        }

        if (File.Exists(FilePath))
            File.Replace(tmpFile, FilePath, bakFile);
        else
            File.Move(tmpFile, FilePath);
    }
}