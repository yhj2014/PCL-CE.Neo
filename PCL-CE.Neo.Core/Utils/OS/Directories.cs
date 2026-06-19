using System.IO;

namespace PCL_CE.Neo.Core.Utils.OS;

public static class Directories
{
    public static bool Exists(string path)
    {
        return Directory.Exists(path);
    }

    public static bool Create(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool Delete(string path, bool recursive = false)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public static string[] GetFiles(string path, string searchPattern = "*")
    {
        try
        {
            return Directory.GetFiles(path, searchPattern);
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public static string[] GetDirectories(string path, string searchPattern = "*")
    {
        try
        {
            return Directory.GetDirectories(path, searchPattern);
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public static long GetSize(string path)
    {
        try
        {
            if (!Directory.Exists(path))
                return 0;

            long size = 0;
            foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                size += new FileInfo(file).Length;
            }
            return size;
        }
        catch
        {
            return 0;
        }
    }

    public static bool Copy(string sourcePath, string destinationPath, bool recursive = false)
    {
        try
        {
            if (!Directory.Exists(sourcePath))
                return false;

            Directory.CreateDirectory(destinationPath);

            foreach (var file in Directory.GetFiles(sourcePath))
            {
                var destFile = Path.Combine(destinationPath, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            if (recursive)
            {
                foreach (var dir in Directory.GetDirectories(sourcePath))
                {
                    var destDir = Path.Combine(destinationPath, Path.GetFileName(dir));
                    Copy(dir, destDir, true);
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}