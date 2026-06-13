using System;

namespace PCL_CE.Neo.App;

public static partial class AppHost
{
    public static void Main(string[] args)
    {
#if LINUX
        RunLinux(args);
#elif MACOS
        RunMacOS(args);
#elif WINDOWS
        RunWindows(args);
#else
        RunWindows(args);
#endif
    }
}
