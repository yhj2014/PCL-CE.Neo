namespace PCL_CE.Neo.UI.Services;

public class DialogService : Core.Abstractions.IDialogService
{
    private readonly List<string> _pickedFiles = new List<string>();

    public string? ShowOpenFileDialog(string filter, string? initialDirectory = null)
    {
#if WINDOWS || MACCATALYST || LINUX
        return PickOpenFile(filter, initialDirectory);
#else
        return PickOpenFile(filter, initialDirectory);
#endif
    }

    private string? PickOpenFile(string filter, string? initialDirectory)
    {
#if WINDOWS || MACCATALYST || LINUX
        try
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;

            var extensions = ParseFilter(filter);
            foreach (var ext in extensions)
            {
                picker.FileTypeFilter.Add(ext);
            }

            var file = picker.PickSingleFileAsync().GetAwaiter().GetResult();
            if (file != null)
            {
                return file.Path;
            }
            return null;
        }
        catch
        {
            return null;
        }
#else
        return null;
#endif
    }

    public string? ShowSaveFileDialog(string filter, string defaultFileName, string? initialDirectory = null)
    {
#if WINDOWS || MACCATALYST || LINUX
        return PickSaveFile(filter, defaultFileName, initialDirectory);
#else
        return PickSaveFile(filter, defaultFileName, initialDirectory);
#endif
    }

    private string? PickSaveFile(string filter, string defaultFileName, string? initialDirectory)
    {
#if WINDOWS || MACCATALYST || LINUX
        try
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            picker.SuggestedFileName = defaultFileName;

            var extensions = ParseFilter(filter);
            var firstExtension = extensions.FirstOrDefault() ?? ".txt";
            var filterName = extensions.Count > 1 ? "All Files" : "Files";
            picker.FileTypeChoices.Add(filterName, extensions);

            var file = picker.PickSaveFileAsync().GetAwaiter().GetResult();
            if (file != null)
            {
                return file.Path;
            }
            return null;
        }
        catch
        {
            return null;
        }
#else
        return null;
#endif
    }

    public string? ShowOpenFolderDialog(string? initialDirectory = null)
    {
#if WINDOWS || MACCATALYST || LINUX
        return PickFolder(initialDirectory);
#else
        return PickFolder(initialDirectory);
#endif
    }

    private string? PickFolder(string? initialDirectory)
    {
#if WINDOWS || MACCATALYST || LINUX
        try
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            folderPicker.FileTypeFilter.Add("*");

            var folder = folderPicker.PickSingleFolderAsync().GetAwaiter().GetResult();
            if (folder != null)
            {
                return folder.Path;
            }
            return null;
        }
        catch
        {
            return null;
        }
#else
        return null;
#endif
    }

    public Core.Abstractions.DialogResult ShowMessageBox(string message, string title, Core.Abstractions.DialogButtons buttons)
    {
        return Core.Abstractions.DialogResult.OK;
    }

    public bool ShowConfirmation(string message, string title)
    {
        return true;
    }

    private List<string> ParseFilter(string filter)
    {
        var extensions = new List<string>();
        if (string.IsNullOrEmpty(filter))
        {
            extensions.Add("*");
            return extensions;
        }

        var parts = filter.Split('|');
        for (int i = 1; i < parts.Length; i += 2)
        {
            var spec = parts[i];
            var exts = spec.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var ext in exts)
            {
                var trimmed = ext.Trim();
                if (!trimmed.StartsWith("*") && !trimmed.StartsWith("."))
                {
                    trimmed = "." + trimmed;
                }
                if (!extensions.Contains(trimmed))
                {
                    extensions.Add(trimmed);
                }
            }
        }

        if (extensions.Count == 0)
        {
            extensions.Add("*");
        }

        return extensions;
    }
}
