namespace PCL_CE.Neo.UI.Services;

public class ThemeService : Core.Abstractions.IThemeService
{
    public Core.Abstractions.AppTheme GetSystemTheme()
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform theme detection
        return Core.Abstractions.AppTheme.Light;
#else
        throw new PlatformNotSupportedException("ThemeService requires Uno Platform");
#endif
    }

    public bool IsDarkMode()
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform theme detection
        return false;
#else
        throw new PlatformNotSupportedException("ThemeService requires Uno Platform");
#endif
    }

    public string GetAccentColor()
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform theme detection
        return "#0078D4";
#else
        throw new PlatformNotSupportedException("ThemeService requires Uno Platform");
#endif
    }
}
