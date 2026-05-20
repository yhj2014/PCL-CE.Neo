namespace PCL_CE.Neo.Core.Abstractions.Mock;

public class DialogServiceMock : IDialogService
{
    public string? LastMessage { get; private set; }
    public string? LastTitle { get; private set; }
    public DialogButtons? LastButtons { get; private set; }
    public DialogResult DefaultResult { get; set; } = DialogResult.OK;
    public string? DefaultFilePath { get; set; }
    
    public event Func<string, string, DialogButtons, DialogResult>? OnShowDialog;
    
    public DialogResult ShowMessageBox(string message, string title, DialogButtons buttons)
    {
        LastMessage = message;
        LastTitle = title;
        LastButtons = buttons;
        
        if (OnShowDialog != null)
        {
            return OnShowDialog(message, title, buttons);
        }
        return DefaultResult;
    }

    public bool ShowConfirmation(string message, string title)
    {
        LastMessage = message;
        LastTitle = title;
        LastButtons = DialogButtons.YesNo;
        return DefaultResult == DialogResult.Yes || DefaultResult == DialogResult.OK;
    }

    public string? ShowOpenFileDialog(string filter, string? initialDirectory = null)
    {
        return DefaultFilePath;
    }

    public string? ShowSaveFileDialog(string filter, string defaultFileName, string? initialDirectory = null)
    {
        return DefaultFilePath ?? defaultFileName;
    }

    public string? ShowOpenFolderDialog(string? initialDirectory = null)
    {
        return DefaultFilePath;
    }
}
