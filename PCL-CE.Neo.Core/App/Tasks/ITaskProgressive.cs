namespace PCL_CE.Neo.Core.App.Tasks;

public interface ITaskProgressive
{
    event ProgressChangedEventHandler ProgressChanged;
}

public delegate void ProgressChangedEventHandler(double progress);