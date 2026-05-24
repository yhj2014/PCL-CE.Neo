namespace PCL_CE.Neo.Core.Abstractions;

public interface IStateAdapter
{
    string Identifier { get; set; }
    Dictionary<string, string> CustomVariables { get; set; }

    double WindowWidth { get; set; }
    double WindowHeight { get; set; }

    string SelectedInstance { get; set; }
    string SelectedFolder { get; set; }
    string Folders { get; set; }
    string JavaList { get; set; }

    int StartupCount { get; set; }
    int LaunchCount { get; set; }

    string DownloadFolder { get; set; }
}
