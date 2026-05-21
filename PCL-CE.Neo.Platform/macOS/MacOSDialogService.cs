namespace PCL_CE.Neo.Platform.macOS;

public class MacOSDialogService : Core.Abstractions.IDialogService
{
#if MACCATALYST
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
        return true;
    }
#else
    public string? ShowOpenFileDialog(string filter, string? initialDirectory = null)
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public string? ShowSaveFileDialog(string filter, string defaultFileName, string? initialDirectory = null)
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public string? ShowOpenFolderDialog(string? initialDirectory = null)
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public Core.Abstractions.DialogResult ShowMessageBox(string message, string title, Core.Abstractions.DialogButtons buttons)
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public bool ShowConfirmation(string message, string title)
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }
#endif
}
