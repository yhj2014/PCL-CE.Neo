using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PCL_CE.Neo.Core;
using PCL_CE.Neo.UI;

#if WINDOWS
using PCL_CE.Neo.Platform.Windows;
#elif MACOS
using PCL_CE.Neo.Platform.macOS;
#elif LINUX
using PCL_CE.Neo.Platform.Linux;
#endif

namespace PCL_CE.Neo.App;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitializeServices();
    }

    private void InitializeServices()
    {
        try
        {
            var services = new ServiceCollection();
            services.AddCoreServices();
            services.AddUIServices();

#if WINDOWS
            services.AddWindowsPlatformServices();
#elif MACOS
            services.AddMacOSPlatformServices();
#elif LINUX
            services.AddLinuxPlatformServices();
#endif
            
            var serviceProvider = services.BuildServiceProvider();
            StatusText.Text = "✅ 核心服务已初始化";
        }
        catch
        {
            StatusText.Text = "❌ 服务初始化失败";
        }
    }
}
