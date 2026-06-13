using System;
using UINeoApp = PCL_CE.Neo.UI.App;

namespace PCL_CE.Neo.AppHost;

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var host = new Uno.UI.Runtime.Skia.Gtk.GtkHost(() => new UINeoApp());
            host.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Application error: {ex}");
        }
    }
}
