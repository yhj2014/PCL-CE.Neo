using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.App.Essentials;

public sealed class SingleInstanceService
{
    private static FileStream? _lockStream;
    private static readonly string _LockFilePath = Path.Combine(Paths.SharedLocalData, "instance.lock");

    private static void _TryRpc(string processId, string content)
    {
        var pipeName = $"PCLCE_RPC@{processId}";
        try
        {
            using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            pipe.Connect(1000);
            using var sw = new StreamWriter(pipe, Encoding.UTF8);
            sw.WriteLine(content);
            sw.Flush();
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "SingleInstanceService", "RPC 通信失败");
        }
    }

    public static bool TryAcquireLock(out string? existingPid)
    {
        existingPid = null;
        try
        {
            var stream = File.Open(_LockFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            LogWrapper.Info("SingleInstanceService", "未发现重复实例，正在向单例锁写入信息");
            using var sw = new StreamWriter(stream, Encoding.ASCII, 8, true);
            sw.Write(Basics.CurrentProcessId);
            sw.Flush();
            _lockStream = stream;
            return true;
        }
        catch (Exception)
        {
            try
            {
                using var stream = File.Open(_LockFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                existingPid = reader.ReadToEnd();
                LogWrapper.Info("SingleInstanceService", $"发现重复实例 {existingPid}，尝试传递参数");
                _TryRpc(existingPid, "REQ cli\n" + JsonSerializer.Serialize(Array.Empty<string>()));
                _TryRpc(existingPid, "REQ activate");
            }
            catch (Exception ex)
            {
                LogWrapper.Error(ex, "SingleInstanceService", "读取单例锁出错");
            }
            return false;
        }
    }

    public static void ReleaseLock()
    {
        if (_lockStream == null) return;
        LogWrapper.Info("SingleInstanceService", "正在删除单例锁");
        _lockStream.Dispose();
        try
        {
            File.Delete(_LockFilePath);
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "SingleInstanceService", "删除单例锁失败");
        }
    }
}