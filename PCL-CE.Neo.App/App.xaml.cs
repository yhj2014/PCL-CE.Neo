using Microsoft.UI.Xaml;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.App;

/// <summary>
/// Uno Platform 应用程序主类
/// </summary>
public partial class App : Application
{
    private Window? _mainWindow;
    private readonly ILogger<App> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    public App()
    {
        try
        {
            InitializeComponent();

            var loggerFactory = Program.ServiceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            _logger = loggerFactory?.CreateLogger<App>() ??
                      LoggerFactory.Create(b => b.AddDebug()).CreateLogger<App>();

            _logger.LogInformation("PCL-CE.Neo 应用程序初始化完成");
        }
        catch (Exception ex)
        {
            var fallbackLogger = LoggerFactory.Create(b => b.AddDebug()).CreateLogger<App>();
            fallbackLogger.LogCritical(ex, "应用程序初始化过程中发生严重错误");
            throw;
        }
    }

    /// <summary>
    /// 应用程序启动时调用
    /// </summary>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            _logger.LogInformation("应用程序启动，创建主窗口");

            _mainWindow = new MainWindow();
            _mainWindow.Activate();

            _logger.LogInformation("主窗口已激活并显示");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "应用程序启动时发生错误");
            throw;
        }
    }
}
