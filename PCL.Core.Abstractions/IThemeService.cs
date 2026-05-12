namespace PCL.Core.Abstractions;

public interface IThemeService
{
    event EventHandler? ThemeChanged;

    ThemeInfo GetCurrentTheme();
    void SetTheme(ThemeInfo theme);
    IEnumerable<ThemeInfo> GetAvailableThemes();
    ThemeType DetectSystemTheme();
}

public class ThemeInfo
{
    public string Name { get; set; } = string.Empty;
    public ThemeType Type { get; set; }
    public string ResourcePath { get; set; } = string.Empty;
}

public enum ThemeType
{
    Light,
    Dark,
    System
}
