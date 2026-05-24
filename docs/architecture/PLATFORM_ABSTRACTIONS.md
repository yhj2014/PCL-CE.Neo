# PCL-CE.Neo 平台抽象规范

本文档定义 PCL-CE.Neo 跨平台架构中的所有平台抽象接口。

## 目录

1. [概述](#1-概述)
2. [核心接口](#2-核心接口)
3. [UI 相关接口](#3-ui-相关接口)
4. [系统服务接口](#4-系统服务接口)
5. [接口实现指南](#5-接口实现指南)

---

## 1. 概述

### 1.1 项目背景

PCL-CE.Neo 是 PCL Community Edition 的跨平台重构项目，由社区开发者发起，旨在为非 Windows 用户提供 PCL 启动器的使用体验。

### 1.2 设计原则

- **最小化**：只抽象真正需要跨平台的功能
- **清晰性**：接口职责单一，易于理解
- **可测试性**：所有接口都应易于 mock 和测试
- **向后兼容**：接口变更需要谨慎，避免破坏现有实现

### 1.2 命名规范

- 接口名称以 `I` 开头
- 使用 PascalCase 命名
- 方法名使用动词或动词短语
- 事件使用 `EventHandler` 或 `Action`

---

## 2. 核心接口

### 2.1 IPlatformService - 平台服务

基础平台服务接口，提供平台信息和基本操作。

```csharp
namespace PCL.Core.Abstractions;

public interface IPlatformService
{
    /// <summary>
    /// 平台名称 (Windows/macOS/Linux)
    /// </summary>
    string PlatformName { get; }

    /// <summary>
    /// 操作系统版本
    /// </summary>
    string OSVersion { get; }

    /// <summary>
    /// CPU 架构
    /// </summary>
    string Architecture { get; }

    /// <summary>
    /// 打开 URL
    /// </summary>
    void OpenUrl(string url);

    /// <summary>
    /// 打开文件夹
    /// </summary>
    void OpenFolder(string path);

    /// <summary>
    /// 获取本地应用数据路径
    /// </summary>
    string GetLocalApplicationDataPath();

    /// <summary>
    /// 获取临时文件路径
    /// </summary>
    string GetTempPath();

    /// <summary>
    /// 获取游戏数据目录
    /// </summary>
    string GetGameDataPath();
}
```

### 2.2 IJavaScanner - Java 扫描器

用于检测系统上安装的 Java 运行时。

```csharp
namespace PCL.Core.Abstractions;

public interface IJavaScanner
{
    /// <summary>
    /// 扫描所有可用的 Java 路径
    /// </summary>
    IEnumerable<string> ScanJavaPaths();

    /// <summary>
    /// 扫描指定目录
    /// </summary>
    IEnumerable<string> ScanDirectory(string directory);

    /// <summary>
    /// 验证 Java 路径是否有效
    /// </summary>
    bool IsValidJavaPath(string path);
}
```

---

## 3. UI 相关接口

### 3.1 IWindowService - 窗口服务

窗口管理和控制接口。

```csharp
namespace PCL.Core.Abstractions;

public interface IWindowService
{
    /// <summary>
    /// 主窗口实例
    /// </summary>
    object? MainWindow { get; }

    /// <summary>
    /// 初始化窗口系统
    /// </summary>
    void Initialize();

    /// <summary>
    /// 显示主窗口
    /// </summary>
    void ShowMainWindow();

    /// <summary>
    /// 关闭主窗口
    /// </summary>
    void CloseMainWindow();

    /// <summary>
    /// 设置窗口标题
    /// </summary>
    void SetTitle(string title);

    /// <summary>
    /// 设置窗口大小
    /// </summary>
    void SetSize(int width, int height);

    /// <summary>
    /// 设置窗口位置
    /// </summary>
    void SetPosition(int x, int y);

    /// <summary>
    /// 最小化窗口
    /// </summary>
    void Minimize();

    /// <summary>
    /// 最大化窗口
    /// </summary>
    void Maximize();

    /// <summary>
    /// 还原窗口
    /// </summary>
    void Restore();

    /// <summary>
    /// 设置窗口置顶
    /// </summary>
    void SetTopmost(bool topmost);
}
```

### 3.2 IThemeService - 主题服务

主题管理接口。

```csharp
namespace PCL.Core.Abstractions;

public interface IThemeService
{
    /// <summary>
    /// 主题变更事件
    /// </summary>
    event EventHandler ThemeChanged;

    /// <summary>
    /// 获取当前主题
    /// </summary>
    ThemeInfo GetCurrentTheme();

    /// <summary>
    /// 设置主题
    /// </summary>
    void SetTheme(ThemeInfo theme);

    /// <summary>
    /// 获取可用主题列表
    /// </summary>
    IEnumerable<ThemeInfo> GetAvailableThemes();

    /// <summary>
    /// 检测系统主题
    /// </summary>
    ThemeType DetectSystemTheme();
}

public class ThemeInfo
{
    public string Name { get; set; } = string.Empty;
    public ThemeType Type { get; set; }
    public string ResourcePath { get; set; } = string.Empty;
}

public enum ThemeType
{
    Light,
    Dark,
    System
}
```

### 3.3 IUIAccessProvider - UI 访问提供者

提供 UI 线程访问和 Dispatcher 功能。

```csharp
namespace PCL.Core.Abstractions;

public interface IUIAccessProvider
{
    /// <summary>
    /// 在 UI 线程上执行操作
    /// </summary>
    void Invoke(Action action);

    /// <summary>
    /// 异步在 UI 线程上执行操作
    /// </summary>
    Task InvokeAsync(Action action);

    /// <summary>
    /// 检查是否在 UI 线程上
    /// </summary>
    bool CheckAccess();

    /// <summary>
    /// 获取屏幕 DPI
    /// </summary>
    double GetScreenDpi();

    /// <summary>
    /// 获取屏幕尺寸
    /// </summary>
    (int Width, int Height) GetScreenSize();
}
```

### 3.4 IAnimationService - 动画服务

跨平台动画支持。

```csharp
namespace PCL.Core.Abstractions;

public interface IAnimationService
{
    /// <summary>
    /// 开始动画
    /// </summary>
    void StartAnimation(AnimationInfo animation);

    /// <summary>
    /// 停止动画
    /// </summary>
    void StopAnimation(string animationId);

    /// <summary>
    /// 暂停动画
    /// </summary>
    void PauseAnimation(string animationId);

    /// <summary>
    /// 恢复动画
    /// </summary>
    void ResumeAnimation(string animationId);
}

public class AnimationInfo
{
    public string Id { get; set; } = string.Empty;
    public string TargetElement { get; set; } = string.Empty;
    public string Property { get; set; } = string.Empty;
    public object? From { get; set; }
    public object? To { get; set; }
    public TimeSpan Duration { get; set; }
    public IEasingFunction? Easing { get; set; }
}
```

---

## 4. 系统服务接口

### 4.1 IAudioService - 音频服务

音频播放控制。

```csharp
namespace PCL.Core.Abstractions;

public interface IAudioService
{
    /// <summary>
    /// 播放音频文件
    /// </summary>
    void Play(string filePath);

    /// <summary>
    /// 停止播放
    /// </summary>
    void Stop();

    /// <summary>
    /// 暂停播放
    /// </summary>
    void Pause();

    /// <summary>
    /// 恢复播放
    /// </summary>
    void Resume();

    /// <summary>
    /// 设置音量 (0-100)
    /// </summary>
    void SetVolume(int volume);

    /// <summary>
    /// 获取当前音量
    /// </summary>
    int GetVolume();

    /// <summary>
    /// 是否正在播放
    /// </summary>
    bool IsPlaying { get; }

    /// <summary>
    /// 播放完成事件
    /// </summary>
    event EventHandler PlaybackFinished;
}
```

### 4.2 IClipboardService - 剪贴板服务

剪贴板操作接口。

```csharp
namespace PCL.Core.Abstractions;

public interface IClipboardService
{
    /// <summary>
    /// 获取剪贴板文本
    /// </summary>
    string? GetText();

    /// <summary>
    /// 设置剪贴板文本
    /// </summary>
    void SetText(string text);

    /// <summary>
    /// 获取剪贴板图片
    /// </summary>
    byte[]? GetImage();

    /// <summary>
    /// 设置剪贴板图片
    /// </summary>
    void SetImage(byte[] imageData);

    /// <summary>
    /// 清空剪贴板
    /// </summary>
    void Clear();
}
```

### 4.3 IDialogService - 对话框服务

系统对话框接口。

```csharp
namespace PCL.Core.Abstractions;

public interface IDialogService
{
    /// <summary>
    /// 显示打开文件对话框
    /// </summary>
    string? ShowOpenFileDialog(string filter, string? initialDirectory = null);

    /// <summary>
    /// 显示保存文件对话框
    /// </summary>
    string? ShowSaveFileDialog(string filter, string defaultFileName, string? initialDirectory = null);

    /// <summary>
    /// 显示打开文件夹对话框
    /// </summary>
    string? ShowOpenFolderDialog(string? initialDirectory = null);

    /// <summary>
    /// 显示消息框
    /// </summary>
    DialogResult ShowMessageBox(string message, string title, DialogButtons buttons);

    /// <summary>
    /// 显示确认对话框
    /// </summary>
    bool ShowConfirmation(string message, string title);
}

public enum DialogResult
{
    OK,
    Cancel,
    Yes,
    No,
    None
}

public enum DialogButtons
{
    OK,
    OKCancel,
    YesNo,
    YesNoCancel
}
```

### 4.4 INotificationService - 通知服务

系统通知接口。

```csharp
namespace PCL.Core.Abstractions;

public interface INotificationService
{
    /// <summary>
    /// 显示通知
    /// </summary>
    void ShowNotification(NotificationInfo notification);

    /// <summary>
    /// 显示更新通知
    /// </summary>
    void ShowUpdateNotification(string version, string releaseNotes);

    /// <summary>
    /// 显示下载完成通知
    /// </summary>
    void ShowDownloadCompleteNotification(string fileName);

    /// <summary>
    /// 清除所有通知
    /// </summary>
    void ClearAllNotifications();
}

public class NotificationInfo
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string? ActionText { get; set; }
    public Action? Action { get; set; }
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}
```

---

## 5. 接口实现指南

### 5.1 Windows 实现

Windows 平台实现应使用：
- `System.Diagnostics.Process.Start` 打开 URL 和文件夹
- `Registry` 访问注册表
- `Win32 API` 用于高级功能
- `WPF/WinUI` 用于 UI

### 5.2 macOS 实现

macOS 平台实现应使用：
- `open` 命令打开 URL 和文件夹
- 文件系统扫描检测 Java
- `AppKit/Uno` 用于 UI

### 5.3 Linux 实现

Linux 平台实现应使用：
- `xdg-open` 打开 URL 和文件夹
- 文件系统扫描检测 Java
- `GTK/Uno` 用于 UI

### 5.4 依赖注入配置

```csharp
// Windows
services.AddSingleton<IPlatformService, WindowsPlatformService>();
services.AddSingleton<IJavaScanner, WindowsJavaScanner>();

// macOS
services.AddSingleton<IPlatformService, MacOSPlatformService>();
services.AddSingleton<IJavaScanner, MacOSJavaScanner>();

// Linux
services.AddSingleton<IPlatformService, LinuxPlatformService>();
services.AddSingleton<IJavaScanner, LinuxJavaScanner>();
```

---

## 6. 测试指南

所有平台抽象接口都应提供 Mock 实现以支持单元测试：

```csharp
public class MockPlatformService : IPlatformService
{
    public string PlatformName { get; set; } = "Test";
    public string OSVersion { get; set; } = "1.0.0";
    public string Architecture { get; set; } = "x64";

    // 实现其他方法...
}
```

---

## 7. 变更日志

| 版本 | 日期 | 变更内容 |
|------|------|----------|
| 1.0 | 2026-05-12 | 初始版本 |
