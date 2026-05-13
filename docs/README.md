# PCL-CE.Neo - 项目文档总览

## 项目概述

PCL-CE.Neo 是 [PCL Community Edition](https://github.com/PCL-Community/PCL-CE) 的跨平台重构版本，使用 .NET 10 和 Uno Platform 构建，支持 Windows、macOS 和 Linux 平台。

## 文档结构

```
docs/
├── README.md                    # 此文件
├── architecture/
│   ├── ARCHITECTURE.md         # 架构设计文档
│   ├── PLATFORM_ABSTRACTIONS.md # 平台抽象规范
│   └── PROJECT_STRUCTURE.md    # 项目结构说明
├── development/
│   ├── DEVELOPMENT.md          # 开发环境配置
│   ├── CONTRIBUTING.md         # 贡献指南
│   ├── CODING_STANDARDS.md     # 代码规范
│   └── GIT_WORKFLOW.md         # Git 工作流
├── guides/
│   ├── UI_MIGRATION_GUIDE.md   # UI 迁移指南
│   ├── PLATFORM_IMPL_GUIDE.md  # 平台实现指南
│   └── TESTING_GUIDE.md        # 测试指南
└── platforms/
    ├── WINDOWS.md              # Windows 平台特定文档
    ├── MACOS.md                # macOS 平台特定文档
    └── LINUX.md                # Linux 平台特定文档
```

## 快速开始

1. 阅读 [架构设计文档](architecture/ARCHITECTURE.md) 了解整体设计
2. 查看 [开发环境配置](development/DEVELOPMENT.md) 配置开发环境
3. 参考 [贡献指南](development/CONTRIBUTING.md) 开始贡献

## 项目阶段

- **阶段 0：准备与规划** ✅ 完成
- **阶段 1：架构准备** ✅ 完成
- **阶段 2：平台实现** 🔄 进行中
- **阶段 3：UI 迁移** ⏳ 待开始
- **阶段 4：测试与优化** ⏳ 待开始
- **阶段 5：发布与过渡** ⏳ 待开始

## 相关链接

- [重构计划书](重构计划书.md) - 详细的重构计划
- [项目仓库](https://github.com/PCL-Community/PCL-CE) - 原始项目
- [Uno Platform 文档](https://platform.uno/docs/) - UI 框架文档
- [.NET 10 文档](https://learn.microsoft.com/dotnet/) - .NET 文档
