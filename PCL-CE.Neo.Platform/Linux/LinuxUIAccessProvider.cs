namespace PCL_CE.Neo.Platform.Linux;

public class LinuxUIAccessProvider : IUIAccessProvider
{
    public void Invoke(Action action)
    {
        // No UI thread marshalling needed for basic implementation
        action();
    }

    public Task InvokeAsync(Action action)
    {
        action();
        return Task.CompletedTask;
    }

    public bool CheckAccess()
    {
        return true;
    }

    public double GetScreenDpi()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "xrdb",
                    Arguments = "-query",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            foreach (var line in output.Split('\n'))
            {
                if (line.StartsWith("Xft.dpi:"))
                {
                    var parts = line.Split(':', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1 && double.TryParse(parts[1].Trim(), out var dpi))
                    {
                        return dpi;
                    }
                }
            }
        }
        catch
        {
            // Fallback
        }

        return 96.0;
    }

    public (int Width, int Height) GetScreenSize()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "xdotool",
                    Arguments = "getdisplaygeometry",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            var parts = output.Split(' ');
            if (parts.Length >= 2 && int.TryParse(parts[0], out var width) && int.TryParse(parts[1], out var height))
            {
                return (width, height);
            }
        }
        catch
        {
            // Fallback
        }

        return (1920, 1080);
    }
}
