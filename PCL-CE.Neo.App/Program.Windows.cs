using System;
using Microsoft.UI.Xaml;
using PCL_CE.Neo.UI;

namespace PCL_CE.Neo.App;

public static partial class AppHost
{
    [STAThread]
    public static void RunWindows(string[] args)
    {
        Application.Start(_ => new App());
    }
}
