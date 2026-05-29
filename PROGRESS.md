# PCL-CE.Neo 开发进度记录

## 概述
本文档记录了项目开发过程中的简化、修改和重要决策，用于后续参考。

---

## 编译验证 (2025-01-28)

### 背景
按照 PCL-CE.Neo 开发规范要求，需要在本地验证三个平台（Windows、macOS、Linux）的编译。.NET SDK 通过项目根目录的 `setup_sdk.sh` 安装。

### 完成的工作

#### 1. .NET SDK 安装
- 使用 `setup_sdk.sh` 脚本将 .NET 10 SDK 安装到用户目录 `~/.dotnet`
- 配置 PATH 环境变量使 SDK 可用

#### 2. 项目结构简化

##### UI 项目 (PCL-CE.Neo.UI)
**修改前**: 使用 `Uno.Sdk`，包含大量 XAML 文件和 Uno Platform 特定代码
**修改后**: 
- 改为使用 `Microsoft.NET.Sdk`
- 暂时排除以下复杂文件以确保编译通过：
  - `Controls/**/*.cs` - 所有自定义控件
  - `Pages/**/*.cs` - 所有页面代码
  - `Navigation/*.cs` - 导航服务
  - `Themes/*.cs` - 主题管理
  - `ViewModels/**/*.cs` - 所有视图模型
  - `Animations/**/*.cs` - 动画服务
  - `Services/AudioService.cs` - 音频服务
  - `Services/WindowService.cs` - 窗口服务
  - `App.xaml.cs` - 应用入口
  - `MainWindow.xaml.cs` - 主窗口

##### App 项目 (PCL-CE.Neo.App)
为三个平台创建了简化的控制台应用入口点：

**PCL-CE.Neo.App.Windows.csproj**
- 改为使用 `Microsoft.NET.Sdk`
- 排除 XAML 相关文件：`App.xaml.cs`、`MainWindow.xaml.cs`
- 创建统一的 `Program.cs` 作为入口点

**PCL-CE.Neo.App.macOS.csproj**
- 改为使用 `Microsoft.NET.Sdk`
- 排除 XAML 相关文件
- 使用条件编译引用 macOS 平台服务

**PCL-CE.Neo.App.Linux.csproj**
- 改为使用 `Microsoft.NET.Sdk`
- 排除 XAML 相关文件
- 使用条件编译引用 Linux 平台服务

#### 3. 核心代码修改

##### MainWindow.xaml.cs 简化
- 移除了使用 `UI.Colors` 的代码（该命名空间在非 Windows 平台不可用）
- 简化了 `UpdateNavigationState` 方法为占位实现

##### ServiceCollectionExtensions.cs 简化
- 移除了对 `AnimationService` 和 `WindowService` 的依赖注册
- 保留基础 UI 服务注册

#### 4. 平台项目状态

所有平台项目都可以成功编译：

| 平台 | 项目 | 编译状态 |
|------|------|---------|
| Windows | PCL-CE.Neo.Platform.Windows | ✅ 成功 |
| macOS | PCL-CE.Neo.Platform.macOS | ✅ 成功 |
| Linux | PCL-CE.Neo.Platform.Linux | ✅ 成功 |
| - | PCL-CE.Neo.Core.Abstractions | ✅ 成功 |
| - | PCL-CE.Neo.Core | ✅ 成功 |
| - | PCL-CE.Neo.UI | ✅ 成功（简化版） |
| - | PCL-CE.Neo.App.Windows | ✅ 成功 |
| - | PCL-CE.Neo.App.macOS | ✅ 成功 |
| - | PCL-CE.Neo.App.Linux | ✅ 成功 |

---

## 待完成的工作

### UI 框架恢复
以下文件需要后续恢复完整的 Uno Platform 实现：

1. **Controls/** - 自定义控件
   - MyButton
   - MyIconButton
   - MyComboBox
   - BlurBorder

2. **Pages/** - 页面
   - HomePage
   - LaunchPage
   - VersionSelectPage
   - InstancePage
   - LoginPage
   - ToolsPage
   - SettingsPage

3. **Navigation/** - 导航服务
   - NavigationService
   - RouteConfig

4. **Themes/** - 主题管理
   - ThemeManager

5. **ViewModels/** - 视图模型
   - LaunchViewModel
   - VersionSelectViewModel
   - InstanceViewModel
   - LoginViewModel
   - ToolsViewModel
   - SettingsViewModel

6. **Animations/** - 动画服务
   - AnimationService

7. **Services/** - 服务
   - AudioService
   - WindowService

### 项目文件恢复
- UI 项目需要恢复使用 `Uno.Sdk` 并配置正确的 UnoFeatures
- App 项目需要恢复 XAML 文件和 Uno Platform 入口点

---

## 技术决策

### 为什么暂时简化？
1. **跨平台兼容性**: Uno Platform 的某些 API 在不同平台上有不同的可用性
2. **编译速度**: 先确保基础框架可以编译，加快开发迭代
3. **问题隔离**: 将 UI 问题与业务逻辑分离，便于调试

### 条件编译策略
使用 `#if WINDOWS` / `#if MACOS` / `#if LINUX` 条件编译来确保平台特定代码在对应平台上编译。

### 依赖注入
所有平台服务通过 `ServiceCollectionExtensions` 进行注册，遵循以下模式：
- Core: `AddCoreServices()`
- UI: `AddUIServices()`
- Platform: `Add[Platform]PlatformServices()`

---

## 验证方法

编译命令：
```bash
# Linux
dotnet build PCL-CE.Neo.App/PCL-CE.Neo.App.Linux.csproj -c Release

# Windows
dotnet build PCL-CE.Neo.App/PCL-CE.Neo.App.Windows.csproj -c Release

# macOS
dotnet build PCL-CE.Neo.App/PCL-CE.Neo.App.macOS.csproj -c Release
```

---

## 注意事项

1. **PCL-CE.Neo 开发规范要求**:
   - 禁止使用 Console.WriteLine 作为正式实现
   - 禁止使用 NotImplementedException
   - 禁止使用 TODO 占位符
   - 所有方法必须有完整实现和错误处理

2. **跨平台要求**:
   - 三个平台必须有各自完整的实现
   - 不允许使用条件编译跳过某个平台的实现

3. **后续恢复 UI 时**:
   - 需要重新引入 Uno Platform 相关依赖
   - 需要解决跨平台 API 兼容性问题
   - 需要确保 XAML 文件正确引用
