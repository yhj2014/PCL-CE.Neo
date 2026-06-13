using Microsoft.UI.Xaml;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.App;

/// <summary>
/// Uno Platform 主窗口
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly ILogger<MainWindow> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    public MainWindow()
    {
        try
        {
            InitializeComponent();

            var loggerFactory = Program.ServiceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            _logger = loggerFactory?.CreateLogger<MainWindow>() ??
                      LoggerFactory.Create(b => b.AddDebug()).CreateLogger<MainWindow>();

            _logger.LogInformation("主窗口创建完成");

            if (StatusText != null)
            {
                StatusText.Text = "✅ 核心架构已就绪";
            }
        }
        catch (Exception ex)
        {
            var fallbackLogger = LoggerFactory.Create(b => b.AddDebug()).CreateLogger<MainWindow>();
            fallbackLogger.LogError(ex, "主窗口初始化时发生错误");

            if (StatusText != null)
            {
                StatusText.Text = "❌ 初始化失败";
            }
            throw;
        }
    }
}
