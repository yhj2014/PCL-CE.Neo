using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace PCL_CE.Neo.Core.Utils.VersionControl;

public static class FileVersionObjectsComparer
{
    public static bool FilesEqual(string path1, string path2)
    {
        try
        {
            if (!File.Exists(path1) || !File.Exists(path2))
                return false;

            var hash1 = ComputeFileHash(path1);
            var hash2 = ComputeFileHash(path2);

            return hash1.Equals(hash2, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to compare files");
            return false;
        }
    }

    public static string ComputeFileHash(string filePath)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to compute hash for: {filePath}");
            return string.Empty;
        }
    }

    public static bool DirectoryContentEqual(string dir1, string dir2)
    {
        try
        {
            if (!Directory.Exists(dir1) || !Directory.Exists(dir2))
                return false;

            var files1 = Directory.GetFiles(dir1, "*", SearchOption.AllDirectories);
            var files2 = Directory.GetFiles(dir2, "*", SearchOption.AllDirectories);

            if (files1.Length != files2.Length)
                return false;

            var fileMap1 = new Dictionary<string, string>();
            foreach (var file in files1)
            {
                var relativePath = file.Substring(dir1.Length).TrimStart(Path.DirectorySeparatorChar);
                fileMap1[relativePath] = file;
            }

            var fileMap2 = new Dictionary<string, string>();
            foreach (var file in files2)
            {
                var relativePath = file.Substring(dir2.Length).TrimStart(Path.DirectorySeparatorChar);
                fileMap2[relativePath] = file;
            }

            foreach (var pair in fileMap1)
            {
                if (!fileMap2.ContainsKey(pair.Key))
                    return false;

                if (!FilesEqual(pair.Value, fileMap2[pair.Key]))
                    return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to compare directory content");
            return false;
        }
    }

    public static List<string> GetMissingFiles(string sourceDir, string targetDir)
    {
        try
        {
            var missing = new List<string>();

            if (!Directory.Exists(sourceDir))
                return missing;

            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            var sourceFiles = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);

            foreach (var sourceFile in sourceFiles)
            {
                var relativePath = sourceFile.Substring(sourceDir.Length).TrimStart(Path.DirectorySeparatorChar);
                var targetFile = Path.Combine(targetDir, relativePath);

                if (!File.Exists(targetFile))
                    missing.Add(relativePath);
            }

            return missing;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to get missing files");
            return new List<string>();
        }
    }

    public static List<string> GetModifiedFiles(string sourceDir, string targetDir)
    {
        try
        {
            var modified = new List<string>();

            if (!Directory.Exists(sourceDir) || !Directory.Exists(targetDir))
                return modified;

            var sourceFiles = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);

            foreach (var sourceFile in sourceFiles)
            {
                var relativePath = sourceFile.Substring(sourceDir.Length).TrimStart(Path.DirectorySeparatorChar);
                var targetFile = Path.Combine(targetDir, relativePath);

                if (File.Exists(targetFile) && !FilesEqual(sourceFile, targetFile))
                    modified.Add(relativePath);
            }

            return modified;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to get modified files");
            return new List<string>();
        }
    }

    public static List<string> GetExtraFiles(string sourceDir, string targetDir)
    {
        try
        {
            var extra = new List<string>();

            if (!Directory.Exists(targetDir))
                return extra;

            if (!Directory.Exists(sourceDir))
            {
                var targetFiles = Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories);
                foreach (var file in targetFiles)
                {
                    var relativePath = file.Substring(targetDir.Length).TrimStart(Path.DirectorySeparatorChar);
                    extra.Add(relativePath);
                }
                return extra;
            }

            var sourceFiles = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
            var sourceFileSet = new HashSet<string>();

            foreach (var sourceFile in sourceFiles)
            {
                var relativePath = sourceFile.Substring(sourceDir.Length).TrimStart(Path.DirectorySeparatorChar);
                sourceFileSet.Add(relativePath);
            }

            var targetFiles = Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories);
            foreach (var targetFile in targetFiles)
            {
                var relativePath = targetFile.Substring(targetDir.Length).TrimStart(Path.DirectorySeparatorChar);
                if (!sourceFileSet.Contains(relativePath))
                    extra.Add(relativePath);
            }

            return extra;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to get extra files");
            return new List<string>();
        }
    }
}