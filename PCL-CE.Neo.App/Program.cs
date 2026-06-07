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

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           PCL-CE.Neo - 跨平台 Minecraft 启动器              ║");
        Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  当前状态: 控制台测试版本 (Uno Platform UI 开发中)         ║");
        Console.WriteLine("║                                                           ║");
        Console.WriteLine("║  平台: " + Environment.OSVersion.Platform.ToString().PadRight(50) + "║");
        Console.WriteLine("║  架构: " + System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString().PadRight(50) + "║");
        Console.WriteLine("║  版本: v0.0.1-alpha (开发版)                                ║");
        Console.WriteLine("║                                                           ║");
        Console.WriteLine("║  ⚠️  说明:                                                  ║");
        Console.WriteLine("║  - 此为核心架构测试版本，图形界面 (GUI) 开发中              ║");
        Console.WriteLine("║  - 核心业务逻辑和平台抽象层已实现并可正常编译               ║");
        Console.WriteLine("║  - 完整的 Uno Platform UI 版本将在后续发布                 ║");
        Console.WriteLine("║                                                           ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("正在初始化核心服务...");

        try
        {
            var services = new ServiceCollection();
            services.AddCoreServices();
            services.AddUIServices();

#if WINDOWS
            services.AddWindowsPlatformServices();
            Console.WriteLine("✓ Windows 平台服务已加载");
#elif MACOS
            services.AddMacOSPlatformServices();
            Console.WriteLine("✓ macOS 平台服务已加载");
#elif LINUX
            services.AddLinuxPlatformServices();
            Console.WriteLine("✓ Linux 平台服务已加载");
#endif

            var serviceProvider = services.BuildServiceProvider();
            Console.WriteLine("✓ 依赖注入容器已初始化");
            
            Console.WriteLine();
            Console.WriteLine("✅ 核心架构验证成功！");
            Console.WriteLine();
            Console.WriteLine("按任意键退出...");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("❌ 初始化失败: " + ex.Message);
            Console.WriteLine("详细信息: " + ex);
            Console.WriteLine();
            Console.WriteLine("按任意键退出...");
        }
        
        Console.ReadKey();
    }
}
