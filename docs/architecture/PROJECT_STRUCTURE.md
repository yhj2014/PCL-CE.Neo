# 项目结构说明

## 目录结构

```
PCL-CE.Neo/
├── PCL-CE.Neo.Core/                    # 核心业务逻辑库
│   ├── Abstractions/                   # 核心业务抽象接口
│   │   ├── IApplicationAdapter.cs
│   │   ├── IAuthAdapter.cs
│   │   ├── IConfigAdapter.cs
│   │   ├── IDatabaseAdapter.cs
│   │   ├── IDownloadAdapter.cs
│   │   ├── IInstanceAdapter.cs
│   │   ├── ILifecycleBridge.cs
│   │   ├── ILinkAdapter.cs
│   │   ├── ILoggerAdapter.cs
│   │   ├── IMinecraftAdapter.cs
│   │   ├── IModAdapter.cs
│   │   ├── INetworkAdapter.cs
│   │   ├── IPathsAdapter.cs
│   │   ├── IStateAdapter.cs
│   │   ├── ITaskAdapter.cs
│   │   └── ITelemetryAdapter.cs
│   ├── Adapters/                       # 核心适配器实现
│   │   ├── ApplicationAdapter.cs
│   │   ├── AuthAdapter.cs
│   │   ├── ConfigAdapter.cs
│   │   ├── DatabaseAdapter.cs
│   │   ├── DownloadAdapter.cs
│   │   ├── InstanceAdapter.cs
│   │   ├── LifecycleBridge.cs
│   │   ├── LinkAdapter.cs
│   │   ├── LoggerAdapter.cs
│   │   ├── MinecraftAdapter.cs
│   │   ├── ModAdapter.cs
│   │   ├── NetworkAdapter.cs
│   │   ├── PathsAdapter.cs
│   │   ├── ResourceDownloadAdapter.cs
│   │   ├── StateAdapter.cs
│   │   ├── TaskAdapter.cs
│   │   └── TelemetryAdapter.cs
│   ├── PlatformDetector.cs             # 平台检测与服务加载
│   ├── ServiceLocator.cs               # 服务定位器
│   ├── ServiceBuilder.cs               # 服务构建器
│   └── PCL-CE.Neo.Core.csproj
│
├── PCL-CE.Neo.Core.Abstractions/       # 平台抽象接口
│   ├── IPlatformService.cs
│   ├── IWindowService.cs
│   ├── IJavaScanner.cs
│   ├── IThemeService.cs
│   ├── IAudioService.cs
│   ├── IClipboardService.cs
│   ├── IDialogService.cs
│   ├── INotificationService.cs
│   ├── IUIAccessProvider.cs
│   └── PCL-CE.Neo.Core.Abstractions.csproj
│
├── PCL-CE.Neo.Platform/                # 平台实现层
│   ├── Windows/                       # Windows 平台实现
│   │   ├── WindowsPlatformService.cs
│   │   ├── WindowsWindowService.cs
│   │   ├── WindowsJavaScanner.cs
│   │   ├── WindowsThemeService.cs
│   │   ├── WindowsClipboardService.cs
│   │   ├── WindowsDialogService.cs
│   │   ├── ServiceCollectionExtensions.cs
│   │   └── PCL-CE.Neo.Platform.Windows.csproj
│   ├── macOS/                         # macOS 平台实现
│   │   ├── MacOSPlatformService.cs
│   │   ├── MacOSWindowService.cs
│   │   ├── MacOSJavaScanner.cs
│   │   ├── MacOSThemeService.cs
│   │   ├── MacOSClipboardService.cs
│   │   ├── MacOSDialogService.cs
│   │   ├── ServiceCollectionExtensions.cs
│   │   └── PCL-CE.Neo.Platform.macOS.csproj
│   └── Linux/                         # Linux 平台实现
│       ├── LinuxPlatformService.cs
│       ├── LinuxWindowService.cs
│       ├── LinuxJavaScanner.cs
│       ├── LinuxThemeService.cs
│       ├── LinuxClipboardService.cs
│       ├── LinuxDialogService.cs
│       ├── ServiceCollectionExtensions.cs
│       └── PCL-CE.Neo.Platform.Linux.csproj
│
├── PCL-CE.Neo.UI/                      # Uno Platform UI 层
│   ├── Resources/                     # UI 资源
│   │   ├── Colors.xaml
│   │   ├── ControlStyles.xaml
│   │   ├── Dimensions.xaml
│   │   └── TextStyles.xaml
│   ├── App.xaml
│   └── PCL-CE.Neo.UI.csproj
│
├── PCL-CE.Neo.App/                     # 应用入口与平台特定代码
│   ├── Platforms/
│   │   ├── Windows/
│   │   ├── macOS/
│   │   └── Linux/
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   └── PCL-CE.Neo.App.csproj
│
├── PCL-CE.Neo.Tests/                   # 单元测试项目
│   ├── PlatformAbstractions/
│   │   └── PlatformServiceTests.cs
│   ├── AuthAdapterTests.cs
│   ├── ConfigAdapterTests.cs
│   ├── DownloadAdapterTests.cs
│   ├── InstanceAdapterTests.cs
│   ├── MinecraftAdapterTests.cs
│   ├── ServiceLocatorTests.cs
│   ├── TaskAdapterTests.cs
│   ├── TestLogger.cs
│   └── PCL-CE.Neo.Tests.csproj
│
├── PCL.Core/                           # 【参考】原始 PCL 核心库
├── PCL.Core.SourceGenerators/          # 【参考】原始代码生成器
├── PCL.Core.Test/                      # 【参考】原始测试项目
├── Plain Craft Launcher 2/             # 【参考】原始 WPF 应用
│
├── docs/                               # 项目文档
│   ├── README.md                       # 文档总览
│   ├── 重构计划书.md                   # 重构详细计划
│   ├── 原项目架构书.md                 # 原项目架构分析
│   ├── architecture/                   # 架构相关文档
│   │   ├── ARCHITECTURE.md             # 架构设计文档
│   │   ├── PLATFORM_ABSTRACTIONS.md    # 平台抽象规范
│   │   └── PROJECT_STRUCTURE.md        # 此文件
│   ├── development/                    # 开发相关文档
│   │   ├── DEVELOPMENT.md              # 开发环境配置
│   │   ├── CONTRIBUTING.md             # 贡献指南
│   │   ├── CODING_STANDARDS.md         # 代码规范
│   │   └── GIT_WORKFLOW.md             # Git 工作流
│   ├── guides/                         # 指南文档
│   │   ├── UI_MIGRATION_GUIDE.md       # UI 迁移指南
│   │   ├── PLATFORM_IMPL_GUIDE.md      # 平台实现指南
│   │   └── TESTING_GUIDE.md            # 测试指南
│   └── platforms/                      # 平台特定文档
│       ├── WINDOWS.md
│       ├── MACOS.md
│       └── LINUX.md
│
├── README.md                           # 项目说明
├── CONTRIBUTING.md                     # 原始贡献指南
└── Plain Craft Launcher 2.slnx         # 解决方案文件
```

## 架构层次

### 1. 平台实现层 (PCL-CE.Neo.Platform.*)
- 提供平台特定功能的具体实现
- 实现 PCL-CE.Neo.Core.Abstractions 中的接口
- 每个平台独立项目

### 2. 平台抽象层 (PCL-CE.Neo.Core.Abstractions)
- 定义跨平台接口
- 隐藏平台差异
- 无平台特定依赖

### 3. 核心业务层 (PCL-CE.Neo.Core)
- 实现所有业务逻辑
- 依赖平台抽象层
- 无 UI 依赖
- 无平台特定依赖

### 4. UI 层 (PCL-CE.Neo.UI)
- Uno Platform 跨平台 UI
- 像素级还原原有设计
- 可在所有平台运行

### 5. 应用层 (PCL-CE.Neo.App)
- 平台特定入口
- DI 容器配置
- 平台特定初始化

## 依赖关系

```
PCL-CE.Neo.App
  ├── PCL-CE.Neo.UI
  │   └── PCL-CE.Neo.Core.Abstractions
  ├── PCL-CE.Neo.Platform.*
  │   └── PCL-CE.Neo.Core.Abstractions
  └── PCL-CE.Neo.Core
      └── PCL-CE.Neo.Core.Abstractions
```
