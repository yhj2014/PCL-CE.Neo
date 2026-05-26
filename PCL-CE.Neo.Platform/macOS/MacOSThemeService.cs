namespace PCL_CE.Neo.Platform.macOS;

public class MacOSThemeService : Core.Abstractions.IThemeService
{
    public event EventHandler? ThemeChanged;

    private Core.Abstractions.ThemeInfo _currentTheme;

    public MacOSThemeService()
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
            var result = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "defaults",
                Arguments = "read -g AppleInterfaceStyle",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (result != null)
            {
                result.WaitForExit();
                var output = result.StandardOutput.ReadToEnd().Trim();
                return output == "Dark" ? Core.Abstractions.ThemeType.Dark : Core.Abstractions.ThemeType.Light;
            }
        }
        catch { }

        return Core.Abstractions.ThemeType.Light;
    }
}
