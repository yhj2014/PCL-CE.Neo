namespace PCL_CE.Neo.UI.Services;

public class DialogService : Core.Abstractions.IDialogService
{
    public string? ShowOpenFileDialog(string filter, string? initialDirectory = null)
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform file picker
        return null;
#else
        throw new PlatformNotSupportedException("DialogService requires Uno Platform");
#endif
    }

    public string? ShowSaveFileDialog(string filter, string defaultFileName, string? initialDirectory = null)
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform file picker
        return null;
#else
        throw new PlatformNotSupportedException("DialogService requires Uno Platform");
#endif
    }

    public string? ShowOpenFolderDialog(string? initialDirectory = null)
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform folder picker
        return null;
#else
        throw new PlatformNotSupportedException("DialogService requires Uno Platform");
#endif
    }

    public Core.Abstractions.DialogResult ShowMessageBox(string message, string title, Core.Abstractions.DialogButtons buttons)
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform content dialog
        return Core.Abstractions.DialogResult.OK;
#else
        throw new PlatformNotSupportedException("DialogService requires Uno Platform");
#endif
    }

    public bool ShowConfirmation(string message, string title)
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform content dialog
        return true;
#else
        throw new PlatformNotSupportedException("DialogService requires Uno Platform");
#endif
    }
}
