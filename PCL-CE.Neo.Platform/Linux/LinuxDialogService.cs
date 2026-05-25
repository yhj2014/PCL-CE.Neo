namespace PCL_CE.Neo.Platform.Linux;

public class LinuxDialogService : Core.Abstractions.IDialogService
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

    public Core.Abstractions.DialogResult ShowMessageBox(string message, string title, Core.Abstractions.DialogButtons buttons)
    {
        return Core.Abstractions.DialogResult.OK;
    }

    public bool ShowConfirmation(string message, string title)
    {
        return false;
    }
}
