namespace PCL.Core.Abstractions;

public interface IDialogService
{
    string? ShowOpenFileDialog(string filter, string? initialDirectory = null);
    string? ShowSaveFileDialog(string filter, string defaultFileName, string? initialDirectory = null);
    string? ShowOpenFolderDialog(string? initialDirectory = null);

    DialogResult ShowMessageBox(string message, string title, DialogButtons buttons);
    bool ShowConfirmation(string message, string title);
}

public enum DialogResult
{
    OK,
    Cancel,
    Yes,
    No,
    None
}

public enum DialogButtons
{
    OK,
    OKCancel,
    YesNo,
    YesNoCancel
}
