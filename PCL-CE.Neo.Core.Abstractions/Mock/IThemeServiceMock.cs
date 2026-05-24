namespace PCL_CE.Neo.Core.Abstractions.Mock;

public class ThemeServiceMock : IThemeService
{
    public event EventHandler? ThemeChanged;
    
    public ThemeInfo CurrentTheme { get; set; } = new ThemeInfo { Name = "Light", Type = ThemeType.Light };
    public List<ThemeInfo> AvailableThemes { get; set; } = new List<ThemeInfo>
    {
        new ThemeInfo { Name = "Light", Type = ThemeType.Light },
        new ThemeInfo { Name = "Dark", Type = ThemeType.Dark }
    };
    public ThemeType DetectedSystemTheme { get; set; } = ThemeType.Light;
    
    public ThemeInfo GetCurrentTheme()
    {
        return CurrentTheme;
    }

    public void SetTheme(ThemeInfo theme)
    {
        CurrentTheme = theme;
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    public IEnumerable<ThemeInfo> GetAvailableThemes()
    {
        return AvailableThemes;
    }

    public ThemeType DetectSystemTheme()
    {
        return DetectedSystemTheme;
    }
}
