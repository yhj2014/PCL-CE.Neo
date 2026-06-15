using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.UI.ViewModels;

public partial class InstanceViewModel : ViewModelBase
{
    private readonly IInstanceAdapter _instanceAdapter;

    [ObservableProperty]
    private string? _newInstanceName;

    [ObservableProperty]
    private string? _selectedVersion;

    [ObservableProperty]
    private GameInstance? _selectedInstance;

    public List<GameInstance> Instances { get; } = new();
    public List<string> AvailableVersions { get; } = new();

    public IAsyncRelayCommand LoadInstancesCommand { get; }
    public IAsyncRelayCommand CreateInstanceCommand { get; }
    public IAsyncRelayCommand DeleteInstanceCommand { get; }

    public InstanceViewModel(
        ILogger<InstanceViewModel> logger,
        IInstanceAdapter instanceAdapter)
        : base(logger)
    {
        _instanceAdapter = instanceAdapter;
        LoadInstancesCommand = new AsyncRelayCommand(LoadInstancesAsync);
        CreateInstanceCommand = new AsyncRelayCommand(CreateInstanceAsync);
        DeleteInstanceCommand = new AsyncRelayCommand(DeleteInstanceAsync);
        InitializeVersions();
    }

    private void InitializeVersions()
    {
        AvailableVersions.Add("1.20.1");
        AvailableVersions.Add("1.19.4");
        AvailableVersions.Add("1.18.2");
        AvailableVersions.Add("1.17.1");
        AvailableVersions.Add("1.16.5");
        if (AvailableVersions.Any())
        {
            SelectedVersion = AvailableVersions.First();
        }
    }

    public async Task LoadInstancesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var instances = await _instanceAdapter.GetAllInstancesAsync();
            Instances.Clear();
            Instances.AddRange(instances);
            Logger.LogInformation("已加载 {Count} 个实例", Instances.Count);
        }, "加载实例列表");
    }

    private async Task CreateInstanceAsync()
    {
        if (string.IsNullOrWhiteSpace(NewInstanceName))
        {
            ErrorMessage = "请输入实例名称";
            return;
        }

        if (string.IsNullOrEmpty(SelectedVersion))
        {
            ErrorMessage = "请选择版本";
            return;
        }

        await ExecuteAsync(async () =>
        {
            var options = new CreateInstanceOptions
            {
                Name = NewInstanceName,
                MinecraftVersion = SelectedVersion
            };

            await _instanceAdapter.CreateInstanceAsync(options);
            Logger.LogInformation("已创建实例: {Name}", NewInstanceName);
            NewInstanceName = string.Empty;
            await LoadInstancesAsync();
        }, "创建实例");
    }

    private async Task DeleteInstanceAsync()
    {
        if (SelectedInstance == null)
        {
            ErrorMessage = "请选择要删除的实例";
            return;
        }

        await ExecuteAsync(async () =>
        {
            await _instanceAdapter.DeleteInstanceAsync(SelectedInstance.Id);
            Logger.LogInformation("已删除实例: {Name}", SelectedInstance.Name);
            await LoadInstancesAsync();
            SelectedInstance = null;
        }, "删除实例");
    }
}