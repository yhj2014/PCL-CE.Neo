using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.App;

namespace PCL_CE.Neo.Core.App.Essentials;

public sealed class SingleInstanceService
{
    private readonly ILogger<SingleInstanceService> _logger;
    private FileStream? _lockStream;
    private static readonly string _LockFilePath = Path.Combine(Paths.SharedLocalData, "instance.lock");

    public SingleInstanceService(ILogger<SingleInstanceService> logger)
    {
        _logger = logger;
    }

    private void _TryRpc(string processId, string content)
    {
        try
        {
            var pipeName = $"PCLCE_RPC@{processId}";
            using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            pipe.Connect(1000);
            using var sw = new StreamWriter(pipe, Encoding.UTF8);
            sw.WriteLine(content);
            sw.Write('\0');
            sw.Flush();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RPC communication failed");
        }
    }

    public bool TryStart()
    {
        try
        {
            var stream = File.Open(_LockFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            _logger.LogDebug("No duplicate instance found, writing to singleton lock");
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
                var pid = reader.ReadToEnd();
                _logger.LogInformation($"Found duplicate instance {pid}, attempting to pass arguments and activate main window");
                try
                {
                    _TryRpc(pid, "REQ cli\n" + JsonSerializer.Serialize(Basics.CommandLineArguments));
                    _TryRpc(pid, "REQ activate");
                }
                catch (Exception ex) { _logger.LogWarning(ex, "RPC communication failed"); }
            }
            catch (Exception ex) { _logger.LogError(ex, "Error reading singleton lock"); }
            return false;
        }
    }

    public void Stop()
    {
        if (_lockStream == null) return;
        _logger.LogDebug("Deleting singleton lock");
        try
        {
            _lockStream.Dispose();
            File.Delete(_LockFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error cleaning up singleton lock");
        }
    }
}