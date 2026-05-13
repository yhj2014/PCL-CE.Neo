using System.Windows;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsThemeService : IThemeService
{
    private ThemeInfo _currentTheme;
    private readonly object _lock = new object();

    public event EventHandler? ThemeChanged;

    public WindowsThemeService()
    {
        // 初始化为系统主题
        var systemTheme = DetectSystemTheme();
        _currentTheme = new ThemeInfo
        {
            Name = systemTheme == ThemeType.Dark ? "Dark" : "Light",
            Type = systemTheme,
            ResourcePath = string.Empty
        };
    }

    public ThemeInfo GetCurrentTheme()
    {
        lock (_lock)
        {
            return new ThemeInfo
            {
                Name = _currentTheme.Name,
                Type = _currentTheme.Type,
                ResourcePath = _currentTheme.ResourcePath
            };
        }
    }

    public void SetTheme(ThemeInfo theme)
    {
        lock (_lock)
        {
            if (theme.Type != _currentTheme.Type || theme.Name != _currentTheme.Name)
            {
                _currentTheme = new ThemeInfo
                {
                    Name = theme.Name,
                    Type = theme.Type,
                    ResourcePath = theme.ResourcePath
                };
                OnThemeChanged();
            }
        }
    }

    public IEnumerable<ThemeInfo> GetAvailableThemes()
    {
        return new[]
        {
            new ThemeInfo { Name = "Light", Type = ThemeType.Light, ResourcePath = string.Empty },
            new ThemeInfo { Name = "Dark", Type = ThemeType.Dark, ResourcePath = string.Empty },
            new ThemeInfo { Name = "System", Type = ThemeType.System, ResourcePath = string.Empty }
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to detect system theme: {ex.Message}");
        }
        return ThemeType.Light;
    }

    protected virtual void OnThemeChanged()
    {
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }
}
