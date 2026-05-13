# PCL-CE.Neo 架构设计文档

## 1. 概述

本文档描述 PCL-CE.Neo（基于 PCL Community Edition 的跨平台重构项目）的架构设计。

### 1.1 目标
将 PCL Community Edition 从仅支持 Windows 的 WPF 应用，重构为基于 .NET 10 和 Uno Platform 的跨平台应用，支持 Windows、macOS 和 Linux。

### 1.2 项目背景
PCL-CE.Neo 是 PCL Community Edition 的跨平台重构项目，由社区开发者发起，旨在为非 Windows 用户提供 PCL 启动器的使用体验。

### 1.2 设计原则
- **最小接口**：只抽象真正需要跨平台的功能
- **Windows 优先**：先确保 Windows 实现与原有行为一致
- **可扩展**：为后续平台预留扩展点
- **渐进式迁移**：保持现有功能稳定，逐步迁移

---

## 2. 架构概览

```
┌─────────────────────────────────────────────────────────────┐
│                        应用层 (PCL.App)                      │
│              Windows / macOS / Linux 特定入口               │
├─────────────────────────────────────────────────────────────┤
│                      UI 层 (PCL.UI)                         │
│        Uno Platform (WinUI 3) - 跨平台 UI 组件              │
├─────────────────────────────────────────────────────────────┤
│                   平台抽象层 (PCL.Core.Abstractions)        │
│     IPlatformService • IWindowService • IJavaScanner 等     │
├─────────────────────────────────────────────────────────────┤
│                      核心层 (PCL.Core)                      │
│     配置 • 网络 • 日志 • Minecraft 逻辑 • 任务管理           │
├─────────────────────────────────────────────────────────────┤
│                   平台实现层 (PCL.Platform)                 │
│           Windows • macOS • Linux 特定实现                  │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. 项目结构

```
PCL-CE.Neo/
├── PCL-CE.Neo.Core/                    # 核心业务逻辑库
│   ├── App/                           # 应用服务
│   │   ├── Configuration/            # 配置管理
│   │   ├── Database/                  # 数据库
│   │   ├── Essentials/                # 核心服务（已重构）
│   │   ├── IoC/                       # 依赖注入
│   │   └── Tasks/                     # 任务管理
│   ├── IO/                            # IO 操作
│   ├── Link/                          # 联机功能
│   ├── Logging/                       # 日志系统
│   ├── Minecraft/                    # Minecraft 核心
│   │   └── Java/                      # Java 相关
│   └── Utils/                         # 工具类
│
├── PCL-CE.Neo.Core.Abstractions/       # 平台抽象接口
│   ├── IPlatformService.cs            # 平台服务接口
│   ├── IWindowService.cs             # 窗口服务接口
│   ├── IJavaScanner.cs               # Java 扫描器接口
│   ├── IThemeService.cs              # 主题服务接口
│   ├── IAudioService.cs              # 音频服务接口
│   ├── IClipboardService.cs          # 剪贴板服务接口
│   └── IDialogService.cs             # 对话框服务接口
│
├── PCL-CE.Neo.Platform/               # 平台实现
│   ├── Windows/                      # Windows 实现
│   ├── MacOS/                        # macOS 实现
│   └── Linux/                        # Linux 实现
│
├── PCL-CE.Neo.UI/                     # Uno Platform UI
│   ├── Controls/                     # 自定义控件
│   ├── Pages/                        # 页面
│   ├── Themes/                       # 主题
│   └── Animations/                   # 动画
│
├── PCL-CE.Neo.App/                    # 应用入口
│   └── Platforms/                   # 各平台入口
│
└── Plain Craft Launcher 2/            # 【原 PCL CE WPF 版本，作为参考】
```

---

## 4. 核心组件

### 4.1 PCL.Core - 核心业务逻辑

**职责**：
- 业务逻辑实现
- 配置管理
- 网络通信
- 日志记录
- Minecraft 版本管理
- Mod 下载和安装

**特点**：
- 无 UI 依赖
- 无平台特定代码
- 目标框架：`net10.0;netstandard2.1`

### 4.2 PCL.Core.Abstractions - 平台抽象

**职责**：
- 定义平台无关的接口
- 抽象系统级功能

**主要接口**：

| 接口 | 描述 |
|------|------|
| `IPlatformService` | 平台基本信息和服务 |
| `IWindowService` | 窗口管理 |
| `IJavaScanner` | Java 运行时检测 |
| `IThemeService` | 主题管理 |
| `IAudioService` | 音频播放 |
| `IClipboardService` | 剪贴板操作 |
| `IDialogService` | 系统对话框 |

### 4.3 PCL.Platform - 平台实现

**职责**：
- 实现平台抽象接口
- 提供平台特定功能

**项目结构**：
```
PCL.Platform/
├── Windows/            # Windows 特定实现
├── MacOS/              # macOS 特定实现
└── Linux/              # Linux 特定实现
```

### 4.4 PCL.UI - 跨平台 UI

**职责**：
- Uno Platform UI 实现
- 像素级还原原有 UI
- 跨平台控件和页面

**特点**：
- 使用 WinUI 3 / Uno Platform
- 保持与原 WPF UI 一致的外观
- 支持 Windows/macOS/Linux

---

## 5. 依赖关系

```
Plain Craft Launcher 2 (WPF)
    └── PCL.Core
        └── [无依赖]
```
PCL-CE.Neo.App (跨平台)
    ├── PCL-CE.Neo.UI
    │   └── PCL-CE.Neo.Core.Abstractions
    ├── PCL-CE.Neo.Platform.Windows (或 MacOS/Linux)
    │   └── PCL-CE.Neo.Core.Abstractions
    └── PCL-CE.Neo.Core
        └── PCL-CE.Neo.Core.Abstractions
```

---

## 6. 关键设计决策

### 6.1 为什么选择 Uno Platform？

| 优势 | 说明 |
|------|------|
| WPF 兼容性 | 与现有 WPF 代码高度兼容 |
| 跨平台 | 一套代码支持 Windows/macOS/Linux/Web |
| WinUI 3 基础 | 使用微软最新的 UI 框架 |
| 活跃社区 | 成熟的社区支持和文档 |

### 6.2 平台抽象策略

- **最小抽象**：只抽象必要的系统功能
- **接口优先**：使用接口定义契约
- **默认实现**：提供合理的默认行为

### 6.3 UI 还原策略

- **像素级还原**：保持原有布局、颜色、间距完全一致
- **资源提取**：将所有 UI 资源标准化为常量
- **自动化测试**：使用截图对比确保还原度

---

## 7. 扩展计划

### 7.1 平台支持
- [x] Windows (首发)
- [ ] macOS (阶段 2)
- [ ] Linux (阶段 2)
- [ ] Web/WASM (后续)

### 7.2 功能扩展
- [ ] 云同步配置
- [ ] 原生通知
- [ ] 各平台原生集成

---

## 8. 文档索引

- [平台抽象规范](../PLATFORM_ABSTRACTIONS.md)
- [UI 迁移指南](../UI_MIGRATION_GUIDE.md)
- [开发环境配置](../DEVELOPMENT.md)
