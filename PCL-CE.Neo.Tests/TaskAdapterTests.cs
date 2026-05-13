using PCL_CE.Neo.Core.Abstractions;

namespace PCL.CE.Neo.Tests;

public class TaskAdapterTests
{
    [Fact]
    public async Task RunTaskAsync_CompletesSuccessfully()
    {
        var logger = new TestLogger<Adapters.TaskAdapter>();
        var taskAdapter = new Adapters.TaskAdapter(logger);

        var result = await taskAdapter.RunTaskAsync("Test Task", async progress =>
        {
            progress.Report(0.5);
            await Task.Delay(10);
            progress.Report(1.0);
        });

        Assert.Equal(Abstractions.TaskState.Completed, result.State);
        Assert.Equal(1.0, result.Progress);
    }

    [Fact]
    public async Task RunTaskAsync_ReturnsResult()
    {
        var logger = new TestLogger<Adapters.TaskAdapter>();
        var taskAdapter = new Adapters.TaskAdapter(logger);

        var result = await taskAdapter.RunTaskAsync<string>("String Task", async _ =>
        {
            await Task.Delay(10);
            return "Test Result";
        });

        Assert.Equal("Test Result", result);
    }

    [Fact]
    public void TaskInfo_ReportProgress()
    {
        var info = new TaskInfoImpl("test-id", "Test", new CancellationTokenSource());

        info.ReportProgress(0.5, "Half done");

        Assert.Equal(0.5, info.Progress);
        Assert.Equal("Half done", info.Status);
    }

    [Fact]
    public void TaskInfo_ProgressClamped()
    {
        var info = new TaskInfoImpl("test-id", "Test", new CancellationTokenSource());

        info.ReportProgress(1.5);

        Assert.Equal(1.0, info.Progress);
    }

    private class TaskInfoImpl : ITaskInfo
    {
        public TaskInfoImpl(string id, string name, CancellationTokenSource cts)
        {
            Id = id;
            Name = name;
            CancellationTokenSource = cts;
        }

        public string Id { get; }
        public string Name { get; }
        public CancellationTokenSource CancellationTokenSource { get; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public Exception? Error { get; set; }
        public Abstractions.TaskState State { get; set; }
        public double _progress;
        public double Progress
        {
            get => _progress;
            set => _progress = Math.Clamp(value, 0, 1);
        }
        public string? Status { get; set; }

        public void ReportProgress(double progress, string? status = null)
        {
            Progress = progress;
            Status = status;
        }

        public void Cancel() => CancellationTokenSource.Cancel();
    }
}
