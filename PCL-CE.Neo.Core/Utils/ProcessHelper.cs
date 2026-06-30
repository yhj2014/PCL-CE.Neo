using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Utils;

public class ProcessHelper
{
    private readonly ILogger<ProcessHelper> _logger;

    public ProcessHelper(ILogger<ProcessHelper> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProcessResult> RunProcessAsync(string fileName, string arguments, 
        string? workingDirectory = null, int timeoutMs = 30000)
    {
        return await RunProcessAsync(fileName, arguments, workingDirectory, timeoutMs, CancellationToken.None);
    }

    public async Task<ProcessResult> RunProcessAsync(string fileName, string arguments, 
        string? workingDirectory, int timeoutMs, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Starting process: {FileName} {Arguments}", fileName, arguments);

        var result = new ProcessResult();

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            var outputBuilder = new List<string>();
            var errorBuilder = new List<string>();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputBuilder.Add(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorBuilder.Add(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var completedTask = process.WaitForExitAsync(cancellationToken);
            var timeoutTask = Task.Delay(timeoutMs, cancellationToken);

            var completed = await Task.WhenAny(completedTask, timeoutTask);

            if (completed == timeoutTask)
            {
                try
                {
                    process.Kill();
                }
                catch
                {
                }

                result.Success = false;
                result.ExitCode = -1;
                result.Output = string.Join("\n", outputBuilder);
                result.Error = "Process timed out";
                _logger.LogWarning("Process timed out: {FileName} {Arguments}", fileName, arguments);
            }
            else
            {
                result.Success = process.ExitCode == 0;
                result.ExitCode = process.ExitCode;
                result.Output = string.Join("\n", outputBuilder);
                result.Error = string.Join("\n", errorBuilder);

                if (result.Success)
                {
                    _logger.LogDebug("Process completed successfully: {FileName}", fileName);
                }
                else
                {
                    _logger.LogWarning("Process failed with exit code {ExitCode}: {FileName} {Arguments}", 
                        result.ExitCode, fileName, arguments);
                }
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ExitCode = -2;
            result.Error = ex.Message;
            _logger.LogError(ex, "Failed to run process: {FileName} {Arguments}", fileName, arguments);
        }

        return result;
    }

    public ProcessResult RunProcess(string fileName, string arguments, 
        string? workingDirectory = null, int timeoutMs = 30000)
    {
        _logger.LogDebug("Starting process synchronously: {FileName} {Arguments}", fileName, arguments);

        var result = new ProcessResult();

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();

            if (!process.WaitForExit(timeoutMs))
            {
                try
                {
                    process.Kill();
                }
                catch
                {
                }

                result.Success = false;
                result.ExitCode = -1;
                result.Error = "Process timed out";
                _logger.LogWarning("Process timed out: {FileName} {Arguments}", fileName, arguments);
            }
            else
            {
                result.Success = process.ExitCode == 0;
                result.ExitCode = process.ExitCode;
                result.Output = process.StandardOutput.ReadToEnd();
                result.Error = process.StandardError.ReadToEnd();

                if (result.Success)
                {
                    _logger.LogDebug("Process completed successfully: {FileName}", fileName);
                }
                else
                {
                    _logger.LogWarning("Process failed with exit code {ExitCode}: {FileName} {Arguments}", 
                        result.ExitCode, fileName, arguments);
                }
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ExitCode = -2;
            result.Error = ex.Message;
            _logger.LogError(ex, "Failed to run process: {FileName} {Arguments}", fileName, arguments);
        }

        return result;
    }

    public bool IsProcessRunning(string processName)
    {
        try
        {
            return Process.GetProcessesByName(processName).Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if process {ProcessName} is running", processName);
            return false;
        }
    }

    public bool KillProcess(string processName)
    {
        try
        {
            var processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                process.Kill();
                process.WaitForExit();
            }
            _logger.LogDebug("Killed {Count} processes named {ProcessName}", processes.Length, processName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to kill process {ProcessName}", processName);
            return false;
        }
    }

    public bool KillProcess(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            process.Kill();
            process.WaitForExit();
            _logger.LogDebug("Killed process with ID {ProcessId}", processId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to kill process with ID {ProcessId}", processId);
            return false;
        }
    }

    public Process? GetProcessById(int processId)
    {
        try
        {
            return Process.GetProcessById(processId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get process with ID {ProcessId}", processId);
            return null;
        }
    }

    public IEnumerable<Process> GetProcessesByName(string processName)
    {
        try
        {
            return Process.GetProcessesByName(processName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get processes named {ProcessName}", processName);
            return Enumerable.Empty<Process>();
        }
    }

    public long GetProcessMemoryUsage(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            return process.WorkingSet64;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get memory usage for process {ProcessId}", processId);
            return 0;
        }
    }

    public int GetProcessCpuUsage(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            return process.Threads.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get CPU usage for process {ProcessId}", processId);
            return 0;
        }
    }
}

public class ProcessResult
{
    public bool Success { get; set; }
    public int ExitCode { get; set; }
    public string? Output { get; set; }
    public string? Error { get; set; }
}