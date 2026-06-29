using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils;

public static class ProcessHelper
{
    public static async Task<int> RunProcessAsync(string fileName, string arguments, string? workingDirectory = null)
    {
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
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (!string.IsNullOrEmpty(output))
                LogWrapper.Debug($"Process output: {output}");

            if (!string.IsNullOrEmpty(error))
                LogWrapper.Warn($"Process error: {error}");

            return process.ExitCode;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to run process: {fileName} {arguments}");
            throw;
        }
    }

    public static async Task<ProcessResult> RunProcessWithResultAsync(string fileName, string arguments, string? workingDirectory = null)
    {
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
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            return new ProcessResult(process.ExitCode, output, error);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to run process: {fileName} {arguments}");
            return new ProcessResult(-1, string.Empty, ex.Message);
        }
    }

    public static async Task<string> RunProcessAndGetOutputAsync(string fileName, string arguments, string? workingDirectory = null)
    {
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
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (!string.IsNullOrEmpty(error))
                LogWrapper.Warn($"Process error: {error}");

            return output;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to run process: {fileName} {arguments}");
            throw;
        }
    }

    public static bool IsProcessRunning(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            return !process.HasExited;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, $"Failed to check if process {processId} is running");
            return false;
        }
    }

    public static bool IsProcessRunning(string processName)
    {
        try
        {
            return Process.GetProcessesByName(processName).Length > 0;
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, $"Failed to check if process {processName} is running");
            return false;
        }
    }

    public static bool KillProcess(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            process.Kill();
            process.WaitForExit(5000);
            return true;
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, $"Failed to kill process {processId}");
            return false;
        }
    }

    public static int KillProcessByName(string processName)
    {
        try
        {
            var processes = Process.GetProcessesByName(processName);
            var killedCount = 0;

            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit(5000);
                    killedCount++;
                }
                catch (Exception ex)
                {
                    LogWrapper.Warn(ex, $"Failed to kill process {process.Id}");
                }
            }

            return killedCount;
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, $"Failed to kill processes by name {processName}");
            return 0;
        }
    }
}

public record ProcessResult(int ExitCode, string Output, string Error)
{
    public bool Success => ExitCode == 0;
}