using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PCL_CE.Neo.UI.Navigation;
using System.Collections.ObjectModel;

namespace PCL_CE.Neo.UI.Pages;

public sealed partial class VersionSelectPage : Page
{
    public ObservableCollection<VersionItem> Versions { get; } = new();

    public VersionSelectPage()
    {
        InitializeComponent();
        VersionList.ItemsSource = Versions;
        LoadVersions();
    }

    private void LoadVersions()
    {
        Versions.Clear();

        Versions.Add(new VersionItem
        {
            Name = "Minecraft 1.20.4",
            Version = "Release",
            Description = "最新正式版，稳定可靠"
        });

        Versions.Add(new VersionItem
        {
            Name = "Minecraft 1.19.4",
            Version = "Release",
            Description = "较新版本，功能丰富"
        });

        Versions.Add(new VersionItem
        {
            Name = "Minecraft 1.18.2",
            Version = "Release",
            Description = "经典版本，兼容性好"
        });

        Versions.Add(new VersionItem
        {
            Name = "Minecraft 1.16.5",
            Version = "Old",
            Description = "老版本，稳定但功能较少"
        });

        Versions.Add(new VersionItem
        {
            Name = "Minecraft 1.12.2",
            Version = "Old",
            Description = "历史版本，Mod 兼容性最好"
        });
    }

    private void OnBackClick(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.GoBack();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Controls.MyTextBox searchBox)
        {
            FilterVersions(searchBox.Text);
        }
    }

    private void FilterVersions(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            LoadVersions();
            return;
        }

        var filtered = Versions.Where(v =>
            v.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
            v.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase)
        ).ToList();

        Versions.Clear();
        foreach (var version in filtered)
        {
            Versions.Add(version);
        }
    }

    private void OnVersionTapped(object sender, RoutedEventArgs e)
    {
        if (sender is Border border && border.DataContext is VersionItem version)
        {
            NavigationService.Instance.Navigate(typeof(DownloadPage), version);
        }
    }
}

public class VersionItem
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
