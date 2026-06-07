# PCL-CE.Neo 运行说明

## 快速开始

解压压缩包后，根据您的操作系统运行对应的可执行文件：

---

## 各平台运行方式

### Windows
运行文件：`PCL-CE.Neo.App.Windows.exe`
- 双击直接运行
- 或在命令行中：
  ```cmd
  PCL-CE.Neo.App.Windows.exe
  ```

### Linux
运行文件：`PCL-CE.Neo.App.Linux`
- 需要先给可执行文件添加执行权限：
  ```bash
  chmod +x PCL-CE.Neo.App.Linux
  ```
- 然后运行：
  ```bash
  ./PCL-CE.Neo.App.Linux
  ```

### macOS
运行文件：`PCL-CE.Neo.App.macOS`
- 需要先给可执行文件添加执行权限：
  ```bash
  chmod +x PCL-CE.Neo.App.macOS
  ```
- 然后运行：
  ```bash
  ./PCL-CE.Neo.App.macOS
  ```

---

## 为什么有这么多文件？

这是 .NET 自包含（Self-Contained）部署的特点，包含了：
- ✅ 应用程序主文件
- ✅ .NET 运行时（不需要单独安装）
- ✅ 所有依赖的库
- ✅ 平台特定的库（如 Linux/macOS 的 .so/.dylib，Windows 的 .dll）

**好处**：不需要在目标机器上安装 .NET SDK 或 Runtime，解压即可运行。

---

## 文件说明

### 主要文件
- **PCL-CE.Neo.App.Windows.exe** (Windows) - 主程序入口
- **PCL-CE.Neo.App.Linux** (Linux) - 主程序入口
- **PCL-CE.Neo.App.macOS** (macOS) - 主程序入口

### 核心库
- PCL-CE.Neo.Core.dll - 核心业务逻辑
- PCL-CE.Neo.UI.dll - UI 组件
- PCL-CE.Neo.Core.Abstractions.dll - 抽象接口
- PCL-CE.Neo.Platform.*.dll - 各平台实现

### .NET 运行时（自包含）
- System.*.dll - .NET 框架库
- libcoreclr.so / libcoreclr.dylib / coreclr.dll - 运行时核心
- 其他平台特定的库

---

## 常见问题

### Q: 双击没有反应？
**A**: 
- Windows: 检查是否有防火墙拦截
- Linux/macOS: 确保有执行权限（`chmod +x`）

### Q: 运行时报错？
**A**:
1. 确保所有文件都在同一目录下，不要只复制一个可执行文件
2. 检查是否缺少权限
3. 查看控制台输出的错误信息

### Q: 能否删除一些文件减少体积？
**A**: **不建议**，所有文件都是运行必需的。如果需要更小的体积，可以考虑框架依赖部署（Framework-dependent）模式。

---

## 技术细节

当前使用的是 **自包含部署（Self-Contained Deployment）**：
- ✅ 无需安装 .NET Runtime
- ✅ 完整的运行时和依赖包含在包中
- ✅ 体积较大（通常 50-100MB+），但兼容性最好

---

*最后更新：2026-06-07*
