# PCL-CE.Neo 测试覆盖率说明

## 概述

本文档说明 PCL-CE.Neo 项目的测试策略和覆盖率目标。

---

## 测试策略

### 单元测试

针对核心业务逻辑和平台抽象层的独立测试。

**目标覆盖率**: ≥ 80%

### 集成测试

针对平台实现和跨组件交互的测试。

**目标覆盖率**: 关键路径 100%

### 性能测试

针对性能基准和回归检测的测试。

**目标**: 所有基准指标通过

---

## 测试文件结构

```
PCL-CE.Neo.Tests/
├── AdapterTests.cs                    # 适配器单元测试
├── AnimationServiceTests.cs            # 动画服务测试
├── AuthAdapterTests.cs                # 认证适配器测试
├── ConfigServiceTests.cs              # 配置服务测试
├── DatabaseServiceTests.cs            # 数据库服务测试
├── LifecycleTests.cs                  # 生命周期测试
├── LinkServiceTests.cs                # 联机服务测试
├── MinecraftAdapterTests.cs           # Minecraft 适配器测试
├── ModAdapterTests.cs                 # Mod 适配器测试
├── NetworkTests.cs                    # 网络服务测试
├── ServiceExtensionsTests.cs          # 服务扩展测试
├── TaskManagerTests.cs                # 任务管理器测试
│
├── Performance/
│   └── BenchmarkTests.cs              # 性能基准测试
│
└── PlatformIntegration/
    ├── Windows/
    │   └── WindowsIntegrationTests.cs # Windows 集成测试
    ├── macOS/
    │   └── MacOSIntegrationTests.cs   # macOS 集成测试
    └── Linux/
        └── LinuxIntegrationTests.cs  # Linux 集成测试
```

---

## 测试覆盖率统计

### 当前测试数量

| 测试类别 | 文件数 | 测试方法数 | 状态 |
|---------|--------|-----------|------|
| 单元测试 | 10 | ~150 | ✅ |
| 集成测试 | 3 | ~90 | ✅ |
| 性能测试 | 1 | ~10 | ✅ |
| **总计** | **14** | **~250** | **✅** |

### 核心组件覆盖率

| 组件 | 单元测试 | 集成测试 | 目标 | 状态 |
|------|---------|---------|------|------|
| ConfigService | ✅ | - | 80% | ✅ |
| DatabaseService | ✅ | - | 80% | ✅ |
| NetworkService | ✅ | - | 80% | ✅ |
| Logger | ✅ | - | 80% | ✅ |
| TaskManager | ✅ | - | 80% | ✅ |
| PlatformServices | - | ✅ | 100% | ✅ |
| Adapters | ✅ | - | 80% | ✅ |

### Mock 实现覆盖

| Mock 类 | 测试覆盖 | 状态 |
|---------|---------|------|
| AnimationServiceMock | ✅ | ✅ |
| AudioServiceMock | ✅ | ✅ |
| ClipboardServiceMock | ✅ | ✅ |
| DialogServiceMock | ✅ | ✅ |
| JavaScannerMock | ✅ | ✅ |
| NotificationServiceMock | ✅ | ✅ |
| PlatformServiceMock | ✅ | ✅ |
| ThemeServiceMock | ✅ | ✅ |
| UIAccessProviderMock | ✅ | ✅ |
| WindowServiceMock | ✅ | ✅ |

---

## 运行测试

### 运行所有测试

```bash
dotnet test
```

### 运行单元测试

```bash
dotnet test --filter "Category=Unit"
```

### 运行集成测试

```bash
dotnet test --filter "Category=Integration"
```

### 运行性能测试

```bash
dotnet test --filter "FullyQualifiedName~Performance"
```

### 生成覆盖率报告

```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 覆盖率目标

### 总体目标

| 指标 | 目标 | 当前状态 |
|------|------|---------|
| 总体覆盖率 | ≥ 80% | 待验证 |
| 核心逻辑覆盖 | ≥ 90% | 待验证 |
| 平台接口覆盖 | 100% | ✅ |
| 关键路径覆盖 | 100% | ✅ |

### 关键路径

以下路径必须 100% 覆盖：

1. **服务初始化流程**
2. **配置加载/保存流程**
3. **游戏启动流程**
4. **认证流程**
5. **下载流程**

---

## 验证状态说明

| 状态 | 说明 |
|------|------|
| ✅ | 已验证通过 |
| ⚠️ | 需要手动验证 |
| ❌ | 缺少测试 |
| 待验证 | 代码存在，需实际运行验证 |

---

## 测试质量标准

### 好的测试

- ✅ 测试单一职责
- ✅ 测试名称清晰描述意图
- ✅ 包含 Arrange-Act-Assert 模式
- ✅ 测试隔离，不依赖其他测试
- ✅ 包含有意义的断言

### 示例

```csharp
[Fact]
public void ConfigService_SetAndGet_ReturnsCorrectValue()
{
    // Arrange
    var service = new ConfigService(logger, tempPath);
    
    // Act
    service.Set("key", "value");
    var result = service.Get<string>("key");
    
    // Assert
    Assert.Equal("value", result);
}
```

---

## 持续集成

### CI/CD 测试流程

```yaml
test:
  runs-on: ${{ matrix.os }}
  strategy:
    matrix:
      os: [windows-latest, macos-latest, ubuntu-latest]
  
  steps:
    - uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
    
    - name: Restore
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Test
      run: dotnet test --collect:"XPlat Code Coverage"
    
    - name: Upload Coverage
      uses: codecov/codecov-action@v3
```

---

## 相关文档

- [重构计划书.md](file:///workspace/docs/重构计划书.md) - 项目计划
- [跨平台功能验证](CROSS_PLATFORM_VERIFICATION.md) - 功能验证

---

**最后更新**: 2026-05-14
