using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsThemeService : IThemeService
{
    private readonly ILogger<WindowsThemeService> _logger;
    private ThemeInfo _currentTheme;
    private ThemeType _lastSystemTheme;

    public event EventHandler? ThemeChanged;

    public WindowsThemeService() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<WindowsThemeService>.Instance)
    {
    }

    public WindowsThemeService(ILogger<WindowsThemeService> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("正在初始化 Windows 主题服务");
            _lastSystemTheme = DetectSystemTheme();
            _currentTheme = new ThemeInfo
            {
                Name = "System",
                Type = ThemeType.System,
                ResourcePath = string.Empty
            };
            _logger.LogInformation("Windows 主题服务初始化完成，当前系统主题: {Theme}", _lastSystemTheme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化 Windows 主题服务时发生错误");
            _currentTheme = new ThemeInfo { Name = "Light", Type = ThemeType.Light, ResourcePath = string.Empty };
            _lastSystemTheme = ThemeType.Light;
        }
    }

    public ThemeInfo GetCurrentTheme()
    {
        try
        {
            _logger.LogDebug("获取当前主题，名称: {Name}, 类型: {Type}", _currentTheme.Name, _currentTheme.Type);
            return _currentTheme;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取当前主题时发生错误");
            return new ThemeInfo { Name = "Light", Type = ThemeType.Light, ResourcePath = string.Empty };
        }
    }

    public void SetTheme(ThemeInfo theme)
    {
        try
        {
            if (theme == null)
            {
                _logger.LogWarning("尝试设置空主题，已忽略");
                return;
            }
            _logger.LogDebug("设置新主题: {Name}, 类型: {Type}", theme.Name, theme.Type);
            _currentTheme = theme;
            _logger.LogInformation("主题已变更为: {Name}", theme.Name);
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置主题时发生错误");
        }
    }

    public IEnumerable<ThemeInfo> GetAvailableThemes()
    {
        try
        {
            _logger.LogDebug("获取可用主题列表");
            var themes = new[]
            {
                new ThemeInfo { Name = "Light", Type = ThemeType.Light, ResourcePath = string.Empty },
                new ThemeInfo { Name = "Dark", Type = ThemeType.Dark, ResourcePath = string.Empty },
                new ThemeInfo { Name = "System", Type = ThemeType.System, ResourcePath = string.Empty }
            };
            _logger.LogInformation("返回 {Count} 个可用主题", themes.Length);
            return themes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取可用主题列表时发生错误，返回默认浅色主题");
            return new[] { new ThemeInfo { Name = "Light", Type = ThemeType.Light, ResourcePath = string.Empty } };
        }
    }

    public ThemeType DetectSystemTheme()
    {
        try
        {
            _logger.LogDebug("检测系统主题，读取注册表");
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key == null)
            {
                _logger.LogWarning("无法打开注册表主题键，默认返回浅色主题");
                _lastSystemTheme = ThemeType.Light;
                return _lastSystemTheme;
            }

            var value = key.GetValue("AppsUseLightTheme");
            if (value is int intValue)
            {
                var theme = intValue == 0 ? ThemeType.Dark : ThemeType.Light;
                _logger.LogInformation("系统主题检测结果: {Theme}, 注册表值: {Value}", theme, intValue);
                _lastSystemTheme = theme;
                return theme;
            }

            _logger.LogWarning("注册表值类型不正确或不存在，默认返回浅色主题");
            _lastSystemTheme = ThemeType.Light;
            return _lastSystemTheme;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检测系统主题时发生错误，返回默认浅色主题");
            _lastSystemTheme = ThemeType.Light;
            return _lastSystemTheme;
        }
    }
}
