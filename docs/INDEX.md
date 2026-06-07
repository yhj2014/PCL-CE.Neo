# 文档索引 (INDEX)

本文档提供 PCL-CE.Neo 项目所有文档的完整索引，方便快速查找所需信息。

---

## ⭐ 重要文档（必读）

### [重构计划书.md](./重构计划书.md) ⚠️ **最重要的文档**
> 这是整个项目的核心规划文档，包含了项目的目标、范围、技术决策、重构策略等关键信息。
> **强烈建议所有开发者首先阅读此文档**，以全面了解项目的背景和方向。

其他重要文档：
- [原项目架构书.md](./原项目架构书.md) - 原始 PCL 项目的架构设计
- [PROGRESS.md](./PROGRESS.md) - 开发进度记录
- [RUN_GUIDE.md](./RUN_GUIDE.md) - 如何运行编译后的应用程序（用户必读）

---

## 📚 架构文档 (architecture/)

### [ARCHITECTURE.md](./architecture/ARCHITECTURE.md)
项目整体架构设计文档，描述了系统的主要组件和它们之间的关系。

### [PLATFORM_ABSTRACTIONS.md](./architecture/PLATFORM_ABSTRACTIONS.md)
平台抽象层规范，定义了如何在跨平台场景下统一接口。

### [PROJECT_STRUCTURE.md](./architecture/PROJECT_STRUCTURE.md)
项目目录结构和文件组织说明。

---

## 🔧 开发指南 (development/)

### [DEVELOPMENT.md](./development/DEVELOPMENT.md)
开发环境配置指南，包括 SDK 安装、IDE 配置等。

### [CONTRIBUTING.md](./development/CONTRIBUTING.md)
贡献指南，说明如何参与项目贡献。

### [CODING_STANDARDS.md](./development/CODING_STANDARDS.md)
代码规范文档，包含命名约定、代码风格等要求。

### [GIT_WORKFLOW.md](./development/GIT_WORKFLOW.md)
Git 工作流规范，包括分支策略、提交规范等。

---

## 📖 指南文档 (guides/)

### [UI_MIGRATION_GUIDE.md](./guides/UI_MIGRATION_GUIDE.md)
UI 迁移指南，从传统 WPF 迁移到 Uno Platform 的指导。

### [PLATFORM_IMPL_GUIDE.md](./guides/PLATFORM_IMPL_GUIDE.md)
平台实现指南，如何为新平台实现抽象层。

### [TESTING_GUIDE.md](./guides/TESTING_GUIDE.md)
测试指南，包含单元测试、集成测试的编写规范。

---

## 💻 平台文档 (platforms/)

### [WINDOWS.md](./platforms/WINDOWS.md)
Windows 平台特定的配置和注意事项。

### [MACOS.md](./platforms/MACOS.md)
macOS 平台特定的配置和注意事项。

### [LINUX.md](./platforms/LINUX.md)
Linux 平台特定的配置和注意事项。

---

## 📊 根目录文档

### 架构与设计
- [ARCHITECTURE.md](./ARCHITECTURE.md) - 架构设计
- [PLATFORM_ABSTRACTIONS.md](./PLATFORM_ABSTRACTIONS.md) - 平台抽象

### 平台文档
- [LINUX.md](./LINUX.md) - Linux 平台信息
- [MACOS.md](./MACOS.md) - macOS 平台信息
- [WINDOWS.md](./WINDOWS.md) - Windows 平台信息

### 项目状态与报告
- [PROGRESS.md](./PROGRESS.md) - 开发进度记录
- [PHASE_1_COMPLETE.md](./PHASE_1_COMPLETE.md) - 阶段1完成报告
- [PHASE_1_2_COMPLETE.md](./PHASE_1_2_COMPLETE.md) - 阶段1-2完成报告
- [PHASES_REAL_STATUS_REPORT.md](./PHASES_REAL_STATUS_REPORT.md) - 各阶段实际状态报告
- [PLATFORM_PHASE_2_SUPPLEMENT_PLAN.md](./PLATFORM_PHASE_2_SUPPLEMENT_PLAN.md) - 平台阶段2补充计划

### 验证与质量报告
- [CODE_QUALITY_CHECKLIST.md](./CODE_QUALITY_CHECKLIST.md) - 代码质量检查清单
- [FINAL_VERIFICATION_REPORT.md](./FINAL_VERIFICATION_REPORT.md) - 最终验证报告
- [STRICT_VERIFICATION_REPORT.md](./STRICT_VERIFICATION_REPORT.md) - 严格验证报告
- [STRICT_FINAL_VERIFICATION_REPORT.md](./STRICT_FINAL_VERIFICATION_REPORT.md) - 严格最终验证报告
- [THOROUGH_REVIEW_REPORT.md](./THOROUGH_REVIEW_REPORT.md) - 彻底审查报告
- [THOROUGH_REVIEW_REPORT_20260514.md](./THOROUGH_REVIEW_REPORT_20260514.md) - 2026-05-14 审查报告
- [TEST_COVERAGE.md](./TEST_COVERAGE.md) - 测试覆盖率报告
- [PERFORMANCE_BENCHMARKS.md](./PERFORMANCE_BENCHMARKS.md) - 性能基准测试

### 历史文档
- [原项目架构书.md](./原项目架构书.md) - 原始 PCL 项目架构文档

---

## 🎯 推荐的阅读顺序

### 对于新加入的开发者：
1. **[重构计划书.md](./重构计划书.md)** ⭐ - 首先阅读，了解项目整体规划
2. **[ARCHITECTURE.md](./ARCHITECTURE.md)** - 了解系统架构
3. **[PROJECT_STRUCTURE.md](./architecture/PROJECT_STRUCTURE.md)** - 了解代码组织
4. **[DEVELOPMENT.md](./development/DEVELOPMENT.md)** - 配置开发环境
5. **[CODING_STANDARDS.md](./development/CODING_STANDARDS.md)** - 熟悉代码规范

### 对于平台开发者：
1. **[重构计划书.md](./重构计划书.md)** ⭐ - 了解重构目标
2. **[PLATFORM_ABSTRACTIONS.md](./architecture/PLATFORM_ABSTRACTIONS.md)** - 平台抽象规范
3. **[PLATFORM_IMPL_GUIDE.md](./guides/PLATFORM_IMPL_GUIDE.md)** - 平台实现指南
4. **对应平台文档** (WINDOWS.md/MACOS.md/LINUX.md)

### 对于 UI 开发：
1. **[重构计划书.md](./重构计划书.md)** ⭐ - 了解 UI 重构范围
2. **[UI_MIGRATION_GUIDE.md](./guides/UI_MIGRATION_GUIDE.md)** - UI 迁移指南
3. **[Uno Platform 文档](https://platform.uno/docs/)** - Uno 官方文档

---

## 🔍 快速查找

### 按主题查找：

| 主题 | 相关文档 |
|------|---------|
| 项目规划 | 重构计划书.md |
| 架构设计 | ARCHITECTURE.md, architecture/*.md |
| 代码规范 | CODING_STANDARDS.md |
| 测试 | TESTING_GUIDE.md, TEST_COVERAGE.md |
| 平台实现 | platforms/*.md, PLATFORM_IMPL_GUIDE.md |
| 开发环境 | DEVELOPMENT.md |
| 进度追踪 | PROGRESS.md, PHASES_REAL_STATUS_REPORT.md |

---

## 📝 更新日志

- 2026-06-07: 创建文档索引，添加分类和推荐阅读顺序

---

*最后更新：2026-06-07*
