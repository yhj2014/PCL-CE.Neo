# Linux 平台开发指南

## 开发环境

### 要求

- Ubuntu 22.04+ (推荐) 或其他主流 Linux 发行版
- VS Code 或 Rider
- .NET 10 SDK
- GTK 3.0 (Uno Platform 依赖)

### 安装

#### Ubuntu/Debian

```bash
# 添加 .NET 仓库
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

# 安装 .NET SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0

# 安装 GTK 依赖
sudo apt-get install -y libgtk-3-dev
```

参考 [开发环境配置](../development/DEVELOPMENT.md)。

## Linux 平台特性

### 原生 API

Linux 平台实现使用：
- GTK (通过 Uno Platform)
- FreeDesktop.org 规范
- X11/Wayland
- 系统通知 (libnotify)

### 平台服务实现

`LinuxPlatformService` 提供：
- 平台信息获取
- 使用 `xdg-open` 打开 URL 和文件夹
- Linux 标准路径管理 (XDG)

### Java 检测

`LinuxJavaScanner` 扫描：
- `/usr/lib/jvm/`
- `/usr/java/`
- `~/java/`
- 系统 PATH

## Linux 特定注意事项

### 发行版差异

考虑不同发行版的差异：
- Ubuntu/Debian
- Fedora
- Arch Linux

### 依赖管理

确保正确处理库依赖。

### 打包

推荐使用：
- AppImage (跨发行版)
- DEB (Debian/Ubuntu)
- RPM (Fedora)

## 调试

使用 VS Code 或 Rider 调试 Linux 平台代码。
