using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PCL_CE.Neo.Core.Minecraft.Java.Scanner;

public class WhereCommandScanner : IJavaScanner
{
    public void Scan(ICollection<string> results)
    {
        try
        {
            string command;
            string arguments;

            if (OperatingSystem.IsWindows())
            {
                command = "where";
                arguments = "java";
            }
            else
            {
                command = "which";
                arguments = "java";
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
                return;

            process.WaitForExit(3000);
            var output = process.StandardOutput.ReadToEnd();

            foreach (var line in output.Split('\n', '\r').Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                var path = line.Trim();
                if (System.IO.File.Exists(path))
                    results.Add(path);
            }
        }
        catch
        {
        }
    }
}