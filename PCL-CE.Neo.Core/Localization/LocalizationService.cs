using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Lifecycle;

namespace PCL_CE.Neo.Core.Localization;

/// <summary>
/// 本地化服务，负责管理 UI 语言和展示格式
/// </summary>
public class LocalizationService : IService, IAsyncDisposable
{
    private readonly ILogger<LocalizationService> _logger;

    /// <summary>
    /// 跟随系统设置的配置值
    /// </summary>
    public const string Auto = "auto";

    /// <summary>
    /// 展示格式同步 UI 语言的配置值
    /// </summary>
    public const string FormatCultureFollowLanguage = "ui-language";

    /// <summary>
    /// 默认语言，也是语言资源的完整兜底
    /// </summary>
    public const string DefaultLanguageCode = "zh-CN";

    private static readonly LocalizationLanguage _DefaultLanguage = new(
        DefaultLanguageCode,
        "简体中文（中国大陆）",
        "zh-CN",
        LocalizationFontProfile.SimplifiedChinese);

    private static CultureInfo _systemFormatCulture = CultureInfo.CurrentCulture;
    private static CultureInfo _systemUiCulture = CultureInfo.CurrentUICulture;

    /// <summary>
    /// 当前 UI 语言（静态访问）
    /// </summary>
    public static LocalizationLanguage StaticCurrentLanguage { get; private set; } = _DefaultLanguage;

    /// <summary>
    /// 当前 UI 展示格式所使用的区域性（静态访问）
    /// </summary>
    public static CultureInfo StaticCurrentFormatCulture { get; private set; } = CultureInfo.CurrentCulture;

    /// <summary>
    /// 受支持的 UI 语言列表
    /// </summary>
    public IReadOnlyList<LocalizationLanguage> SupportedLanguages { get; } =
    [
        _DefaultLanguage,
        new("zh-TW", "繁體中文（台灣）", "zh-TW", LocalizationFontProfile.TraditionalChinese),
        new("en-US", "English (US)", "en-US", LocalizationFontProfile.English),
        new("en-GB", "English (United Kingdom)", "en-GB", LocalizationFontProfile.English),
        new("ja-JP", "日本語（日本）", "ja-JP", LocalizationFontProfile.Japanese),
        new("fr-FR", "Français (France)", "fr-FR", LocalizationFontProfile.Other),
        new("es-ES", "Español (España)", "es-ES", LocalizationFontProfile.Other)
    ];

    /// <summary>
    /// 语言或展示格式更改后触发的事件
    /// </summary>
    public event Action? LanguageChanged;

    /// <summary>
    /// 本地化文本资源字典
    /// </summary>
    private static readonly Dictionary<string, Dictionary<string, string>> _languageResources = new();

    public string Identifier => "localization";
    public string Name => "本地化";
    public bool SupportAsync => true;

