# Linux 平台实现指南

## 概述

本文档描述 Linux 平台上的具体实现细节。

## 平台特性

- **目标框架**: net10.0-linux
- **窗口管理**: 使用 GTK3/Qt5 或 X11/Wayland
- **Java 检测**: 扫描常见路径和 java 命令
- **音频**: 使用 ALSA/PulseAudio/PipeWire
- **主题**: 支持 GTK 主题检测

## 实现文件

| 文件 | 描述 |
|------|------|
| `LinuxPlatformService.cs` | 平台信息和操作 |
| `LinuxWindowService.cs` | 窗口管理 |
| `LinuxJavaScanner.cs` | Java 检测 |
| `LinuxAudioService.cs` | 音频播放 |
| `LinuxThemeService.cs` | 主题管理 |
| `LinuxClipboardService.cs` | 剪贴板操作 |
| `LinuxDialogService.cs` | 对话框 |
| `LinuxNotificationService.cs` | libnotify 通知 |
| `LinuxUIAccessProvider.cs` | UI 辅助功能 |

## 关键实现细节

### Java 检测

Linux 平台通过以下方式检测 Java：

1. **java 命令扫描**
   ```bash
   which java
   update-alternatives --list java
   ```

2. **常见路径扫描**
   - `/usr/lib/jvm/`
   - `/opt/java/`
   - `~/.java/`
   - `/usr/local/java/`

3. **JAVA_HOME 环境变量**

### 窗口管理

使用 X11 或 Wayland API：

```csharp
// X11 获取前台窗口
var display = XOpenDisplay(IntPtr.Zero);
var root = XDefaultRootWindow(display);
XGetInputFocus(display, out var window, out _);
```

### 对话框

使用 Zenity 或yad：

```bash
zenity --question --title="标题" --text="消息"
zenity --error --title="错误" --text="错误消息"
zenity --file-selection --directory
```

### 通知

使用 libnotify：

```bash
notify-send "标题" "消息"
```

## 测试

Linux 平台集成测试位于：

```
PCL-CE.Neo.Tests/PlatformIntegration/Linux/
```

## 已知限制

- 桌面环境兼容性取决于 GTK/Qt 版本
- 部分发行版可能缺少必要的依赖
- NVIDIA 显卡可能需要额外配置

## 依赖

### Ubuntu/Debian
```bash
sudo apt install libnotify-bin zenity default-jdk
```

### Fedora/RHEL
```bash
sudo dnf install notify-daemon zenity java-openjdk
```

### Arch Linux
```bash
sudo pacman -S libnotify zenity jdk-openjdk
```

## 相关文档

- [平台抽象接口规范](PLATFORM_ABSTRACTIONS.md)
- [架构设计文档](ARCHITECTURE.md)

---

**最后更新：2026-05-13**
