# 平台实现指南

## 概述

本文档说明如何为不同平台实现平台抽象层接口。

## 平台特定项目结构

每个平台的实现项目包含：

```
PCL-CE.Neo.Platform.<Platform>/
├── <Platform>PlatformService.cs       # IPlatformService 实现
├── <Platform>WindowService.cs         # IWindowService 实现
├── <Platform>JavaScanner.cs           # IJavaScanner 实现
├── <Platform>ThemeService.cs          # IThemeService 实现
├── <Platform>ClipboardService.cs      # IClipboardService 实现
├── <Platform>DialogService.cs         # IDialogService 实现
├── ServiceCollectionExtensions.cs     # DI 注册扩展
└── PCL-CE.Neo.Platform.<Platform>.csproj
```

## 实现步骤

### 1. 创建平台项目

参考现有平台项目结构创建新项目。

### 2. 实现平台服务

实现 `IPlatformService` 接口：

```csharp
public class MyPlatformService : IPlatformService
{
    public string PlatformName => "MyPlatform";
    public string OSVersion => Environment.OSVersion.VersionString;
    public string Architecture => RuntimeInformation.ProcessArchitecture.ToString();
    
    public void OpenUrl(string url)
    {
        // 平台特定的实现
    }
    
    public void OpenFolder(string path)
    {
        // 平台特定的实现
    }
    
    // ... 其他方法
}
```

### 3. 实现其他接口

按需要实现其他平台抽象接口。

### 4. 配置 DI 注册

创建 `ServiceCollectionExtensions`：

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlatformService, MyPlatformService>();
        services.AddSingleton<IWindowService, MyWindowService>();
        services.AddSingleton<IJavaScanner, MyJavaScanner>();
        // ... 其他服务
        return services;
    }
}
```

### 5. 更新 PlatformDetector

在 `PlatformDetector` 中添加对新平台的支持：

```csharp
private static string DetectCurrentPlatform()
{
    if (OperatingSystem.IsMyPlatform())
    {
        return "MyPlatform";
    }
    // ... 其他平台检测
}
```

## 平台特定实现细节

### Windows 平台

- 使用 Windows API
- 注册表访问
- WPF/UWP 集成

### macOS 平台

- 使用 AppKit
- macOS 原生 UI
- 系统集成

### Linux 平台

- 使用 Linux 系统调用
- GTK/FreeDesktop 集成
- 考虑不同发行版的差异

## 测试平台实现

- 在目标平台上进行实际测试
- 验证所有平台特定功能
- 确保性能可接受

## 平台服务测试清单

- [ ] 平台信息正确报告
- [ ] URL 打开功能正常
- [ ] 文件夹打开功能正常
- [ ] Java 路径检测正确
- [ ] 窗口管理功能正常
- [ ] 主题切换正常
- [ ] 剪贴板操作正常
- [ ] 对话框显示正常
- [ ] 通知显示正常

## 最佳实践

- 保持平台实现尽可能简洁
- 使用平台原生 API
- 处理平台特定的边缘情况
- 提供合理的默认行为
- 添加详细的日志记录