    public LocalizationService(ILogger<LocalizationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync()
    {
        _logger.LogInformation("本地化服务启动");

        _systemFormatCulture = CultureInfo.CurrentCulture;
        _systemUiCulture = CultureInfo.CurrentUICulture;

        // 加载语言资源
        await LoadLanguageResourcesAsync().ConfigureAwait(false);

        // 从配置应用语言
        ApplyFromConfig();

        _logger.LogInformation("当前 UI 语言: {Language}, 展示格式: {FormatCulture}",
            StaticCurrentLanguage.Code, StaticCurrentFormatCulture.Name);
    }

    public Task StopAsync()
    {
        _logger.LogInformation("本地化服务停止");
        return Task.CompletedTask;
    }

    /// <summary>
    /// 加载语言资源文件
    /// </summary>
    private async Task LoadLanguageResourcesAsync()
    {
        try
        {
            var resourcePath = GetResourcePath();
            if (!Directory.Exists(resourcePath))
            {
                _logger.LogWarning("语言资源目录不存在: {Path}", resourcePath);
                return;
            }

            foreach (var language in SupportedLanguages)
            {
                var languageFile = Path.Combine(resourcePath, $"{language.Code}.json");
                if (File.Exists(languageFile))
                {
                    try
                    {
                        var content = await File.ReadAllTextAsync(languageFile).ConfigureAwait(false);
                        var resources = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
                        if (resources != null)
                        {
                            _languageResources[language.Code] = resources;
                            _logger.LogDebug("已加载语言资源: {Language}, 条目数: {Count}",
                                language.Code, resources.Count);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "加载语言资源失败: {Language}", language.Code);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载语言资源时发生异常");
        }
    }

    /// <summary>
    /// 获取语言资源目录路径
    /// </summary>
    private static string GetResourcePath()
    {
        var basePath = AppContext.BaseDirectory;
        return Path.Combine(basePath, "Localization", "Languages");
    }

    /// <summary>
    /// 按当前配置应用 UI 语言与展示格式
    /// </summary>
    public void ApplyFromConfig()
    {
        Apply(Auto, Auto, false);
    }

    /// <summary>
    /// 应用 UI 语言与展示格式
    /// </summary>
    /// <param name="languageCode">UI 语言代码，auto 表示跟随系统语言</param>
    /// <param name="formatCultureCode">展示格式区域性，auto 表示跟随系统区域格式</param>
    /// <param name="save">是否写回配置</param>
    public void Apply(string languageCode, string formatCultureCode = Auto, bool save = true)
    {
        try
        {
            var normalizedLanguageCode = NormalizeConfigValue(languageCode);
            var language = ResolveLanguage(normalizedLanguageCode);
            var uiCulture = CultureInfo.GetCultureInfo(language.CultureName);
            var formatCulture = ResolveFormatCulture(formatCultureCode, uiCulture, out var normalizedFormatCultureCode);

            var isLanguageChanged = !string.Equals(StaticCurrentLanguage.Code, language.Code, StringComparison.OrdinalIgnoreCase);
            var isFormatCultureChanged = !string.Equals(StaticCurrentFormatCulture.Name, formatCulture.Name, StringComparison.OrdinalIgnoreCase);

            if (!isLanguageChanged && !isFormatCultureChanged)
            {
                return;
            }

            ApplyCultures(uiCulture, formatCulture);

            StaticCurrentLanguage = language;
            StaticCurrentFormatCulture = formatCulture;

            Lang.SyncCulture(formatCulture);

            _logger.LogInformation("当前 UI 语言: {Language}, 展示格式: {FormatCulture}",
                language.Code, formatCulture.Name);

            LanguageChanged?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "应用语言设置时发生异常");
        }
    }

    /// <summary>
    /// 判断语言代码是否受支持
    /// </summary>
    /// <param name="languageCode">语言代码</param>
    /// <returns>是否受支持</returns>
    public bool IsLanguageSupported(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode)) return true;
        if (string.Equals(languageCode, Auto, StringComparison.OrdinalIgnoreCase)) return true;
        var normalizedCode = NormalizeCultureCode(languageCode);
        return SupportedLanguages.Any(language =>
            string.Equals(language.Code, normalizedCode, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 按配置值解析语言
    /// </summary>
    /// <param name="languageCode">语言代码</param>
    /// <returns>解析后的语言</returns>
    public LocalizationLanguage ResolveLanguage(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode) ||
            string.Equals(languageCode, Auto, StringComparison.OrdinalIgnoreCase))
        {
            return ResolveSystemLanguage();
        }

        var normalizedCode = NormalizeCultureCode(languageCode);
        return SupportedLanguages.FirstOrDefault(language =>
                   string.Equals(language.Code, normalizedCode, StringComparison.OrdinalIgnoreCase))
               ?? _DefaultLanguage;
    }

    /// <summary>
    /// 获取本地化文本
    /// </summary>
    /// <param name="key">资源键</param>
    /// <returns>本地化文本，如果未找到则返回 null</returns>
    public static string? GetText(string key)
    {
        // 先尝试当前语言
        if (_languageResources.TryGetValue(StaticCurrentLanguage.Code, out var resources))
        {
            if (resources.TryGetValue(key, out var text))
            {
                return text;
            }
        }

        // 回退到默认语言
        if (_languageResources.TryGetValue(DefaultLanguageCode, out var defaultResources))
        {
            if (defaultResources.TryGetValue(key, out var text))
            {
                return text;
            }
        }

        return null;
    }

    private LocalizationLanguage ResolveSystemLanguage()
    {
        var systemLanguage = NormalizeCultureCode(_systemUiCulture.Name);
        var exact = SupportedLanguages.FirstOrDefault(language =>
            string.Equals(language.Code, systemLanguage, StringComparison.OrdinalIgnoreCase));
        if (exact is not null) return exact;

        var neutral = _systemUiCulture.TwoLetterISOLanguageName;
        return SupportedLanguages.FirstOrDefault(language =>
                   language.Code.StartsWith(neutral + "-", StringComparison.OrdinalIgnoreCase))
               ?? _DefaultLanguage;
    }

    private CultureInfo ResolveFormatCulture(string? formatCultureCode, CultureInfo uiCulture, out string normalizedCode)
    {
        if (string.IsNullOrWhiteSpace(formatCultureCode) ||
            string.Equals(formatCultureCode, Auto, StringComparison.OrdinalIgnoreCase))
        {
            normalizedCode = Auto;
            return _systemFormatCulture;
        }

        if (string.Equals(formatCultureCode, FormatCultureFollowLanguage, StringComparison.OrdinalIgnoreCase))
        {
            normalizedCode = FormatCultureFollowLanguage;
            return uiCulture;
        }

        try
        {
            var culture = CultureInfo.GetCultureInfo(formatCultureCode);
            normalizedCode = culture.Name;
            return culture;
        }
        catch (CultureNotFoundException)
        {
            _logger.LogWarning("无法识别展示格式区域性: {FormatCulture}, 已回退系统区域格式", formatCultureCode);
            normalizedCode = Auto;
            return _systemFormatCulture;
        }
    }

    private void ApplyCultures(CultureInfo uiCulture, CultureInfo formatCulture)
    {
        CultureInfo.CurrentUICulture = uiCulture;
        CultureInfo.DefaultThreadCurrentUICulture = uiCulture;
        Thread.CurrentThread.CurrentUICulture = uiCulture;

        CultureInfo.CurrentCulture = formatCulture;
        CultureInfo.DefaultThreadCurrentCulture = formatCulture;
        Thread.CurrentThread.CurrentCulture = formatCulture;
    }

    private static string NormalizeConfigValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? Auto : NormalizeCultureCode(value);
    }

    private static string NormalizeCultureCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? Auto : value.Replace('_', '-').Trim();
    }

    private bool _disposed;
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await StopAsync().ConfigureAwait(false);
    }
}