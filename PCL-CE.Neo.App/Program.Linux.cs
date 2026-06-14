using System;
using Uno.WinUI.Runtime.Skia.X11;
using UINeoApp = PCL_CE.Neo.UI.App;

namespace PCL_CE.Neo.AppHost;

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var host = new X11ApplicationHost(() => new UINeoApp());
            host.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Application error: {ex}");
        }
    }
}
