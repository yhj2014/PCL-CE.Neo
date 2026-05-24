# Windows 平台开发指南

## 开发环境

### 要求

- Windows 10 1903 或更高版本
- Visual Studio 2022 (推荐) 或 VS Code
- .NET 10 SDK
- Windows SDK

### 安装

参考 [开发环境配置](../development/DEVELOPMENT.md)。

## Windows 平台特性

### 原生 API

Windows 平台实现使用：
- Win32 API
- Windows Runtime (WinRT)
- 注册表访问
- Windows 通知系统

### 平台服务实现

`WindowsPlatformService` 提供：
- 平台信息获取
- URL 打开
- 文件夹打开
- 路径管理

### Java 检测

`WindowsJavaScanner` 使用：
- 注册表检测
- 路径扫描
- Java 版本验证

## Windows 特定注意事项

### 高 DPI 支持

确保正确处理高 DPI 显示。

### 权限

某些操作需要管理员权限。

### 打包

使用 MSIX 或传统安装程序打包。

## 调试

使用 Visual Studio 调试 Windows 平台特定代码。
