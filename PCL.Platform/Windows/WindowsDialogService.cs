using Microsoft.Win32;
using PCL.Core.Abstractions;
using System.Windows;

namespace PCL.Platform.Windows;

public class WindowsDialogService : IDialogService
{
    public string? ShowOpenFileDialog(string filter, string? initialDirectory = null)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter,
            InitialDirectory = initialDirectory
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ShowSaveFileDialog(string filter, string defaultFileName, string? initialDirectory = null)
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            FileName = defaultFileName,
            InitialDirectory = initialDirectory
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ShowOpenFolderDialog(string? initialDirectory = null)
    {
        var dialog = new OpenFolderDialog
        {
            InitialDirectory = initialDirectory
        };

        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    public DialogResult ShowMessageBox(string message, string title, DialogButtons buttons)
    {
        var messageBoxButton = buttons switch
        {
            DialogButtons.OK => MessageBoxButton.OK,
            DialogButtons.OKCancel => MessageBoxButton.OKCancel,
            DialogButtons.YesNo => MessageBoxButton.YesNo,
            DialogButtons.YesNoCancel => MessageBoxButton.YesNoCancel,
            _ => MessageBoxButton.OK
        };

        var result = MessageBox.Show(message, title, messageBoxButton, MessageBoxImage.Information);

        return result switch
        {
            MessageBoxResult.OK => DialogResult.OK,
            MessageBoxResult.Cancel => DialogResult.Cancel,
            MessageBoxResult.Yes => DialogResult.Yes,
            MessageBoxResult.No => DialogResult.No,
            _ => DialogResult.None
        };
    }

    public bool ShowConfirmation(string message, string title)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }
}
