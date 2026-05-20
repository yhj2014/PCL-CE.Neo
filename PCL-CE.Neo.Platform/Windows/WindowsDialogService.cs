using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsDialogService : IDialogService
{
    public string? ShowOpenFileDialog(string filter, string? initialDirectory = null)
    {
        return null;
    }

    public string? ShowSaveFileDialog(string filter, string defaultFileName, string? initialDirectory = null)
    {
        return null;
    }

    public string? ShowOpenFolderDialog(string? initialDirectory = null)
    {
        return null;
    }

    public DialogResult ShowMessageBox(string message, string title, DialogButtons buttons)
    {
        Console.WriteLine($"[{title}] {message}");
        return DialogResult.OK;
    }

    public bool ShowConfirmation(string message, string title)
    {
        Console.WriteLine($"[{title}] {message}");
        return true;
    }
}
