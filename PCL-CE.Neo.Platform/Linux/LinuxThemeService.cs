using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Linux;

public class LinuxThemeService : IThemeService
{
    private readonly ILogger<LinuxThemeService> _logger;
    private ThemeInfo _currentTheme;

    public event EventHandler? ThemeChanged;

    public LinuxThemeService() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<LinuxThemeService>.Instance)
    {
    }

    public LinuxThemeService(ILogger<LinuxThemeService> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("LinuxThemeService initializing");
            _currentTheme = new ThemeInfo
            {
                Name = "System",
                Type = DetectSystemTheme(),
                ResourcePath = ""
            };
            _logger.LogInformation("Initial system theme detected: {Type}", _currentTheme.Type);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during LinuxThemeService initialization, using default theme");
            _currentTheme = new ThemeInfo { Name = "Light", Type = ThemeType.Light, ResourcePath = "" };
        }
    }

    public ThemeInfo GetCurrentTheme()
    {
        try
        {
            return _currentTheme;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current theme");
            return new ThemeInfo { Name = "Light", Type = ThemeType.Light, ResourcePath = "" };
        }
    }

    public void SetTheme(ThemeInfo theme)
    {
        try
        {
            if (theme == null)
            {
                _logger.LogWarning("SetTheme called with null theme");
                return;
            }

            _logger.LogDebug("Setting theme: {Name} ({Type})", theme.Name, theme.Type);
            _currentTheme = theme;

            try
            {
                var scheme = theme.Type switch
                {
                    ThemeType.Dark => "prefer-dark",
                    ThemeType.Light => "prefer-light",
                    _ => "default"
                };

                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "gsettings",
                    Arguments = $"set org.gnome.desktop.interface color-scheme \"{scheme}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                });

                if (process != null)
                {
                    process.WaitForExit(3000);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "gsettings not available or failed");
            }

            ThemeChanged?.Invoke(this, EventArgs.Empty);
            _logger.LogInformation("Theme changed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set theme");
        }
    }

    public IEnumerable<ThemeInfo> GetAvailableThemes()
    {
        return new[]
        {
            new ThemeInfo { Name = "Light", Type = ThemeType.Light, ResourcePath = "" },
            new ThemeInfo { Name = "Dark", Type = ThemeType.Dark, ResourcePath = "" },
            new ThemeInfo { Name = "System", Type = ThemeType.System, ResourcePath = "" }
        };
    }

    public ThemeType DetectSystemTheme()
    {
        try
        {
            var gtkTheme = Environment.GetEnvironmentVariable("GTK_THEME");
            if (!string.IsNullOrWhiteSpace(gtkTheme))
            {
                if (gtkTheme.Contains("dark", StringComparison.OrdinalIgnoreCase) ||
                    gtkTheme.Contains("Dark", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogDebug("Detected dark theme from GTK_THEME");
                    return ThemeType.Dark;
                }
                if (gtkTheme.Contains("light", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogDebug("Detected light theme from GTK_THEME");
                    return ThemeType.Light;
                }
            }

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "gsettings",
                Arguments = "get org.gnome.desktop.interface color-scheme",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(3000);

                if (!string.IsNullOrWhiteSpace(output))
                {
                    _logger.LogDebug("color-scheme: {Output}", output);

                    if (output.Contains("dark", StringComparison.OrdinalIgnoreCase)
                        || output.Contains("prefer-dark", StringComparison.OrdinalIgnoreCase))
                    {
                        return ThemeType.Dark;
                    }
                    if (output.Contains("light", StringComparison.OrdinalIgnoreCase)
                        || output.Contains("prefer-light", StringComparison.OrdinalIgnoreCase))
                    {
                        return ThemeType.Light;
                    }
                }
            }

            using var process2 = Process.Start(new ProcessStartInfo
            {
                FileName = "gsettings",
                Arguments = "get org.gnome.desktop.interface gtk-theme",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (process2 != null)
            {
                var output2 = process2.StandardOutput.ReadToEnd().Trim();
                process2.WaitForExit(3000);

                if (!string.IsNullOrWhiteSpace(output2)
                    && output2.Contains("dark", StringComparison.OrdinalIgnoreCase))
                {
                    return ThemeType.Dark;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to detect system theme via gsettings");
        }

        return ThemeType.Light;
    }
}
