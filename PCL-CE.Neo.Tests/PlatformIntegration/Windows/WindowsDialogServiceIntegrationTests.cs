using Xunit;
using FluentAssertions;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Platform.Windows;
using System.Diagnostics;

namespace PCL_CE.Neo.Tests.PlatformIntegration.Windows;

[Trait("Category", "Integration")]
[Trait("Platform", "Windows")]
public class WindowsDialogServiceIntegrationTests
{
    private readonly IDialogService _dialogService;

    public WindowsDialogServiceIntegrationTests()
    {
        _dialogService = new WindowsDialogService();
    }

    [Fact]
    public void ShowMessageBox_ShouldNotThrow()
    {
        // 注意：在非UI环境中，对话框可能无法正常工作，但我们只测试不抛异常
        // Act & Assert
        Action act = () => _dialogService.ShowMessageBox("Test message", "Test Title", DialogButtons.OK);
        act.Should().NotThrow();
    }

    [Fact]
    public void ShowConfirmation_ShouldNotThrow()
    {
        // 注意：在非UI环境中，对话框可能无法正常工作，但我们只测试不抛异常
        // Act & Assert
        Action act = () => _dialogService.ShowConfirmation("Are you sure?", "Confirm");
        act.Should().NotThrow();
    }

    [Fact]
    public void ShowOpenFileDialog_ShouldNotThrow()
    {
        // 注意：在非UI环境中，对话框可能无法正常工作，但我们只测试不抛异常
        // Act & Assert
        Action act = () => _dialogService.ShowOpenFileDialog("All files|*.*");
        act.Should().NotThrow();
    }

    [Fact]
    public void ShowSaveFileDialog_ShouldNotThrow()
    {
        // 注意：在非UI环境中，对话框可能无法正常工作，但我们只测试不抛异常
        // Act & Assert
        Action act = () => _dialogService.ShowSaveFileDialog("All files|*.*", "test.txt");
        act.Should().NotThrow();
    }

    [Fact]
    public void ShowOpenFolderDialog_ShouldNotThrow()
    {
        // 注意：在非UI环境中，对话框可能无法正常工作，但我们只测试不抛异常
        // Act & Assert
        Action act = () => _dialogService.ShowOpenFolderDialog();
        act.Should().NotThrow();
    }
}
