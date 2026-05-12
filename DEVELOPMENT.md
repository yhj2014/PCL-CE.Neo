# PCL CE 开发环境配置指南

本文档帮助开发者配置 PCL CE 跨平台重构项目的开发环境。

## 目录

1. [环境要求](#1-环境要求)
2. [.NET 10 SDK 安装](#2-net-10-sdk-安装)
3. [Uno Platform 安装](#3-uno-platform-安装)
4. [IDE 配置](#4-ide-配置)
5. [项目构建](#5-项目构建)
6. [常见问题](#6-常见问题)

---

## 1. 环境要求

### 1.1 最低要求

| 组件 | 最低版本 | 推荐版本 |
|------|----------|----------|
| 操作系统 | Windows 10 1903 | Windows 11 |
| 内存 | 8 GB | 16 GB |
| 磁盘空间 | 10 GB | 20 GB |
| 显卡 | 支持 DirectX 11 | 支持 DirectX 12 |

### 1.2 必需软件

- .NET 10 SDK
- Visual Studio 2022 17.8+ 或 VS Code
- Windows 10 SDK (19041+)
- Git

### 1.3 可选软件

- JetBrains Rider
- ReSharper
- Figma (UI 设计参考)

---

## 2. .NET 10 SDK 安装

### 2.1 Windows

**方式一：使用 winget**
```powershell
winget install Microsoft.DotNet.SDK.10
```

**方式二：手动安装**
1. 访问 [.NET 10 下载页面](https://dotnet.microsoft.com/download/dotnet/10.0)
2. 下载 x64 或 ARM64 SDK
3. 运行安装程序

**验证安装**：
```powershell
dotnet --version
# 应显示: 10.0.xxxx
```

### 2.2 macOS

```bash
brew install --cask dotnet
```

**验证安装**：
```bash
dotnet --version
```

### 2.3 Linux (Ubuntu/Debian)

```bash
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install dotnet-sdk-10.0
```

---

## 3. Uno Platform 安装

### 3.1 安装 Uno Platform Templates

```bash
dotnet new install Uno.Templates
```

### 3.2 验证安装

```bash
dotnet new list
# 应该看到 Uno 相关的模板
```

### 3.3 安装特定版本

```bash
dotnet new install Uno.Templates --version 5.0.0
```

---

## 4. IDE 配置

### 4.1 Visual Studio 2022

#### 必需工作负载
- .NET 跨平台开发
- 使用 C++ 的桌面开发（用于原生调试）
- Windows 10/11 SDK

#### 必需扩展
- [Uno Platform 扩展](https://marketplace.visualstudio.com/items?itemName=unoplatform.uno)
- C# Dev Kit
- .NET Aspire 工具

#### 配置步骤

1. 打开 Visual Studio Installer
2. 修改 Visual Studio 2022
3. 勾选以下工作负载：
   - .NET 跨平台开发
   - 使用 C++ 的桌面开发
4. 点击"修改"
5. 重启 Visual Studio

### 4.2 VS Code

#### 必需扩展
- C# (Microsoft)
- .NET Install Tool
- Uno Platform 扩展

#### 配置 .vscode/settings.json

```json
{
    "dotnet.defaultSolutionFile": "Plain Craft Launcher 2.slnx",
    "omnisharp.useModernNet": true,
    "omnisharp.enableRoslynAnalyzers": true
}
```

### 4.3 JetBrains Rider

#### 必需插件
- .NET 工具
- Uno Platform 支持（通过 ReSharper）

---

## 5. 项目构建

### 5.1 克隆仓库

```bash
git clone https://github.com/PCL-Community/PCL-CE.git
cd PCL-CE
```

### 5.2 还原依赖

```bash
dotnet restore
```

### 5.3 构建项目

**仅构建核心库**：
```bash
dotnet build PCL.Core/PCL.Core.csproj
```

**构建整个解决方案**：
```bash
dotnet build
```

**发布**：
```bash
dotnet publish PCL.App/PCL.App.csproj -c Release -r win-x64
```

### 5.4 运行测试

```bash
dotnet test PCL.Core.Test/PCL.Core.Test.csproj
```

---

## 6. 常见问题

### 6.1 构建错误

**错误：找不到 .NET 10 SDK**
```bash
# 检查已安装的 SDK
dotnet --list-sdks

# 如果只有旧版本，重新安装
```

**错误：NuGet 包还原失败**
```bash
# 清除缓存
dotnet nuget locals all --clear

# 重新还原
dotnet restore
```

### 6.2 Uno Platform 问题

**错误：WinUI 3 不可用**
```bash
# 安装 Windows App SDK
winget install Microsoft.WindowsAppSDK.1.5
```

**错误：macOS/Linux 构建失败**
```
确保已安装对应平台的 .NET 运行时和 SDK
```

### 6.3 性能问题

**IDE 卡顿**
- 增加内存分配
- 禁用不需要的扩展
- 使用增量构建

**构建缓慢**
```bash
# 使用并行构建
dotnet build -m 4

# 使用 Release 配置
dotnet build -c Release
```

---

## 7. 调试

### 7.1 调试 WPF 版本

1. 在 Visual Studio 中打开解决方案
2. 设置 `Plain Craft Launcher 2` 为启动项目
3. 按 F5 开始调试

### 7.2 调试 Uno 版本

1. 打开 `PCL.App` 项目
2. 选择目标平台（Windows/macOS/Linux）
3. 按 F5 开始调试

### 7.3 调试技巧

**条件断点**：
```csharp
if (condition)
{
    Debugger.Break();
}
```

**日志输出**：
```csharp
System.Diagnostics.Debug.WriteLine($"Variable: {value}");
```

---

## 8. 代码规范

### 8.1 命名规范

| 类型 | 规范 | 示例 |
|------|------|------|
| 类名 | PascalCase | `MainWindowService` |
| 接口名 | PascalCase + I 前缀 | `IPlatformService` |
| 方法名 | PascalCase | `ScanJavaPaths` |
| 私有字段 | _camelCase | `_javaPaths` |
| 常量 | PascalCase | `MaxRetryCount` |

### 8.2 代码格式化

项目使用 `.editorconfig` 定义格式化规则。

**自动格式化**：
```bash
dotnet format
```

### 8.3 提交规范

```
<type>(<scope>): <subject>

<body>

<footer>
```

示例：
```
feat(core): 添加平台抽象接口

- 新增 IPlatformService
- 新增 IJavaScanner
- 新增 IWindowService

Closes #123
```

---

## 9. 资源链接

- [.NET 10 文档](https://learn.microsoft.com/dotnet/)
- [Uno Platform 文档](https://platform.uno/docs/)
- [WinUI 3 文档](https://learn.microsoft.com/windows/apps/winui/)
- [PCL CE GitHub](https://github.com/PCL-Community/PCL-CE)

---

## 10. 联系方式

- GitHub Issues: [新建 Issue](https://github.com/PCL-Community/PCL-CE/issues/new)
- 讨论群: 599620549
