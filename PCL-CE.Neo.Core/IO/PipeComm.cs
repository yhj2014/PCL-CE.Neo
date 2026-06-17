using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.IO;

public static class PipeComm
{
    public static readonly Encoding PipeEncoding = Encoding.UTF8;
    public const char PipeEndingChar = (char)27;

    public static NamedPipeServerStream StartPipeServer(string identifier, string pipeName, Func<StreamReader, StreamWriter, Process?, bool> loopCallback, Action? stopCallback = null, bool stopWhenException = false, int[]? allowedProcessId = null)
    {
        var pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None, 1024, 1024);
        var threadName = $"PipeServer/{identifier}";

        _ = Task.Run(async () =>
        {
            var hasNextLoop = true;
            var connected = false;

            while (hasNextLoop)
            {
                try
                {
                    hasNextLoop = false;
                    pipe.WaitForConnection();

                    Process? clientProcess = null;
                    var clientProcessId = 0;

                    try
                    {
                        if (OperatingSystem.IsWindows())
                        {
                            clientProcessId = (int)Interop.Windows.KernelInterop.GetNamedPipeClientProcessId(pipe.SafePipeHandle.DangerousGetHandle());
                            if (allowedProcessId != null)
                            {
                                var denied = Array.TrueForAll(allowedProcessId, id => id != clientProcessId);
                                if (denied)
                                {
                                    hasNextLoop = true;
                                    pipe.Disconnect();
                                    continue;
                                }
                            }
                            clientProcess = Process.GetProcessById(clientProcessId);
                        }
                    }
                    catch (Exception)
                    {
                        if (allowedProcessId != null)
                        {
                            hasNextLoop = true;
                            throw;
                        }
                    }

                    connected = true;

                    var reader = new StreamReader(pipe, PipeEncoding, false, 1024, true);
                    var writer = new StreamWriter(pipe, PipeEncoding, 1024, true);

                    hasNextLoop = loopCallback(reader, writer, clientProcess);

                    writer.Write(PipeEndingChar);
                    writer.Flush();
                    reader.Read();
                }
                catch (Exception)
                {
                    if (!pipe.IsConnected && connected && isIOException)
                    {
                        hasNextLoop = true;
                    }
                    else
                    {
                        if (stopWhenException) hasNextLoop = false;
                    }
                }
                try
                {
                    pipe.Disconnect();
                }
                catch (InvalidOperationException)
                {
                }
                connected = false;
            }

            pipe.Dispose();
            stopCallback?.Invoke();
        });

        return pipe;
    }

    private static bool isIOException(Exception ex) => ex is IOException;
}