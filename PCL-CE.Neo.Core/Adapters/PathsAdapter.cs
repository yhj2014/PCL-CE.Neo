using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class PathsAdapter : IPathsAdapter
{
    private readonly ILogger<PathsAdapter> _logger;
    private readonly IApplicationAdapter _appAdapter;
    private string _data = null!;
    private string _sharedData = null!;
    private string _sharedLocalData = null!;
    private string _temp = null!;
    private string _oldSharedData = null!;

#if DEBUG
    private const string Name = "PCLCE_Debug";
    private const string OldName = ".PCLCEDebug";
#else
    private const string Name = "PCLCE";
    private const string OldName = ".PCLCE";
#endif

    public PathsAdapter(ILogger<PathsAdapter> logger, IApplicationAdapter appAdapter)
    {
        _logger = logger;
        _appAdapter = appAdapter;
        InitializePaths();
    }

    public string Data => _data;
    public string SharedData => _sharedData;
    public string SharedLocalData => _sharedLocalData;
    public string Temp => _temp;
    public string OldSharedData => _oldSharedData;

    public string GetSpecialPath(Environment.SpecialFolder folder, string relative)
    {
        var folderPath = Environment.GetFolderPath(folder);
        return Path.Combine(folderPath, relative);
    }

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(_data);
        Directory.CreateDirectory(_sharedData);
        Directory.CreateDirectory(_sharedLocalData);
        Directory.CreateDirectory(_temp);
        _logger.LogInformation("目录初始化完成");
    }

    private void InitializePaths()
    {
        _data = Path.Combine(_appAdapter.ExecutableDirectory, "PCL");
        _sharedData = GetSpecialPath(Environment.SpecialFolder.ApplicationData, Name);
        _sharedLocalData = GetSpecialPath(Environment.SpecialFolder.LocalApplicationData, Name);
        _temp = Path.Combine(Path.GetTempPath(), Name);
        _oldSharedData = GetSpecialPath(Environment.SpecialFolder.ApplicationData, OldName);

#if DEBUG
        var envPath = Environment.GetEnvironmentVariable("PCL_PATH");
        if (!string.IsNullOrEmpty(envPath)) _data = envPath;
        var envSharedPath = Environment.GetEnvironmentVariable("PCL_PATH_SHARED");
        if (!string.IsNullOrEmpty(envSharedPath)) _sharedData = envSharedPath;
        var envLocalPath = Environment.GetEnvironmentVariable("PCL_PATH_LOCAL");
        if (!string.IsNullOrEmpty(envLocalPath)) _sharedLocalData = envLocalPath;
        var envTempPath = Environment.GetEnvironmentVariable("PCL_PATH_TEMP");
        if (!string.IsNullOrEmpty(envTempPath)) _temp = envTempPath;
#endif

        _logger.LogDebug("路径初始化: Data={Data}, SharedData={SharedData}, Local={Local}, Temp={Temp}",
            _data, _sharedData, _sharedLocalData, _temp);
    }
}
