# 测试指南

## 测试金字塔

我们遵循测试金字塔策略：
- **单元测试** (60%)：测试单个类或组件
- **集成测试** (30%)：测试多个组件协作
- **端到端测试** (10%)：完整用户流程

## 测试工具

- **测试框架**：xUnit
- **Mock 框架**：Moq
- **断言库**：FluentAssertions
- **覆盖率**：Coverlet

## 单元测试

### 测试项目结构

```
PCL-CE.Neo.Tests/
├── PlatformAbstractions/
│   └── PlatformServiceTests.cs
├── <AdapterName>Tests.cs
├── ServiceLocatorTests.cs
├── TestLogger.cs
└── ...
```

### 测试命名规范

`方法名_场景_预期结果`

```csharp
[Fact]
public void GetConfig_WithExistingKey_ReturnsValue()
```

### AAA 模式

每个测试应遵循 AAA 模式：

```csharp
[Fact]
public void IsValidJavaPath_WithValidPath_ReturnsTrue()
{
    // Arrange
    var scanner = new JavaScanner();
    var path = "/usr/bin/java";
    
    // Act
    var result = scanner.IsValidJavaPath(path);
    
    // Assert
    Assert.True(result);
}
```

## 测试覆盖目标

- **核心业务逻辑**：≥ 80%
- **适配器层**：≥ 70%
- **平台实现**：≥ 60%

## 运行测试

### 运行所有测试

```bash
dotnet test PCL-CE.Neo.Tests
```

### 运行特定测试

```bash
dotnet test --filter "FullyQualifiedName~PlatformServiceTests
```

### 生成覆盖率报告

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 平台抽象接口测试

为平台抽象接口创建 Mock 实现，用于单元测试：

```csharp
public class MockPlatformService : IPlatformService
{
    public string PlatformName => "Test";
    public string OSVersion => "1.0";
    public string Architecture => "x64";
    
    // ... 其他方法
}
```

## 持续集成

所有测试在 CI/CD 中自动运行，任何失败都会阻止合并。

## 测试最佳实践

- 测试独立，不依赖执行顺序
- 测试应该快速执行
- 测试应该是确定性的
- 测试应该有清晰的断言
- 测试应该覆盖边界条件
- 测试应该验证异常处理
