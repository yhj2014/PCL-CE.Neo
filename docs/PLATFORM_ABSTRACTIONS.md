# 平台抽象接口规范

## 概述

本文档定义了 PCL-CE.Neo 的平台抽象接口，所有跨平台功能都通过这些接口实现。

## 接口列表

### 1. IPlatformService

平台信息和基本操作接口。

```csharp
public interface IPlatformService
{
    PlatformType CurrentPlatform { get; }
    PlatformVersion PlatformVersion { get; }
    string PlatformName { get; }
    bool Is64BitProcess { get; }
    string GetResourcePath(string relativePath);
}
```

**实现文件：**
- [IPlatformService.cs](file:///workspace/PCL-CE.Neo.Core.Abstractions/IPlatformService.cs)
- [WindowsPlatformService.cs](file:///workspace/PCL-CE.Neo.Platform/Windows/WindowsPlatformService.cs)
- [MacOSPlatformService.cs](file:///workspace/PCL-CE.Neo.Platform/macOS/MacOSPlatformService.cs)
- [LinuxPlatformService.cs](file:///workspace/PCL-CE.Neo.Platform/Linux/LinuxPlatformService.cs)

### 2. IWindowService

窗口管理接口。

```csharp
public interface IWindowService
{
    void Minimize();
    void Maximize();
    void Restore();
    void Close();
    WindowState CurrentState { get; }
    void SetTitle(string title);
    void SetSize(int width, int height);
}
```

**实现文件：**
- [IWindowService.cs](file:///workspace/PCL-CE.Neo.Core.Abstractions/IWindowService.cs)
- [WindowsWindowService.cs](file:///workspace/PCL-CE.Neo.Platform/Windows/WindowsWindowService.cs)
- [MacOSWindowService.cs](file:///workspace/PCL-CE.Neo.Platform/macOS/MacOSWindowService.cs)
- [LinuxWindowService.cs](file:///workspace/PCL-CE.Neo.Platform/Linux/LinuxWindowService.cs)

### 3. IJavaScanner

Java 环境检测接口。

```csharp
public interface IJavaScanner
{
    Task<IReadOnlyList<JavaInstallation>> ScanJavaAsync();
    string? GetJavaPathFromRegistry();
    string? GetJavaHome();
    Task<JavaInstallation?> DetectJavaAsync();
}
```

**实现文件：**
- [IJavaScanner.cs](file:///workspace/PCL-CE.Neo.Core.Abstractions/IJavaScanner.cs)
- [WindowsJavaScanner.cs](file:///workspace/PCL-CE.Neo.Platform/Windows/WindowsJavaScanner.cs)
- [MacOSJavaScanner.cs](file:///workspace/PCL-CE.Neo.Platform/macOS/MacOSJavaScanner.cs)
- [LinuxJavaScanner.cs](file:///workspace/PCL-CE.Neo.Platform/Linux/LinuxJavaScanner.cs)

### 4. IAudioService

音频播放接口。

```csharp
public interface IAudioService
{
    Task PlaySoundAsync(string soundFile, float volume = 1.0f);
    Task PlayNotificationAsync();
    void SetMute(bool muted);
    bool IsMuted { get; }
}
```

**实现文件：**
- [IAudioService.cs](file:///workspace/PCL-CE.Neo.Core.Abstractions/IAudioService.cs)
- [WindowsAudioService.cs](file:///workspace/PCL-CE.Neo.Platform/Windows/WindowsAudioService.cs)
- [MacOSAudioService.cs](file:///workspace/PCL-CE.Neo.Platform/macOS/MacOSAudioService.cs)
- [LinuxAudioService.cs](file:///workspace/PCL-CE.Neo.Platform/Linux/LinuxAudioService.cs)

### 5. IThemeService

主题管理接口。

```csharp
public interface IThemeService
{
    Task<AppTheme> GetSystemThemeAsync();
    void SetTheme(AppTheme theme);
    AppTheme CurrentTheme { get; }
    event Action<AppTheme>? ThemeChanged;
}
```

**实现文件：**
- [IThemeService.cs](file:///workspace/PCL-CE.Neo.Core.Abstractions/IThemeService.cs)
- [WindowsThemeService.cs](file:///workspace/PCL-CE.Neo.Platform/Windows/WindowsThemeService.cs)
- [MacOSThemeService.cs](file:///workspace/PCL-CE.Neo.Platform/macOS/MacOSThemeService.cs)
- [LinuxThemeService.cs](file:///workspace/PCL-CE.Neo.Platform/Linux/LinuxThemeService.cs)

### 6. IClipboardService

剪贴板操作接口。

```csharp
public interface IClipboardService
{
    Task<string?> GetTextAsync();
    Task SetTextAsync(string text);
    void Clear();
}
```

**实现文件：**
- [IClipboardService.cs](file:///workspace/PCL-CE.Neo.Core.Abstractions/IClipboardService.cs)
- [WindowsClipboardService.cs](file:///workspace/PCL-CE.Neo.Platform/Windows/WindowsClipboardService.cs)
- [MacOSClipboardService.cs](file:///workspace/PCL-CE.Neo.Platform/macOS/MacOSClipboardService.cs)
- [LinuxClipboardService.cs](file:///workspace/PCL-CE.Neo.Platform/Linux/LinuxClipboardService.cs)

### 7. IDialogService

对话框接口。

```csharp
public interface IDialogService
{
    Task<bool> ShowConfirmAsync(string title, string message);
    Task ShowErrorAsync(string title, string message);
    Task ShowInfoAsync(string title, string message);
    Task<string?> ShowInputAsync(string title, string message, string? defaultValue = null);
    Task<string?> ShowFilePickerAsync(string filter, string? initialDirectory = null);
    Task<string?> ShowFolderPickerAsync(string? initialDirectory = null);
}
```

**实现文件：**
- [IDialogService.cs](file:///workspace/PCL-CE.Neo.Core.Abstractions/IDialogService.cs)
- [WindowsDialogService.cs](file:///workspace/PCL-CE.Neo.Platform/Windows/WindowsDialogService.cs)
- [MacOSDialogService.cs](file:///workspace/PCL-CE.Neo.Platform/macOS/MacOSDialogService.cs)
- [LinuxDialogService.cs](file:///workspace/PCL-CE.Neo.Platform/Linux/LinuxDialogService.cs)

### 8. INotificationService

系统通知接口。

```csharp
public interface INotificationService
{
    Task ShowNotificationAsync(string title, string message, NotificationSeverity severity = NotificationSeverity.Info);
    bool IsSupported { get; }
}
```

**实现文件：**
- [INotificationService.cs](file:///workspace/PCL-CE.Neo.Core.Abstractions/INotificationService.cs)
- [WindowsNotificationService.cs](file:///workspace/PCL-CE.Neo.Platform/Windows/WindowsNotificationService.cs)
- [MacOSNotificationService.cs](file:///workspace/PCL-CE.Neo.Platform/macOS/MacOSNotificationService.cs)
- [LinuxNotificationService.cs](file:///workspace/PCL-CE.Neo.Platform/Linux/LinuxNotificationService.cs)

### 9. IUIAccessProvider

UI 辅助功能接口。

```csharp
public interface IUIAccessProvider
{
    bool IsScreenReaderActive();
    void SetFocus(IntPtr hwnd);
    void BringWindowToFront(IntPtr hwnd);
    IntPtr GetForegroundWindow();
}
```

**实现文件：**
- [IUIAccessProvider.cs](file:///workspace/PCL-CE.Neo.Core.Abstractions/IUIAccessProvider.cs)
- [WindowsUIAccessProvider.cs](file:///workspace/PCL-CE.Neo.Platform/Windows/WindowsUIAccessProvider.cs)
- [MacOSUIAccessProvider.cs](file:///workspace/PCL-CE.Neo.Platform/macOS/MacOSUIAccessProvider.cs)
- [LinuxUIAccessProvider.cs](file:///workspace/PCL-CE.Neo.Platform/Linux/LinuxUIAccessProvider.cs)

## Mock 实现

所有接口都提供了 Mock 实现，用于测试：

```
PCL-CE.Neo.Core.Abstractions/Mock/
├── PlatformServiceMock.cs
├── WindowServiceMock.cs
├── JavaScannerMock.cs
├── ThemeServiceMock.cs
├── AudioServiceMock.cs
├── ClipboardServiceMock.cs
├── DialogServiceMock.cs
├── NotificationServiceMock.cs
└── UIAccessProviderMock.cs
```

## 使用示例

```csharp
public class MyService
{
    private readonly IJavaScanner _javaScanner;
    
    public MyService(IJavaScanner javaScanner)
    {
        _javaScanner = javaScanner;
    }
    
    public async Task SetupJavaAsync()
    {
        var javaList = await _javaScanner.ScanJavaAsync();
        foreach (var java in javaList)
        {
            Console.WriteLine($"Found Java: {java.Path} ({java.Version})");
        }
    }
}
```

## 平台检测

使用 `PlatformDetector` 类检测当前平台：

```csharp
var platform = PlatformDetector.Detect();
switch (platform)
{
    case PlatformType.Windows:
        // Windows 特定代码
        break;
    case PlatformType.MacOS:
        // macOS 特定代码
        break;
    case PlatformType.Linux:
        // Linux 特定代码
        break;
}
```

---

**最后更新：2026-05-13**
