using System.Windows;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsThemeService : IThemeService
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
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            if (value is int intValue)
            {
                return intValue == 0 ? ThemeType.Dark : ThemeType.Light;
            }
        }
        catch
        {
        }
        return ThemeType.Light;
    }
}
