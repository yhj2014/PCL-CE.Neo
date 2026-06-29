using System;
using System.IO;
using System.IO.Compression;

namespace PCL_CE.Neo.Core.Utils;

public static class CompressionHelper
{
    public static void CompressFile(string sourceFilePath, string destinationFilePath)
    {
        try
        {
            using var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
            using var destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write);
            using var gzipStream = new GZipStream(destinationStream, CompressionLevel.Optimal);
            
            sourceStream.CopyTo(gzipStream);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to compress file: {sourceFilePath}");
            throw;
        }
    }

    public static void DecompressFile(string sourceFilePath, string destinationFilePath)
    {
        try
        {
            using var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
            using var destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write);
            using var gzipStream = new GZipStream(sourceStream, CompressionMode.Decompress);
            
            gzipStream.CopyTo(destinationStream);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to decompress file: {sourceFilePath}");
            throw;
        }
    }

    public static byte[] CompressBytes(byte[] data)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            using var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal);
            
            gzipStream.Write(data, 0, data.Length);
            gzipStream.Flush();
            
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to compress bytes");
            throw;
        }
    }

    public static byte[] DecompressBytes(byte[] compressedData)
    {
        try
        {
            using var memoryStream = new MemoryStream(compressedData);
            using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            
            gzipStream.CopyTo(resultStream);
            
            return resultStream.ToArray();
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to decompress bytes");
            throw;
        }
    }

    public static void CreateZipArchive(string zipFilePath, string sourceDirectory)
    {
        try
        {
            if (File.Exists(zipFilePath))
                File.Delete(zipFilePath);
            
            ZipFile.CreateFromDirectory(sourceDirectory, zipFilePath, CompressionLevel.Optimal, false);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to create zip archive: {zipFilePath}");
            throw;
        }
    }

    public static void ExtractZipArchive(string zipFilePath, string destinationDirectory)
    {
        try
        {
            if (!Directory.Exists(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);
            
            ZipFile.ExtractToDirectory(zipFilePath, destinationDirectory);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to extract zip archive: {zipFilePath}");
            throw;
        }
    }

    public static long GetCompressedSize(string filePath)
    {
        try
        {
            using var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var memoryStream = new MemoryStream();
            using var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal);
            
            sourceStream.CopyTo(gzipStream);
            
            return memoryStream.Length;
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, $"Failed to get compressed size for: {filePath}");
            return -1;
        }
    }

    public static double GetCompressionRatio(string filePath)
    {
        try
        {
            var originalSize = new FileInfo(filePath).Length;
            var compressedSize = GetCompressedSize(filePath);
            
            if (compressedSize <= 0)
                return 0;
            
            return (double)compressedSize / originalSize;
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, $"Failed to get compression ratio for: {filePath}");
            return 0;
        }
    }
}