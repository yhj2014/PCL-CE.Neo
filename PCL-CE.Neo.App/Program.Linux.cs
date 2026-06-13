using System;
using Uno.UI.Runtime.Skia;
using PCL_CE.Neo.UI;

namespace PCL_CE.Neo.App;

public static partial class AppHost
{
    public static void RunLinux(string[] args)
    {
        try
        {
            var host = new GtkHost(() => new App(), args);
            host.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Application error: {ex}");
        }
    }
}
