# 平台实现待办清单

## 项目概述
PCL-CE.Neo 是一个跨平台 Minecraft 启动器，使用 .NET 10 SDK 和 Uno Platform 实现。
- **当前阶段**：Phase 1-2（基础框架和适配器层）
- **目标**：三个平台都能编译通过并启动到主界面

---

## 架构说明

### 平台分层
1. **PCL-CE.Neo.Core.Abstractions** - 纯业务逻辑接口（非UI）
2. **PCL-CE.Neo.Core** - 业务逻辑实现（非UI）
3. **PCL-CE.Neo.Platform** - 平台特定非UI功能
4. **PCL-CE.Neo.UI (Uno Platform)** - UI层，处理所有UI相关功能

### 平台服务层保留的接口
- `IJavaScanner` - Java扫描（跨平台通用实现）
- `IPlatformService` - 平台信息和系统操作（跨平台通用实现）

### UI相关接口（已移除平台实现）
以下接口的实现在Uno Platform UI层：
- `IAnimationService` - 动画服务
- `IAudioService` - 音频服务
- `IClipboardService` - 剪贴板服务
- `IDialogService` - 对话框服务
- `INotificationService` - 通知服务
- `IThemeService` - 主题服务
- `IUIAccessProvider` - UI线程访问
- `IWindowService` - 窗口管理

---

## 平台实现状态

### ✅ Windows 平台 - 已实现
**文件路径**：`/workspace/PCL-CE.Neo.Platform/Windows/`

#### 已完成功能
- [x] `WindowsJavaScanner.cs` - Java扫描（跨平台通用实现）
- [x] `WindowsPlatformService.cs` - 平台信息和系统操作（跨平台通用实现）

**最小验证场景**：✅ 可以启动到主界面

---

### ✅ macOS 平台 - 已实现
**文件路径**：`/workspace/PCL-CE.Neo.Platform/macOS/`

#### 已完成功能
- [x] `MacOSJavaScanner.cs` - Java扫描（跨平台通用实现）
- [x] `MacOSPlatformService.cs` - 平台信息和系统操作（跨平台通用实现）

**最小验证场景**：✅ 可以启动到主界面

---

### ✅ Linux 平台 - 已实现
**文件路径**：`/workspace/PCL-CE.Neo.Platform/Linux/`

#### 已完成功能
- [x] `LinuxJavaScanner.cs` - Java扫描（跨平台通用实现）
- [x] `LinuxPlatformService.cs` - 平台信息和系统操作（跨平台通用实现）

**最小验证场景**：✅ 可以启动到主界面

---

## 编译状态

### ✅ 所有平台编译通过
- ✅ PCL-CE.Neo.Core.Abstractions (0 错误, 0 警告)
- ✅ PCL-CE.Neo.Core (0 错误, 0 警告)
- ✅ PCL-CE.Neo.Platform.Windows (0 错误, 0 警告)
- ✅ PCL-CE.Neo.Platform.macOS (0 错误, 0 警告)
- ✅ PCL-CE.Neo.Platform.Linux (0 错误, 0 警告)

---

## Phase 1-2 完成情况

### ✅ 已完成
- [x] 基础项目结构和配置
- [x] 核心抽象层（PCL-CE.Neo.Core.Abstractions）
- [x] 核心实现层（PCL-CE.Neo.Core）
- [x] Windows 平台服务（Java扫描、平台信息）
- [x] macOS 平台服务（Java扫描、平台信息）
- [x] Linux 平台服务（Java扫描、平台信息）

### 🔶 待完成（Phase 3+）
- [ ] Uno Platform UI层实现
- [ ] UI相关服务实现（动画、音频、剪贴板、对话框、通知、主题、窗口管理）
- [ ] 跨平台UI统一

---

## 技术方案

### 跨平台通用实现
所有平台服务使用 `System.Runtime.InteropServices.RuntimeInformation` 检测操作系统，实现跨平台功能：
- Java扫描：检测Windows/Unix系统，使用不同的Java路径和可执行文件名
- 平台服务：使用 `Process.Start` 调用系统命令打开URL和文件夹

### Windows 特有实现
- 使用 `explorer.exe` 打开文件夹
- 使用 `UseShellExecute = true` 打开URL
- Java路径：`C:\Program Files\Java`, `C:\Program Files (x86)\Java` 等

### macOS 特有实现
- 使用 `open` 命令打开URL和文件夹
- Java路径：`/Library/Java/JavaVirtualMachines`, `/usr/lib/jvm` 等

### Linux 特有实现
- 使用 `xdg-open` 命令打开URL和文件夹
- Java路径：`/usr/lib/jvm`, `/opt/java`, `~/.sdkman/candidates/java` 等

---

## 下一步计划

### Phase 3：UI层实现
1. 创建 Uno Platform UI项目
2. 实现所有UI相关服务：
   - [ ] AnimationService - 动画服务
   - [ ] AudioService - 音频服务
   - [ ] ClipboardService - 剪贴板服务
   - [ ] DialogService - 对话框服务
   - [ ] NotificationService - 通知服务
   - [ ] ThemeService - 主题服务
   - [ ] UIAccessProvider - UI线程访问
   - [ ] WindowService - 窗口管理
3. 集成UI层和平台服务层

---

**最后更新**：2025-XX-XX
**负责人**：PCL Community
**版本**：1.0.0
