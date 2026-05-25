using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;

namespace PCL_CE.Neo.UI.Themes;

public enum AppTheme
{
    Light,
    Dark,
    System
}

public partial class ThemeManager : ObservableObject
{
    private const string ThemeSettingKey = "AppTheme";
    private static ThemeManager? _instance;

    public static ThemeManager Instance => _instance ??= new ThemeManager();

    [ObservableProperty]
    private AppTheme _currentTheme = AppTheme.Light;

    private ThemeManager()
    {
    }

    public void Initialize()
    {
        LoadTheme();
    }

    public void SetTheme(AppTheme theme)
    {
        CurrentTheme = theme;
        ApplyTheme(theme);
        SaveTheme(theme);
    }

    private void ApplyTheme(AppTheme theme)
    {
        var resources = Application.Current.Resources;
        var mergedDictionaries = resources.MergedDictionaries;

        foreach (var dict in mergedDictionaries.ToList())
        {
            if (dict.Source?.OriginalString?.Contains("Colors.xaml") == true ||
                dict.Source?.OriginalString?.Contains("DarkColors.xaml") == true)
            {
                mergedDictionaries.Remove(dict);
            }
        }

        var colorResource = theme == AppTheme.Dark
            ? new ResourceDictionary { Source = new Uri("ms-appx:///Resources/DarkColors.xaml") }
            : new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Colors.xaml") };

        mergedDictionaries.Insert(0, colorResource);
    }

    private void LoadTheme()
    {
        var savedTheme = LoadSetting(ThemeSettingKey);
        if (Enum.TryParse<AppTheme>(savedTheme, out var theme))
        {
            CurrentTheme = theme;
            ApplyTheme(theme);
        }
    }

    private void SaveTheme(AppTheme theme)
    {
        SaveSetting(ThemeSettingKey, theme.ToString());
    }

    private string? LoadSetting(string key)
    {
        try
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            return localSettings.Values[key] as string;
        }
        catch
        {
            return null;
        }
    }

    private void SaveSetting(string key, string value)
    {
        try
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[key] = value;
        }
        catch
        {
        }
    }
}
