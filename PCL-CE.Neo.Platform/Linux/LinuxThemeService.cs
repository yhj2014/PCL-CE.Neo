using PCL.CE.Neo.Core.Abstractions;

namespace PCL.CE.Neo.Platform.Linux;

public class LinuxThemeService : IThemeService
{
    public event EventHandler? ThemeChanged;

    public ThemeInfo GetCurrentTheme()
    {
        return new ThemeInfo
        {
            Name = "Light",
            Type = ThemeType.Light,
            ResourcePath = string.Empty
        };
    }

    public void SetTheme(ThemeInfo theme)
    {
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    public IEnumerable<ThemeInfo> GetAvailableThemes()
    {
        return new[]
        {
            new ThemeInfo { Name = "Light", Type = ThemeType.Light, ResourcePath = string.Empty },
            new ThemeInfo { Name = "Dark", Type = ThemeType.Dark, ResourcePath = string.Empty }
        };
    }

    public ThemeType DetectSystemTheme()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "gsettings",
                    Arguments = "get org.gnome.desktop.interface color-scheme",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            var result = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (result.Contains("dark", StringComparison.OrdinalIgnoreCase))
            {
                return ThemeType.Dark;
            }
        }
        catch
        {
        }

        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "gsettings",
                    Arguments = "get org.gnome.desktop.interface gtk-theme",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            var result = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (result.Contains("dark", StringComparison.OrdinalIgnoreCase))
            {
                return ThemeType.Dark;
            }
        }
        catch
        {
        }

        return ThemeType.Light;
    }
}
