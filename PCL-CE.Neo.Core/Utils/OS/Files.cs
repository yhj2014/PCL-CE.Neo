using System.IO;

namespace PCL_CE.Neo.Core.Utils.OS;

public static class Files
{
    public static bool Exists(string path)
    {
        return File.Exists(path);
    }

    public static bool Delete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<string?> ReadTextAsync(string path)
    {
        try
        {
            return await File.ReadAllTextAsync(path).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }

    public static async Task<bool> WriteTextAsync(string path, string content)
    {
        try
        {
            await File.WriteAllTextAsync(path, content).ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<byte[]?> ReadBytesAsync(string path)
    {
        try
        {
            return await File.ReadAllBytesAsync(path).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }

    public static async Task<bool> WriteBytesAsync(string path, byte[] content)
    {
        try
        {
            await File.WriteAllBytesAsync(path, content).ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static long GetSize(string path)
    {
        try
        {
            return new FileInfo(path).Length;
        }
        catch
        {
            return 0;
        }
    }

    public static DateTime GetLastWriteTime(string path)
    {
        try
        {
            return File.GetLastWriteTime(path);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    public static bool Copy(string sourcePath, string destinationPath, bool overwrite = false)
    {
        try
        {
            File.Copy(sourcePath, destinationPath, overwrite);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool Move(string sourcePath, string destinationPath)
    {
        try
        {
            File.Move(sourcePath, destinationPath);
            return true;
        }
        catch
        {
            return false;
        }
    }
}