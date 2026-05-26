using Microsoft.Win32;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsThemeService : Core.Abstractions.IThemeService
{
    public event EventHandler? ThemeChanged;

    private Core.Abstractions.ThemeInfo _currentTheme;

    public WindowsThemeService()
    {
        _currentTheme = new Core.Abstractions.ThemeInfo
        {
            Name = "System",
            Type = DetectSystemTheme(),
            ResourcePath = ""
        };
    }

    public Core.Abstractions.ThemeInfo GetCurrentTheme()
    {
        return _currentTheme;
    }

    public void SetTheme(Core.Abstractions.ThemeInfo theme)
    {
        _currentTheme = theme;
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    public IEnumerable<Core.Abstractions.ThemeInfo> GetAvailableThemes()
    {
        return new[]
        {
            new Core.Abstractions.ThemeInfo { Name = "Light", Type = Core.Abstractions.ThemeType.Light, ResourcePath = "" },
            new Core.Abstractions.ThemeInfo { Name = "Dark", Type = Core.Abstractions.ThemeType.Dark, ResourcePath = "" },
            new Core.Abstractions.ThemeInfo { Name = "System", Type = Core.Abstractions.ThemeType.System, ResourcePath = "" }
        };
    }

    public Core.Abstractions.ThemeType DetectSystemTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            if (value is int intValue)
            {
                return intValue == 0 ? Core.Abstractions.ThemeType.Dark : Core.Abstractions.ThemeType.Light;
            }
        }
        catch { }

        return Core.Abstractions.ThemeType.Light;
    }
}
