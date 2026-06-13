using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.UI;

namespace PCL_CE.Neo.App;

/// <summary>
/// PCL-CE.Neo 应用程序入口
/// </summary>
public static class Program
{
    private static IServiceProvider? _serviceProvider;
    private static ILogger? _logger;

    /// <summary>
    /// 应用程序主入口点
    /// </summary>
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            InitializeServices();
            RunApplication();
        }
        catch (Exception ex)
        {
            if (_logger != null)
            {
                _logger.LogCritical(ex, "应用程序启动失败");
            }
            throw;
        }
    }

    /// <summary>
    /// 初始化依赖注入容器和服务
    /// </summary>
    private static void InitializeServices()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
        });

        services.AddCoreServices();
        services.AddUIServices();

        RegisterPlatformServices(services);

        _serviceProvider = services.BuildServiceProvider();
        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger("PCL_CE.Neo.App.Program");
        _logger.LogInformation("PCL-CE.Neo 应用程序服务初始化完成");
    }

    /// <summary>
    /// 根据当前平台动态注册平台服务
    /// </summary>
    private static void RegisterPlatformServices(IServiceCollection services)
    {
        string platformName = GetPlatformName();
        string expectedNamespace = $"PCL_CE.Neo.Platform.{platformName}";
        string expectedTypeName = $"{expectedNamespace}.ServiceCollectionExtensions";
        string expectedMethodName = "Add" + platformName + "PlatformServices";

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var type = assembly.GetType(expectedTypeName);
                if (type != null)
                {
                    var method = type.GetMethod(
                        expectedMethodName,
                        BindingFlags.Static | BindingFlags.Public,
                        new[] { typeof(IServiceCollection) });

                    if (method != null)
                    {
                        method.Invoke(null, new object[] { services });
                        if (_logger != null)
                        {
                            _logger.LogInformation("已注册 {Platform} 平台服务", platformName);
                        }
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logger.LogWarning(ex, "从程序集 {Assembly} 加载平台服务时出错", assembly.GetName().Name);
                }
            }
        }

        if (_logger != null)
        {
            _logger.LogWarning("未找到平台特定的服务注册方法：{TypeName}.{MethodName}", expectedTypeName, expectedMethodName);
        }
    }

    /// <summary>
    /// 获取当前平台名称
    /// </summary>
    private static string GetPlatformName()
    {
        if (OperatingSystem.IsWindows())
            return "Windows";
        if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
            return "macOS";
        if (OperatingSystem.IsLinux())
            return "Linux";
        return "Linux";
    }

    /// <summary>
    /// 运行应用程序
    /// </summary>
    private static void RunApplication()
    {
        _logger?.LogInformation("应用程序启动");

        var platformService = _serviceProvider?.GetService<IPlatformService>();
        if (platformService != null)
        {
            _logger?.LogInformation(
                "当前平台: {Platform}, OS: {OS}",
                platformService.PlatformName,
                platformService.OSVersion);
        }

        _logger?.LogInformation("应用程序运行中...");
    }

    /// <summary>
    /// 获取服务容器
    /// </summary>
    public static IServiceProvider? ServiceProvider => _serviceProvider;
}
