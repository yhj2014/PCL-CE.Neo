using System;
using Microsoft.UI.Xaml;
using UINeoApp = PCL_CE.Neo.UI.App;

namespace PCL_CE.Neo.AppHost;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Application.Start(_ => new UINeoApp());
    }
}
