using PCL_CE.Neo.Core.TaskManager;
using Xunit;

namespace PCL_CE.Neo.Tests;

public class TaskManagerTests
{
    [Fact]
    public void TaskManager_AddsAndRemovesTasks()
    {
        var manager = new TaskManager();
        var task = new TestTask("test-task");
        
        manager.Add(task);
        
        Assert.Single(manager.Tasks);
        Assert.Contains(task, manager.Tasks);
        
        manager.Remove(task);
        Assert.Empty(manager.Tasks);
    }

    [Fact]
    public void TaskManager_FiresEvents()
    {
        var manager = new TaskManager();
        var task = new TestTask("test-task");
        
        var addedFired = false;
        var removedFired = false;
        
        manager.TaskAdded += t => addedFired = true;
        manager.TaskRemoved += t => removedFired = true;
        
        manager.Add(task);
        Assert.True(addedFired);
        
        manager.Remove(task);
        Assert.True(removedFired);
    }

    [Fact]
    public void TaskBase_CompletesSuccessfully()
    {
        var task = new TestTask("test-task");
        
        task.RunToCompletion();
        
        Assert.Equal(TaskState.Completed, task.State);
        Assert.Equal(1.0, task.Progress);
    }

    [Fact]
    public void TaskBase_FailsWithException()
    {
        var task = new FailingTestTask("test-task");
        
        task.RunToCompletion();
        
        Assert.Equal(TaskState.Failed, task.State);
        Assert.NotNull(task.Exception);
    }

    [Fact]
    public void TaskBase_CancelsOperation()
    {
        var task = new LongRunningTestTask("test-task");
        
        task.Start();
        task.Cancel();
        
        Assert.Equal(TaskState.Cancelled, task.State);
    }

    private class TestTask : TaskBase
    {
        public TestTask(string name)
        {
            Name = name;
            Id = Guid.NewGuid().ToString();
        }

        public override string Id { get; }
        public override string Name { get; }

        public void RunToCompletion()
        {
            SetProgress(0.5);
            SetProgress(1.0);
            Complete();
        }

        public override Task StartAsync()
        {
            SetProgress(1.0);
            Complete();
            return Task.CompletedTask;
        }
    }

    private class FailingTestTask : TaskBase
    {
        public FailingTestTask(string name)
        {
            Name = name;
            Id = Guid.NewGuid().ToString();
        }

        public override string Id { get; }
        public override string Name { get; }

        public void RunToCompletion()
        {
            Fail("Test failure", new Exception("Test exception"));
        }

        public override Task StartAsync()
        {
            Fail("Test failure", new Exception("Test exception"));
            return Task.CompletedTask;
        }
    }

    private class LongRunningTestTask : TaskBase
    {
        public LongRunningTestTask(string name)
        {
            Name = name;
            Id = Guid.NewGuid().ToString();
        }

        public override string Id { get; }
        public override string Name { get; }

        public override async Task StartAsync()
        {
            await Task.Delay(10000, CancellationToken);
        }
    }
}
