# macOS 平台开发指南

## 开发环境

### 要求

- macOS 12 (Monterey) 或更高版本
- Visual Studio for Mac 或 VS Code
- .NET 10 SDK
- Xcode 命令行工具

### 安装

```bash
# 安装 Xcode 命令行工具
xcode-select --install
```

参考 [开发环境配置](../development/DEVELOPMENT.md)。

## macOS 平台特性

### 原生 API

macOS 平台实现使用：
- AppKit
- Core Services
- macOS 通知中心
- 系统事件

### 平台服务实现

`MacOSPlatformService` 提供：
- 平台信息获取
- 使用 `open` 命令打开 URL 和文件夹
- macOS 特定路径管理

### Java 检测

`MacOSJavaScanner` 扫描：
- `/Library/Java/JavaVirtualMachines/`
- `~/Library/Java/JavaVirtualMachines/`
- 系统 PATH

## macOS 特定注意事项

### 沙盒

注意 macOS 应用沙盒限制。

### 公证

发布前需要对应用进行公证。

### 打包

使用 .app 包格式和 DMG 分发。

## 调试

使用 Visual Studio for Mac 或 VS Code 调试 macOS 平台代码。
