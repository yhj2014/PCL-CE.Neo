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

        Console.WriteLine("PCL-CE.Neo 应用启动");
        Console.WriteLine("平台: " + Environment.OSVersion.Platform);
        Console.WriteLine("架构: " + System.Runtime.InteropServices.RuntimeInformation.OSArchitecture);
        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }
}
