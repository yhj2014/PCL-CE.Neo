using Xunit;
using FluentAssertions;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Platform.Windows;
using System.Diagnostics;

namespace PCL_CE.Neo.Tests.PlatformIntegration.Windows;

[Trait("Category", "Integration")]
[Trait("Platform", "Windows")]
public class WindowsJavaScannerIntegrationTests
{
    private readonly IJavaScanner _javaScanner;

    public WindowsJavaScannerIntegrationTests()
    {
        _javaScanner = new WindowsJavaScanner();
    }

    [Fact]
    public void ScanJavaPaths_ShouldReturnList()
    {
        // Act
        var javaPaths = _javaScanner.ScanJavaPaths().ToList();

        // Assert
        javaPaths.Should().NotBeNull();
        Debug.WriteLine($"Found Java Paths: {javaPaths.Count}");
        foreach (var path in javaPaths)
        {
            Debug.WriteLine($"  - {path}");
        }
    }

    [Fact]
    public void ScanDirectory_ShouldReturnValidJavaPaths()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "PCL_CE_Neo_Test_Java");
        Directory.CreateDirectory(tempDir);
        var jdkDir = Path.Combine(tempDir, "jdk-17");
        var testBinDir = Path.Combine(jdkDir, "bin");
        Directory.CreateDirectory(testBinDir);
        var testJavaExe = Path.Combine(testBinDir, "java.exe");
        File.Create(testJavaExe).Dispose();

        try
        {
            var javaPaths = _javaScanner.ScanDirectory(tempDir).ToList();

            javaPaths.Should().NotBeNull();
            javaPaths.Should().Contain(jdkDir);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void IsValidJavaPath_ShouldReturnTrueForValidJavaPath()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "PCL_CE_Neo_Test_Java_Valid");
        Directory.CreateDirectory(tempDir);
        var testBinDir = Path.Combine(tempDir, "bin");
        Directory.CreateDirectory(testBinDir);
        var testJavaExe = Path.Combine(testBinDir, "java.exe");
        File.Create(testJavaExe).Dispose();

        try
        {
            var isValid = _javaScanner.IsValidJavaPath(tempDir);

            isValid.Should().BeTrue();
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void IsValidJavaPath_ShouldReturnFalseForNonJavaPath()
    {
        // Arrange
        var invalidPath = Path.Combine(Path.GetTempPath(), "notjava.exe");

        // Act
        var isValid = _javaScanner.IsValidJavaPath(invalidPath);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValidJavaPath_ShouldReturnFalseForNonExistentPath()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent", "java.exe");

        // Act
        var isValid = _javaScanner.IsValidJavaPath(nonExistentPath);

        // Assert
        isValid.Should().BeFalse();
    }
}
