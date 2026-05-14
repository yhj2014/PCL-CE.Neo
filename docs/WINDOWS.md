# Windows 平台实现指南

## 概述

本文档描述 Windows 平台上的具体实现细节。

## 平台特性

- **目标框架**: net10.0-windows
- **窗口管理**: 使用 Win32 API 或 WPF
- **Java 检测**: 扫描注册表和常见路径
- **音频**: 使用 NAudio 或 Windows Media Player
- **主题**: 支持深色/浅色主题检测

## 实现文件

| 文件 | 描述 |
|------|------|
| `WindowsPlatformService.cs` | 平台信息和操作 |
| `WindowsWindowService.cs` | 窗口管理 |
| `WindowsJavaScanner.cs` | Java 检测 |
| `WindowsAudioService.cs` | 音频播放 |
| `WindowsThemeService.cs` | 主题管理 |
| `WindowsClipboardService.cs` | 剪贴板操作 |
| `WindowsDialogService.cs` | 对话框 |
| `WindowsNotificationService.cs` | Windows Toast 通知 |
| `WindowsUIAccessProvider.cs` | UI 辅助功能 |

## 关键实现细节

### Java 检测

Windows 平台通过以下方式检测 Java：

1. **注册表扫描**
   - `HKEY_LOCAL_MACHINE\SOFTWARE\JavaSoft\Java Development Kit`
   - `HKEY_LOCAL_MACHINE\SOFTWARE\JavaSoft\Java Runtime Environment`

2. **常见路径扫描**
   - `C:\Program Files\Java\`
   - `C:\Program Files (x86)\Java\`
   - `%APPDATA%\.java\`

3. **JAVA_HOME 环境变量**

### 窗口管理

使用 WPF 的 Window 类或 Win32 API：

```csharp
// 最小化
WindowState = WindowState.Minimized;

// 最大化
WindowState = WindowState.Maximized;

// 恢复
WindowState = WindowState.Normal;
```

### Toast 通知

使用 Windows.UI.Notifications 或 Microsoft.Toolkit.Uwp.Notifications：

```csharp
var template = ToastNotificationManager.GetTemplateContent(
    ToastTemplateType.ToastText02);
template.GetElementsByTagName("text")[0].InnerText = title;
template.GetElementsByTagName("text")[1].InnerText = message;
```

## 测试

Windows 平台具有最完整的集成测试：

```
PCL-CE.Neo.Tests/PlatformIntegration/Windows/
├── WindowsPlatformServiceIntegrationTests.cs
├── WindowsThemeServiceIntegrationTests.cs
├── WindowsJavaScannerIntegrationTests.cs
├── WindowsDialogServiceIntegrationTests.cs
├── WindowsNotificationServiceIntegrationTests.cs
├── WindowsAudioServiceIntegrationTests.cs
├── WindowsUIAccessProviderIntegrationTests.cs
├── WindowsClipboardServiceIntegrationTests.cs
├── WindowsServiceBuilderIntegrationTests.cs
├── WindowsPlatformIntegrationTestSuite.cs
└── WindowsIntegrationTestUtils.cs
```

## 已知限制

- Windows 7 及更早版本支持有限
- 部分功能需要 Windows 10 及以上版本

## 相关文档

- [平台抽象接口规范](PLATFORM_ABSTRACTIONS.md)
- [架构设计文档](ARCHITECTURE.md)

---

**最后更新：2026-05-13**
