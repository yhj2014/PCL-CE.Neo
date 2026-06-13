using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.macOS;

public class MacOSThemeService : IThemeService
{
    private readonly ILogger<MacOSThemeService> _logger;
    private ThemeInfo _currentTheme;
    private ThemeType _lastSystemTheme;

    public event EventHandler? ThemeChanged;

    public MacOSThemeService(ILogger<MacOSThemeService> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("Initializing macOS theme service");
            _lastSystemTheme = DetectSystemTheme();
            _currentTheme = new ThemeInfo
            {
                Name = "System",
                Type = ThemeType.System,
                ResourcePath = string.Empty
            };
            _logger.LogInformation("macOS theme service initialized, current system theme: {Theme}", _lastSystemTheme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing macOS theme service");
            _currentTheme = new ThemeInfo { Name = "Light", Type = ThemeType.Light, ResourcePath = string.Empty };
            _lastSystemTheme = ThemeType.Light;
        }
    }

    public ThemeInfo GetCurrentTheme()
    {
        try
        {
            _logger.LogDebug("Getting current theme, name: {Name}, type: {Type}", _currentTheme.Name, _currentTheme.Type);
            return _currentTheme;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current theme");
            return new ThemeInfo { Name = "Light", Type = ThemeType.Light, ResourcePath = string.Empty };
        }
    }

    public void SetTheme(ThemeInfo theme)
    {
        try
        {
            if (theme == null)
            {
                _logger.LogWarning("Attempted to set null theme, ignored");
                return;
            }
            _logger.LogDebug("Setting new theme: {Name}, type: {Type}", theme.Name, theme.Type);
            _currentTheme = theme;
            _logger.LogInformation("Theme changed to: {Name}", theme.Name);
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting theme");
        }
    }

    public IEnumerable<ThemeInfo> GetAvailableThemes()
    {
        try
        {
            _logger.LogDebug("Getting available themes list");
            var themes = new[]
            {
                new ThemeInfo { Name = "Light", Type = ThemeType.Light, ResourcePath = string.Empty },
                new ThemeInfo { Name = "Dark", Type = ThemeType.Dark, ResourcePath = string.Empty },
                new ThemeInfo { Name = "System", Type = ThemeType.System, ResourcePath = string.Empty }
            };
            _logger.LogInformation("Returning {Count} available themes", themes.Length);
            return themes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available themes list, returning default light theme");
            return new[] { new ThemeInfo { Name = "Light", Type = ThemeType.Light, ResourcePath = string.Empty } };
        }
    }

    public ThemeType DetectSystemTheme()
    {
        try
        {
            _logger.LogDebug("Detecting system theme using defaults read");

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "defaults",
                Arguments = "read -g AppleInterfaceStyle",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd().Trim();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit(3000);

                if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                {
                    var theme = output.Equals("Dark", StringComparison.OrdinalIgnoreCase)
                        ? ThemeType.Dark
                        : ThemeType.Light;
                    _logger.LogInformation("System theme detection result: {Theme}, output: {Output}", theme, output);
                    _lastSystemTheme = theme;
                    return theme;
                }
                else
                {
                    _logger.LogDebug("AppleInterfaceStyle not set (exit code: {Code}), defaulting to Light. Error: {Error}",
                        process.ExitCode, string.IsNullOrWhiteSpace(error) ? "(none)" : error.Trim());
                    _lastSystemTheme = ThemeType.Light;
                    return _lastSystemTheme;
                }
            }

            _logger.LogWarning("Failed to start defaults process, defaulting to Light theme");
            _lastSystemTheme = ThemeType.Light;
            return _lastSystemTheme;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting system theme, returning default Light theme");
            _lastSystemTheme = ThemeType.Light;
            return _lastSystemTheme;
        }
    }
}
