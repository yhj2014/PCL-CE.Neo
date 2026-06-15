using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.UI.Themes;

public interface IThemeManager
{
    ThemeType CurrentTheme { get; }
    event EventHandler<ThemeType>? ThemeChanged;
    void SetTheme(ThemeType theme);
    void ApplySystemTheme();
    string GetResourcePath(ThemeType theme);
}

public class ThemeManager : IThemeManager
{
    private readonly ILogger<ThemeManager> _logger;
    private readonly IThemeService _themeService;
    private ThemeType _currentTheme;

    public ThemeType CurrentTheme => _currentTheme;

    public event EventHandler<ThemeType>? ThemeChanged;

    public ThemeManager(ILogger<ThemeManager> logger, IThemeService themeService)
    {
        _logger = logger;
        _themeService = themeService;
        _themeService.ThemeChanged += OnSystemThemeChanged;
        _currentTheme = _themeService.GetCurrentTheme().Type;
        _logger.LogInformation("主题管理器已初始化，当前主题: {Theme}", _currentTheme);
    }

    private void OnSystemThemeChanged(object? sender, EventArgs e)
    {
        var systemTheme = _themeService.GetCurrentTheme();
        if (systemTheme.Type != _currentTheme)
        {
            _currentTheme = systemTheme.Type;
            _logger.LogInformation("系统主题已更改为: {Theme}", _currentTheme);
            ThemeChanged?.Invoke(this, _currentTheme);
        }
    }

    public void SetTheme(ThemeType theme)
    {
        if (theme != _currentTheme)
        {
            _currentTheme = theme;
            _logger.LogInformation("主题已更改为: {Theme}", _currentTheme);
            ThemeChanged?.Invoke(this, _currentTheme);
        }
    }

    public void ApplySystemTheme()
    {
        var systemTheme = _themeService.GetCurrentTheme();
        SetTheme(systemTheme.Type);
    }

    public string GetResourcePath(ThemeType theme)
    {
        return theme switch
        {
            ThemeType.Dark => "ms-appx:///Resources/Styles/Dark.xaml",
            ThemeType.Light => "ms-appx:///Resources/Styles/Light.xaml",
            _ => "ms-appx:///Resources/Styles/System.xaml"
        };
    }
}