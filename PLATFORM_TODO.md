# 平台实现待办清单

## 项目概述
PCL-CE.Neo 是一个跨平台 Minecraft 启动器，使用 .NET 10 SDK 和 Uno Platform 实现。
- **当前阶段**：Phase 1-2（基础框架和适配器层）
- **目标**：三个平台都能编译通过并启动到主界面

---

## 平台实现状态

### ✅ Windows 平台 - 已实现
**文件路径**：`/workspace/PCL-CE.Neo.Platform/Windows/`

#### 已完成功能
- [x] `WindowsNotificationService.cs` - 系统通知（MessageBox）
- [x] `WindowsUIAccessProvider.cs` - UI线程访问（WPF Dispatcher）
- [x] `WindowsWindowService.cs` - 窗口管理（WPF Window）
- [x] `WindowsAudioService.cs` - 音频播放（System.Media.SoundPlayer）
- [x] `WindowsDialogService.cs` - 文件对话框（Win32 Dialog）
- [x] `WindowsClipboardService.cs` - 剪贴板（WPF Clipboard）
- [x] `WindowsThemeService.cs` - 主题检测（Registry）
- [x] `WindowsJavaScanner.cs` - Java扫描（Registry + 文件扫描）
- [x] `WindowsPlatformService.cs` - 平台服务
- [x] `WindowsAnimationService.cs` - 动画服务

**最小验证场景**：✅ 可以启动到主界面

---

### 🔶 macOS 平台 - 部分实现
**文件路径**：`/workspace/PCL-CE.Neo.Platform/macOS/`

#### 待实现功能
- [ ] `MacOSNotificationService.cs`
  - 方法：`ShowNotification`, `ShowUpdateNotification`, `ShowDownloadCompleteNotification`, `ClearAllNotifications`
  - 需要的API：`NSUserNotification` 或 `UNUserNotificationCenter`
  - 优先级：高
  - 当前状态：使用Mock实现

- [ ] `MacOSUIAccessProvider.cs`
  - 方法：`Invoke`, `InvokeAsync`, `CheckAccess`, `GetScreenDpi`, `GetScreenSize`
  - 需要的API：`AppKit DispatchQueue`
  - 优先级：中
  - 当前状态：使用Mock实现

- [ ] `MacOSWindowService.cs`
  - 方法：`ShowMainWindow`, `CloseMainWindow`, `SetTitle`, `SetSize`, `SetPosition`, `Minimize`, `Maximize`, `Restore`, `SetTopmost`, `GetSystemDpi`
  - 需要的API：`AppKit NSWindow`
  - 优先级：高
  - 当前状态：使用Mock实现

- [ ] `MacOSAudioService.cs`
  - 方法：`Play`, `Stop`, `Pause`, `Resume`, `SetVolume`, `GetVolume`
  - 需要的API：`AVFoundation AVAudioPlayer`
  - 优先级：中
  - 当前状态：使用Mock实现

- [ ] `MacOSDialogService.cs`
  - 方法：`ShowOpenFileDialog`, `ShowSaveFileDialog`, `ShowOpenFolderDialog`, `ShowMessageBox`, `ShowConfirmation`
  - 需要的API：`AppKit NSSavePanel`, `NSOpenPanel`
  - 优先级：高
  - 当前状态：需要检查实现

- [ ] `MacOSClipboardService.cs`
  - 方法：`GetText`, `SetText`, `GetImage`, `SetImage`, `Clear`
  - 需要的API：`AppKit NSPasteboard`
  - 优先级：中
  - 当前状态：需要检查实现

- [ ] `MacOSThemeService.cs`
  - 方法：`GetSystemTheme`, `IsDarkMode`, `GetAccentColor`
  - 需要的API：`NSApp.effectiveAppearance`
  - 优先级：低
  - 当前状态：需要检查实现

- [ ] `MacOSJavaScanner.cs`
  - 方法：`ScanJavaPathsAsync`, `GetJavaInfoAsync`, `IsValidJavaPath`
  - 需要的API：`/usr/libexec/java_home`, `/Library/Java/JavaVirtualMachines/`
  - 优先级：高
  - 当前状态：需要检查实现

- [ ] `MacOSAnimationService.cs`
  - 方法：`AnimateAsync`, `FadeInAsync`, `FadeOutAsync`, `ScaleAsync`, `MoveToAsync`
  - 需要的API：`AppKit NSAnimationContext`
  - 优先级：低
  - 当前状态：✅ 已实现

**最小验证场景**：⚠️ 需要修复才能启动到主界面

---

### 🔶 Linux 平台 - 部分实现
**文件路径**：`/workspace/PCL-CE.Neo.Platform/Linux/`

#### 待实现功能
- [ ] `LinuxNotificationService.cs`
  - 方法：`ShowNotification`, `ShowUpdateNotification`, `ShowDownloadCompleteNotification`, `ClearAllNotifications`
  - 需要的API：`notify-send` (libnotify)
  - 优先级：高
  - 当前状态：使用Process调用notify-send

