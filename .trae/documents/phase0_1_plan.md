# PCL CE 跨平台重构 - 阶段 0 和阶段 1 实施计划

## 仓库研究结论

通过对当前仓库的分析，我确认以下关键信息：

### 当前项目状态
- **技术栈**：.NET 8 + WPF，仅支持 Windows 平台
- **项目结构**：
  - [PCL.Core](file:///workspace/PCL.Core) - 核心库（包含 WPF 依赖）
  - [Plain Craft Launcher 2](file:///workspace/Plain%20Craft%20Launcher%202) - WPF 主应用
  - [PCL.Core.SourceGenerators](file:///workspace/PCL.Core.SourceGenerators) - 代码生成器
  - [PCL.Core.Test](file:///workspace/PCL.Core.Test) - 测试项目
- **CI/CD**：已有 GitHub Actions 工作流（[build-test.yml](file:///workspace/.github/workflows/build-test.yml)）
- **平台依赖**：大量 Windows 特定代码（[WindowInterop.cs](file:///workspace/PCL.Core/Utils/OS/WindowInterop.cs)、[RegistryJavaScanner.cs](file:///workspace/PCL.Core/Minecraft/Java/Scanner/RegistryJavaScanner.cs) 等）

### UI 还原要求确认
用户明确要求：**UI 需要像素级还原** - 这意味着在迁移到 Uno Platform 时，必须保持：
- 所有控件的布局、间距、尺寸完全一致
- 颜色、字体、样式精确匹配
- 动画效果保持一致
- 自定义控件的行为和外观完全相同

---

## 阶段 0：准备与规划

### 目标
建立项目基础设施，确保开发环境就绪，为重构奠定基础。

### 任务清单

#### 1. 设置分支策略
- 创建 `feature/crossplatform-phase1` 分支作为工作分支
- 创建分支保护规则
- 更新 `.github/` 相关配置

#### 2. 创建架构文档
- 在根目录创建 `ARCHITECTURE.md` - 架构设计文档
- 创建 `PLATFORM_ABSTRACTIONS.md` - 平台抽象规范
- 创建 `UI_MIGRATION_GUIDE.md` - UI 迁移指南（重点说明像素级还原要求）
- 创建 `DEVELOPMENT.md` - 开发环境指南

#### 3. 更新 CI/CD 配置
- 创建新的 GitHub Actions 工作流用于跨平台构建
- 在原工作流中添加新分支的支持

---

## 阶段 1：架构准备

### 目标
建立平台抽象层，将 PCL.Core 升级到 .NET 10 并移除 WPF 依赖，重构核心业务逻辑。

### 任务清单

#### Part 1：项目重组与 .NET 10 升级

##### 1.1 创建新项目（直接在根目录）
在根目录下创建以下新项目：
- `PCL.Core.Abstractions/` - 平台抽象接口
- `PCL.Platform/` - 平台实现
  - `Windows/` - Windows 平台实现
  - `MacOS/` - macOS 平台实现（占位）
  - `Linux/` - Linux 平台实现（占位）
- `PCL.UI/` - Uno Platform UI 项目
- `PCL.App/` - 应用入口项目

##### 1.2 升级 PCL.Core 到 .NET 10
- 修改 [PCL.Core.csproj](file:///workspace/PCL.Core/PCL.Core.csproj)：
  - 支持多目标：`net10.0;netstandard2.1`
  - 移除 `UseWPF` 和 `EnableWindowsTargeting`
  - 更新所有 NuGet 包到 .NET 10 兼容版本
  - 移除 Windows 特定包（`System.Management`、`Microsoft.Xaml.Behaviors.Wpf` 等）到平台项目
  - 解决 API 变更问题

##### 1.3 代码分离 - 迁移非 UI 代码
在 PCL.Core 内部进行代码分离：
- ✅ 保留：`App/Configuration/` - 配置系统（无 WPF 依赖）
- ✅ 保留：`App/Database/` - 数据库服务
- ✅ 保留：`App/IoC/` - 依赖注入（需调整）
- ✅ 保留：`App/Tasks/` - 任务管理
- ✅ 保留：`IO/` - IO 操作
- ✅ 保留：`Link/` - 联机功能（大部分可移植）
- ✅ 保留：`Logging/` - 日志系统
- ✅ 保留：`Minecraft/` - Minecraft 相关（除 Java 扫描器）
- ✅ 保留：`Utils/` - 工具类（除 OS/Windows 特定部分）

**重构/移动**：
- `UI/` - 全部 UI 相关（后续迁移到 PCL.UI）
- `Utils/OS/` - 操作系统相关（移到平台抽象）
- `App/Essentials/` - 需重构的应用服务

#### Part 2：定义平台抽象接口

##### 2.1 创建平台抽象项目
创建 `PCL.Core.Abstractions/PCL.Core.Abstractions.csproj`

##### 2.2 定义核心接口

**IPlatformService.cs** - 基础平台服务
```csharp
namespace PCL.Core.Abstractions;

public interface IPlatformService
{
    string PlatformName { get; }
    string OSVersion { get; }
    string Architecture { get; }
    void OpenUrl(string url);
    void OpenFolder(string path);
    string GetLocalApplicationDataPath();
    string GetTempPath();
}
```

**IWindowService.cs** - 窗口管理
```csharp
namespace PCL.Core.Abstractions;

public interface IWindowService
{
    void Initialize();
    void ShowMainWindow();
    void CloseMainWindow();
    // 更多窗口相关方法
}
```

**IJavaScanner.cs** - Java 检测
```csharp
namespace PCL.Core.Abstractions;

public interface IJavaScanner
{
    IEnumerable<string> ScanJavaPaths();
}
```

**IThemeService.cs** - 主题服务
```csharp
namespace PCL.Core.Abstractions;

public interface IThemeService
{
    event EventHandler ThemeChanged;
    object GetCurrentTheme();
    void SetTheme(object theme);
}
```

**IUIAccessProvider.cs** - UI 访问抽象
```csharp
namespace PCL.Core.Abstractions;

public interface IUIAccessProvider
{
    // 抽象原 WpfUIAccessProvider 的功能
}
```

**IAudioService.cs** - 音频播放（后续需要）
```csharp
namespace PCL.Core.Abstractions;

public interface IAudioService
{
    void Play(string path);
    void Stop();
}
```

**IClipboardService.cs** - 剪贴板
```csharp
namespace PCL.Core.Abstractions;

public interface IClipboardService
{
    string GetText();
    void SetText(string text);
}
```

**IDialogService.cs** - 系统对话框
```csharp
namespace PCL.Core.Abstractions;

public interface IDialogService
{
    string ShowOpenFileDialog(string filter);
    string ShowSaveFileDialog(string filter, string defaultName);
    string ShowOpenFolderDialog();
}
```

##### 2.3 调整生命周期服务
重构 `App/IoC/Lifecycle.cs` 和相关服务：
- 移除 WPF 特定的生命周期钩子
- 添加平台抽象的初始化
- 保持生命周期流程逻辑不变

#### Part 3：重构核心业务逻辑

##### 3.1 重构 ApplicationService
重构 [ApplicationService.cs](file:///workspace/PCL.Core/App/Essentials/ApplicationService.cs)：
- 移除对 `System.Windows.Application` 的直接依赖
- 使用 `IWindowService` 抽象
- 保持生命周期事件机制

##### 3.2 重构 MainWindowService
重构 [MainWindowService.cs](file:///workspace/PCL.Core/App/Essentials/MainWindowService.cs)：
- 移除 WPF Window 依赖
- 通过 `IWindowService` 进行窗口操作

##### 3.3 重构 Java 检测系统
- 将原 Java 扫描器抽象为 `IJavaScanner` 接口
- 将 [RegistryJavaScanner.cs](file:///workspace/PCL.Core/Minecraft/Java/Scanner/RegistryJavaScanner.cs) 移到 Windows 平台实现
- 其他扫描器（Path、WhereCommand 等）保留在核心但抽象化

##### 3.4 分离 UI 相关工具类
- 将 `Utils/WpfUtils.cs` 移到 PCL.UI 项目
- 将 `Utils/Exts/UiExtension.cs` 移到 PCL.UI 项目
- 将 `UI/` 整个目录标记为待迁移

##### 3.5 调整动画系统
- 暂时保留动画系统代码但不编译
- 创建动画接口定义供后续 UI 项目使用
- 标记为「UI 依赖，待迁移」

##### 3.6 添加单元测试
- 为平台抽象接口创建测试（更新 PCL.Core.Test）
- 为重构后的核心服务添加测试
- 目标：核心逻辑测试覆盖率 ≥ 80%

#### Part 4：创建 Windows 平台实现

##### 4.1 创建 Windows 平台项目
`PCL.Platform/Windows/PCL.Platform.Windows.csproj`

##### 4.2 实现平台服务
- **WindowsPlatformService** - 实现 `IPlatformService`
- **WindowsWindowService** - 实现 `IWindowService`
- **WindowsJavaScanner** - 移植原注册表扫描器
- **WindowsClipboardService** - Windows 剪贴板实现
- **WindowsDialogService** - Windows 对话框实现

##### 4.3 依赖注入配置
创建平台特定的 DI 配置：
```csharp
public static class WindowsServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlatformService, WindowsPlatformService>();
        services.AddSingleton<IWindowService, WindowsWindowService>();
        services.AddSingleton<IJavaScanner, WindowsJavaScanner>();
        // ...
        return services;
    }
}
```

#### Part 5：更新解决方案文件

##### 5.1 更新解决方案
更新 [Plain Craft Launcher 2.slnx](file:///workspace/Plain%20Craft%20Launcher%202.slnx) 添加新项目

---

## 关键技术细节

### 像素级还原准备
为了确保后续 UI 迁移时能够像素级还原，在此阶段需要：

1. **UI 资源提取** - 提取所有颜色、尺寸、字体资源为常量
2. **截图记录** - 为所有主要界面创建像素级截图作为参考
3. **控件规范文档** - 记录每个自定义控件的精确规范

### PCL.Core 重构策略
采用**渐进式重构**：
1. 直接在 PCL.Core 上修改
2. 使用条件编译控制平台特定代码
3. 逐步迁移功能

### 平台抽象设计原则
- **最小接口** - 只抽象真正需要跨平台的功能
- **Windows 优先** - 先确保 Windows 实现与原有行为一致
- **可扩展** - 为后续 macOS/Linux 预留扩展点

---

## 文件和模块修改清单

### 新增文件（创建）

```
PCL.Core.Abstractions/
├── PCL.Core.Abstractions.csproj
├── IPlatformService.cs
├── IWindowService.cs
├── IJavaScanner.cs
├── IThemeService.cs
├── IUIAccessProvider.cs
├── IAudioService.cs
├── IClipboardService.cs
└── IDialogService.cs

PCL.Platform/
└── Windows/
    ├── PCL.Platform.Windows.csproj
    ├── WindowsPlatformService.cs
    ├── WindowsWindowService.cs
    ├── WindowsJavaScanner.cs
    ├── WindowsClipboardService.cs
    └── WindowsDialogService.cs

PCL.UI/
└── PCL.UI.csproj (基础 Uno 项目)

PCL.App/
└── PCL.App.csproj

ARCHITECTURE.md
PLATFORM_ABSTRACTIONS.md
UI_MIGRATION_GUIDE.md
DEVELOPMENT.md
```

### 修改文件（编辑）

```
PCL.Core/
├── PCL.Core.csproj (升级到 .NET 10，移除 WPF)
├── App/Essentials/
│   ├── ApplicationService.cs (重构)
│   └── MainWindowService.cs (重构)
└── Minecraft/Java/Scanner/
    └── RegistryJavaScanner.cs (移到平台项目)

Plain Craft Launcher 2.slnx (添加新项目)

.github/workflows/
└── build-test.yml (更新)
```

---

## 风险评估与应对

| 风险 | 可能性 | 影响 | 应对措施 |
|------|--------|------|----------|
| 核心逻辑重构引入 bug | 高 | 高 | 充分单元测试 + 新旧版本对比测试 |
| .NET 10 兼容性问题 | 中 | 中 | 提前验证，保留 .NET 8 作为回退选项 |
| 平台抽象设计不完善 | 中 | 高 | 分阶段验证，快速迭代调整 |
| UI 还原难度超预期 | 高 | 中 | 在此阶段做好详细记录，为阶段 3 奠定基础 |

---

## 实施顺序（按优先级）

### 第一优先级（必须先做）
1. 创建平台抽象接口项目
2. 升级 PCL.Core 项目文件（移除 WPF 依赖）
3. 重构 ApplicationService 和 MainWindowService

### 第二优先级（重要）
4. 调整 PCL.Core 内部代码结构
5. 实现 Windows 平台服务
6. 添加单元测试

### 第三优先级（完善）
7. 创建文档
8. 更新 CI/CD
9. 创建 UI 项目基础结构
10. 更新解决方案

---

## 验收标准

### 阶段 0 验收
- [ ] 分支策略设置完成
- [ ] 架构文档创建完成
- [ ] CI/CD 配置更新完成

### 阶段 1 验收
- [ ] PCL.Core 可在 .NET 10 下编译通过（无 WPF 依赖）
- [ ] 所有平台抽象接口定义完成
- [ ] Windows 平台实现完成
- [ ] 核心服务重构完成
- [ ] 单元测试覆盖率 ≥ 80%
- [ ] 解决方案更新完成
- [ ] 像素级还原准备工作完成（资源提取、截图记录）

---

## 后续衔接

完成阶段 0 和 1 后，将进入：
- **阶段 2**：平台实现（macOS/Linux）
- **阶段 3**：UI 迁移（重点：像素级还原）

本阶段完成后，将为 UI 迁移奠定坚实基础。
