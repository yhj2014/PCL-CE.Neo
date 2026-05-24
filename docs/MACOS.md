# macOS 平台实现指南

## 概述

本文档描述 macOS 平台上的具体实现细节。

## 平台特性

- **目标框架**: net10.0-macos
- **窗口管理**: 使用 AppKit (NSWindow)
- **Java 检测**: 使用 /usr/libexec/java_home
- **音频**: 使用 AVFoundation
- **主题**: 支持深色/浅色主题检测 (NSApp.Appearance)

## 实现文件

| 文件 | 描述 |
|------|------|
| `MacOSPlatformService.cs` | 平台信息和操作 |
| `MacOSWindowService.cs` | 窗口管理 |
| `MacOSJavaScanner.cs` | Java 检测 |
| `MacOSAudioService.cs` | 音频播放 |
| `MacOSThemeService.cs` | 主题管理 |
| `MacOSClipboardService.cs` | 剪贴板操作 |
| `MacOSDialogService.cs` | 对话框 |
| `MacOSNotificationService.cs` | macOS 通知中心 |
| `MacOSUIAccessProvider.cs` | UI 辅助功能 |

## 关键实现细节

### Java 检测

macOS 平台通过以下方式检测 Java：

1. **/usr/libexec/java_home**
   这是 macOS 原生的 Java 主目录检测工具。

   ```csharp
   var psi = new ProcessStartInfo
   {
       FileName = "/usr/libexec/java_home",
       Arguments = "-V",
       UseShellExecute = false,
       RedirectStandardOutput = true
   };
   ```

2. **常见路径扫描**
   - `/Library/Java/JavaVirtualMachines/`
   - `/System/Library/Java/JavaVirtualMachines/`
   - `~/Library/Java/JavaVirtualMachines/`

### 窗口管理

使用 AppKit 的 NSWindow：

```swift
// 最小化
window.miniaturize(nil)

// 最大化
window.zoom(nil)

// 居中
window.center()
```

### 通知

使用 UserNotifications 框架：

```swift
let content = UNMutableNotificationContent()
content.title = title
content.body = message
let request = UNNotificationRequest(
    identifier: UUID().uuidString,
    content: content,
    trigger: nil
)
UNUserNotificationCenter.current().add(request)
```

## 测试

macOS 平台集成测试位于：

```
PCL-CE.Neo.Tests/PlatformIntegration/macOS/
```

## 已知限制

- macOS 10.15 (Catalina) 及以上支持
- Apple Silicon (M1/M2) 原生支持
- 不支持 macOS 上运行 Windows 游戏

## 相关文档

- [平台抽象接口规范](PLATFORM_ABSTRACTIONS.md)
- [架构设计文档](ARCHITECTURE.md)

---

**最后更新：2026-05-13**
