# PCL-CE.Neo 架构设计文档

## 概述

PCL-CE.Neo 是一个跨平台 Minecraft 启动器，使用 Uno Platform 实现 UI，.NET 10 作为运行时框架。

## 架构原则

1. **平台抽象** - 所有平台特定功能通过接口抽象
2. **依赖注入** - 使用 Microsoft.Extensions.DependencyInjection
3. **MVVM/MVUX** - 使用 MVVM 或 MVUX 模式进行 UI 开发
4. **异步优先** - 所有 I/O 操作都是异步的

## 项目结构

```
PCL-CE.Neo/
├── PCL-CE.Neo.Core/              # 核心业务逻辑（跨平台）
├── PCL-CE.Neo.Core.Abstractions/ # 平台抽象接口
├── PCL-CE.Neo.Platform/          # 平台特定实现
│   ├── Windows/
│   ├── macOS/
│   └── Linux/
├── PCL-CE.Neo.UI/                # Uno Platform UI
└── PCL-CE.Neo.Tests/             # 测试项目
```

## 核心组件

### 1. PCL-CE.Neo.Core

核心业务逻辑层，包含所有不依赖平台的业务代码。

#### 主要命名空间

- `PCL_CE.Neo.Core.App` - 应用程序基础
- `PCL_CE.Neo.Core.Configuration` - 配置管理
- `PCL_CE.Neo.Core.Database` - 数据持久化
- `PCL_CE.Neo.Core.IO` - 文件操作和下载
- `PCL_CE.Neo.Core.Lifecycle` - 生命周期管理
- `PCL_CE.Neo.Core.Link` - 联机功能
- `PCL_CE.Neo.Core.Logging` - 日志系统
- `PCL_CE.Neo.Core.Minecraft` - Minecraft 相关
- `PCL_CE.Neo.Core.Network` - 网络请求
- `PCL_CE.Neo.Core.TaskManager` - 任务管理

### 2. PCL-CE.Neo.Core.Abstractions

平台抽象接口层，定义所有需要平台特定实现的接口。

#### 主要接口

| 接口 | 描述 |
|------|------|
| `IPlatformService` | 平台信息和操作 |
| `IWindowService` | 窗口管理 |
| `IJavaScanner` | Java 检测 |
| `IAudioService` | 音频播放 |
| `IThemeService` | 主题管理 |
| `IClipboardService` | 剪贴板操作 |
| `IDialogService` | 对话框 |
| `INotificationService` | 系统通知 |
| `IUIAccessProvider` | UI 辅助功能 |

### 3. PCL-CE.Neo.Platform

平台特定实现层，为每个平台实现抽象接口。

#### 平台特定实现

| 平台 | 命名空间 |
|------|----------|
| Windows | `PCL_CE.Neo.Platform.Windows` |
| macOS | `PCL_CE.Neo.Platform.MacOS` |
| Linux | `PCL_CE.Neo.Platform.Linux` |

## 依赖注入

所有服务通过 Microsoft.Extensions.DependencyInjection 进行注册和管理。

```csharp
services.AddCoreServices();
services.AddCoreAdapters();
services.AddPlatformServices(); // 根据当前平台添加
```

## 服务启动流程

1. 初始化 `Paths` - 设置数据目录
2. 初始化 `LogService` - 启动日志系统
3. 初始化 `ConfigService` - 加载配置
4. 初始化平台服务 - 根据平台注册服务
5. 启动生命周期服务

## 适配器模式

适配器层位于核心业务逻辑和平台抽象之间，提供类型安全的接口。

```
Core Business Logic → Adapter → Platform Abstraction → Platform Implementation
```

### 主要适配器

| 适配器 | 描述 |
|--------|------|
| `IConfigAdapter` | 配置读写 |
| `IDatabaseAdapter` | 数据存储 |
| `INetworkAdapter` | 网络请求 |
| `IDownloadAdapter` | 文件下载 |
| `IMinecraftAdapter` | Minecraft 操作 |
| `IAuthAdapter` | 认证登录 |
| `IModAdapter` | Mod 管理 |

## 跨平台兼容性

### Windows

- 使用 Win32 API 进行窗口管理
- 使用 WPF 的对话框组件
- 使用 Windows 注册表扫描 Java

### macOS

- 使用 AppKit 进行窗口管理
- 使用 NSAlert 进行对话框
- 使用 /usr/libexec/java_home 扫描 Java

### Linux

- 使用 GTK/Qt 进行窗口管理
- 使用 Zenity/Dialog 进行对话框
- 扫描常见 Java 安装路径

## 测试策略

- **单元测试** - 测试核心业务逻辑
- **集成测试** - 测试平台实现
- **冒烟测试** - 验证基本功能

## 性能考虑

1. **异步 I/O** - 所有磁盘和网络操作都是异步的
2. **缓存** - 常用数据进行内存缓存
3. **延迟加载** - 按需加载非关键资源
4. **增量更新** - 只更新必要的数据

## 安全性

1. **令牌存储** - 使用安全存储保存认证令牌
2. **代理支持** - 支持 HTTP/SOCKS5 代理
3. **SSL/TLS** - 所有网络请求使用 HTTPS
4. **文件校验** - 下载文件进行哈希校验

## 扩展性

1. **插件系统** - 支持 Mod 加载
2. **自定义下载源** - 支持多个下载镜像
3. **第三方认证** - 支持 Yggdrasil API

## 文档更新记录

| 日期 | 版本 | 变更内容 |
|------|------|----------|
| 2026-05-13 | 1.0 | 初始版本 |

---

**最后更新：2026-05-13**
