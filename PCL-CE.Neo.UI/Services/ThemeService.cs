namespace PCL_CE.Neo.UI.Services;

public class ThemeService : Core.Abstractions.IThemeService
{
    private Core.Abstractions.AppTheme _cachedTheme = Core.Abstractions.AppTheme.Light;
    private string _cachedAccentColor = "#0078D4";
    private bool _initialized = false;

    public Core.Abstractions.AppTheme GetSystemTheme()
    {
        if (!_initialized)
        {
            InitializeTheme();
        }
        return _cachedTheme;
    }

    public bool IsDarkMode()
    {
        return GetSystemTheme() == Core.Abstractions.AppTheme.Dark;
    }

    public string GetAccentColor()
    {
        if (!_initialized)
        {
            InitializeTheme();
        }
        return _cachedAccentColor;
    }

    private void InitializeTheme()
    {
#if WINDOWS
        InitializeWindowsTheme();
#elif MACCATALYST
        InitializeMacOSTheme();
#elif LINUX
        InitializeLinuxTheme();
#endif
        _initialized = true;
    }

#if WINDOWS
    private void InitializeWindowsTheme()
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
                    _cachedTheme = lightTheme == 0
                        ? Core.Abstractions.AppTheme.Dark
                        : Core.Abstractions.AppTheme.Light;
                }
            }

            using var accentKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\DWM");
            if (accentKey != null)
            {
                var colorValue = accentKey.GetValue("AccentColor");
                if (colorValue is int color)
                {
                    var r = (color >> 16) & 0xFF;
                    var g = (color >> 8) & 0xFF;
                    var b = color & 0xFF;
                    _cachedAccentColor = $"#{r:X2}{g:X2}{b:X2}";
                }
            }
        }
        catch
        {
            _cachedTheme = Core.Abstractions.AppTheme.Light;
            _cachedAccentColor = "#0078D4";
        }
    }
#endif

#if MACCATALYST
    private void InitializeMacOSTheme()
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
                _cachedTheme = Core.Abstractions.AppTheme.Dark;
            }
            else
            {
                _cachedTheme = Core.Abstractions.AppTheme.Light;
            }

            _cachedAccentColor = "#007AFF";
        }
        catch
        {
            _cachedTheme = Core.Abstractions.AppTheme.Light;
            _cachedAccentColor = "#007AFF";
        }
    }
#endif

#if LINUX
    private void InitializeLinuxTheme()
    {
        try
        {
            var gtkTheme = GetGtkThemeSetting();
            if (!string.IsNullOrEmpty(gtkTheme) &&
                (gtkTheme.Contains("dark", StringComparison.OrdinalIgnoreCase) ||
                 gtkTheme.Contains("Dark", StringComparison.OrdinalIgnoreCase)))
            {
                _cachedTheme = Core.Abstractions.AppTheme.Dark;
            }
            else
            {
                _cachedTheme = Core.Abstractions.AppTheme.Light;
            }

            _cachedAccentColor = GetLinuxAccentColor() ?? "#0078D4";
        }
        catch
        {
            _cachedTheme = Core.Abstractions.AppTheme.Light;
            _cachedAccentColor = "#0078D4";
        }
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

    private string? GetLinuxAccentColor()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "gsettings",
                    Arguments = "get org.gnome.desktop.interface accent-color-style",
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
                var value = output.Trim();
                return value switch
                {
                    "1" => "#D7780D",
                    "2" => "#9141AC",
                    "3" => "#0F52BA",
                    "4" => "#1A5F2A",
                    _ => "#0078D4"
                };
            }
        }
        catch
        {
        }
        return "#0078D4";
    }
#endif
}
