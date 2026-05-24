using Xunit;
using FluentAssertions;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Platform.Windows;
using System.Diagnostics;

namespace PCL_CE.Neo.Tests.PlatformIntegration.Windows;

[Trait("Category", "Integration")]
[Trait("Platform", "Windows")]
public class WindowsNotificationServiceIntegrationTests
{
    private readonly INotificationService _notificationService;

    public WindowsNotificationServiceIntegrationTests()
    {
        _notificationService = new WindowsNotificationService();
    }

    [Fact]
    public void ShowNotification_ShouldNotThrow()
    {
        // Arrange
        var notification = new NotificationInfo
        {
            Title = "Test Notification",
            Message = "This is a test notification message",
            Type = NotificationType.Info
        };

        // Act & Assert
        Action act = () => _notificationService.ShowNotification(notification);
        act.Should().NotThrow();
    }

    [Fact]
    public void ShowUpdateNotification_ShouldNotThrow()
    {
        // Act & Assert
        Action act = () => _notificationService.ShowUpdateNotification("1.0.0", "Test update notes");
        act.Should().NotThrow();
    }

    [Fact]
    public void ShowDownloadCompleteNotification_ShouldNotThrow()
    {
        // Act & Assert
        Action act = () => _notificationService.ShowDownloadCompleteNotification("testfile.txt");
        act.Should().NotThrow();
    }

    [Fact]
    public void ClearAllNotifications_ShouldNotThrow()
    {
        // Act & Assert
        Action act = () => _notificationService.ClearAllNotifications();
        act.Should().NotThrow();
    }
}
