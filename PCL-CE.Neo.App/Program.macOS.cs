using System;
using Uno.UI.Runtime.Skia.MacOS;
using UINeoApp = PCL_CE.Neo.UI.App;

namespace PCL_CE.Neo.AppHost;

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var host = new MacSkiaHost(() => new UINeoApp());
            host.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Application error: {ex}");
        }
    }
}
