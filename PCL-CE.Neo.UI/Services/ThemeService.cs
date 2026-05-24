namespace PCL_CE.Neo.UI.Services;

public class ThemeService : Core.Abstractions.IThemeService
{
    public event EventHandler? ThemeChanged;

    private Core.Abstractions.ThemeInfo _currentTheme = new() { Name = "Light", Type = Core.Abstractions.ThemeType.Light, ResourcePath = "Themes/Light" };
    private bool _initialized = false;

    public Core.Abstractions.ThemeInfo GetCurrentTheme()
    {
        if (!_initialized)
        {
            InitializeTheme();
        }
        return _currentTheme;
    }

    public void SetTheme(Core.Abstractions.ThemeInfo theme)
    {
        _currentTheme = theme;
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    public IEnumerable<Core.Abstractions.ThemeInfo> GetAvailableThemes()
    {
        return new List<Core.Abstractions.ThemeInfo>
        {
            new() { Name = "Light", Type = Core.Abstractions.ThemeType.Light, ResourcePath = "Themes/Light" },
            new() { Name = "Dark", Type = Core.Abstractions.ThemeType.Dark, ResourcePath = "Themes/Dark" },
            new() { Name = "System", Type = Core.Abstractions.ThemeType.System, ResourcePath = "Themes/System" }
        };
    }

    public Core.Abstractions.ThemeType DetectSystemTheme()
    {
        if (!_initialized)
        {
            InitializeTheme();
        }
        return _currentTheme.Type;
    }

    private void InitializeTheme()
    {
        var systemThemeType = DetectSystemThemeCore();
        _currentTheme = new Core.Abstractions.ThemeInfo
        {
            Name = systemThemeType.ToString(),
            Type = systemThemeType,
            ResourcePath = $"Themes/{systemThemeType}"
        };
        _initialized = true;
    }

    private Core.Abstractions.ThemeType DetectSystemThemeCore()
    {
#if WINDOWS
        return DetectWindowsTheme();
#elif MACCATALYST
        return DetectMacOSTheme();
#elif LINUX
        return DetectLinuxTheme();
#else
        return Core.Abstractions.ThemeType.Light;
#endif
    }

#if WINDOWS
    private Core.Abstractions.ThemeType DetectWindowsTheme()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key != null)
            {
                var value = key.GetValue("AppsUseLightTheme");
                if (value is int lightTheme)
                {
                    return lightTheme == 0 ? Core.Abstractions.ThemeType.Dark : Core.Abstractions.ThemeType.Light;
                }
            }
        }
        catch
        {
        }
        return Core.Abstractions.ThemeType.Light;
    }
#endif

#if MACCATALYST
    private Core.Abstractions.ThemeType DetectMacOSTheme()
    {
        try
        {
            var script = @"
tell application ""System Events""
    tell appearance preferences
        get properties
    end tell
end tell";

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments = $"-e '{script}'",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);

            if (output.Contains("dark aura") || output.Contains("dark"))
            {
                return Core.Abstractions.ThemeType.Dark;
            }
        }
        catch
        {
        }
        return Core.Abstractions.ThemeType.Light;
    }
#endif

#if LINUX
    private Core.Abstractions.ThemeType DetectLinuxTheme()
    {
        try
        {
            var gtkTheme = GetGtkThemeSetting();
            if (!string.IsNullOrEmpty(gtkTheme) &&
                (gtkTheme.Contains("dark", StringComparison.OrdinalIgnoreCase) ||
                 gtkTheme.Contains("Dark", StringComparison.OrdinalIgnoreCase)))
            {
                return Core.Abstractions.ThemeType.Dark;
            }
        }
        catch
        {
        }
        return Core.Abstractions.ThemeType.Light;
    }

    private string? GetGtkThemeSetting()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "gsettings",
                    Arguments = "get org.gnome.desktop.interface gtk-theme",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadLine();
            process.WaitForExit(2000);

            if (!string.IsNullOrEmpty(output))
            {
                return output.Trim().Trim('"');
            }
        }
        catch
        {
        }
        return null;
    }
#endif
}