- [ ] `LinuxUIAccessProvider.cs`
  - 方法：`Invoke`, `InvokeAsync`, `CheckAccess`, `GetScreenDpi`, `GetScreenSize`
  - 需要的API：`X11`, `GTK`, 或者跨平台UI框架
  - 优先级：中
  - 当前状态：使用Mock实现

- [ ] `LinuxWindowService.cs`
  - 方法：`ShowMainWindow`, `CloseMainWindow`, `SetTitle`, `SetSize`, `SetPosition`, `Minimize`, `Maximize`, `Restore`, `SetTopmost`, `GetSystemDpi`
  - 需要的API：`GTK#`, `Avalonia`, 或 Uno Platform
  - 优先级：高
  - 当前状态：使用Mock实现

- [ ] `LinuxAudioService.cs`
  - 方法：`Play`, `Stop`, `Pause`, `Resume`, `SetVolume`, `GetVolume`
  - 需要的API：`GStreamer`, `ALSA`, 或 `PulseAudio`
  - 优先级：中
  - 当前状态：使用Process调用paplay

- [ ] `LinuxDialogService.cs`
  - 方法：`ShowOpenFileDialog`, `ShowSaveFileDialog`, `ShowOpenFolderDialog`, `ShowMessageBox`, `ShowConfirmation`
  - 需要的API：`GTK# FileChooserDialog`
  - 优先级：高
  - 当前状态：需要检查实现

- [ ] `LinuxClipboardService.cs`
  - 方法：`GetText`, `SetText`, `GetImage`, `SetImage`, `Clear`
  - 需要的API：`X11 Clipboard`, 或 `GI`, `Clip`
  - 优先级：中
  - 当前状态：需要检查实现

- [ ] `LinuxThemeService.cs`
  - 方法：`GetSystemTheme`, `IsDarkMode`, `GetAccentColor`
  - 需要的API：`GSettings`, `Xresources`
  - 优先级：低
  - 当前状态：需要检查实现

- [ ] `LinuxJavaScanner.cs`
  - 方法：`ScanJavaPathsAsync`, `GetJavaInfoAsync`, `IsValidJavaPath`
  - 需要的API：`/usr/lib/jvm/`, `~/.sdkman/candidates/java/`
  - 优先级：高
  - 当前状态：需要检查实现

- [ ] `LinuxAnimationService.cs`
  - 方法：`AnimateAsync`, `FadeInAsync`, `FadeOutAsync`, `ScaleAsync`, `MoveToAsync`
  - 需要的API：Uno Platform 动画系统
  - 优先级：低
  - 当前状态：需要检查实现

**最小验证场景**：⚠️ 需要修复才能启动到主界面

---

## 实现优先级

### 高优先级（核心功能）
1. ✅ Windows - 完整实现
2. 🔶 macOS - `DialogService`, `JavaScanner`, `NotificationService`
3. 🔶 Linux - `DialogService`, `JavaScanner`, `NotificationService`

### 中优先级（用户体验）
1. 🔶 macOS - `WindowService`, `ClipboardService`, `AudioService`
2. 🔶 Linux - `WindowService`, `ClipboardService`, `AudioService`

### 低优先级（增强功能）
1. 🔶 macOS - `ThemeService`, `AnimationService`
2. 🔶 Linux - `ThemeService`, `AnimationService`

---

## 技术方案

### Windows 平台
- **UI框架**：WPF (Windows Presentation Foundation)
- **窗口管理**：System.Windows.Window
- **对话框**：Microsoft.Win32.OpenFileDialog, SaveFileDialog
- **主题检测**：Microsoft.Win32.Registry
- **Java扫描**：Registry + 文件系统扫描

### macOS 平台
- **UI框架**：AppKit (Uno Platform)
- **窗口管理**：AppKit.NSWindow
- **对话框**：AppKit.NSSavePanel, NSOpenPanel
- **主题检测**：NSApp.effectiveAppearance
- **Java扫描**：/usr/libexec/java_home + /Library/Java/JavaVirtualMachines/

### Linux 平台
- **UI框架**：GTK# 或 Uno Platform
- **窗口管理**：GTK.Window
- **对话框**：Gtk.FileChooserDialog
- **主题检测**：GSettings, Xresources
- **Java扫描**：~/.sdkman/candidates/java/ + /usr/lib/jvm/

---

## 条件编译指令

```csharp
#if WINDOWS
    // Windows 特有代码
#elif MACCATALYST
    // macOS 特有代码
#elif LINUX
    // Linux 特有代码
#else
    // 通用代码
#endif
```

---

## 进度追踪

### Phase 1-2 完成情况
- ✅ 基础项目结构和配置
- ✅ 核心抽象层（PCL-CE.Neo.Core.Abstractions）
- ✅ 核心实现层（PCL-CE.Neo.Core）
- ✅ Windows 平台完整实现
- 🔶 macOS 平台部分实现
- 🔶 Linux 平台部分实现

### Phase 3 目标
- [ ] macOS 平台核心功能实现
- [ ] Linux 平台核心功能实现
- [ ] 跨平台UI统一

---

**最后更新**：2025-XX-XX
**负责人**：PCL Community
**版本**：1.0.0
