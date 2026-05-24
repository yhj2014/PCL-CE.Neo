# 代码规范

## 命名规范

### C# 命名约定

| 元素类型 | 约定 | 示例 |
|---------|------|------|
| 命名空间 | PascalCase | PCL_CE.Neo.Core.Adapters |
| 类 | PascalCase | ApplicationAdapter |
| 接口 | IPascalCase (I 前缀) | IPlatformService |
| 方法 | PascalCase | GetJavaPath() |
| 属性 | PascalCase | PlatformName |
| 常量 | PascalCase | MaxRetries |
| 私有字段 | _camelCase (下划线前缀) | _logger |
| 局部变量 | camelCase | javaPath |
| 参数 | camelCase | platformName |

### 文件命名

- 类文件名与类名一致：`ApplicationAdapter.cs`
- 接口文件名与接口名一致：`IPlatformService.cs`

## 代码组织

### 文件头部

每个源文件应包含：
```csharp
// -----------------------------------------------------------------------------
// PCL-CE.Neo Project
// Copyright (c) PCL Community. All rights reserved.
// Licensed under the same license as PCL Community Edition.
// -----------------------------------------------------------------------------
```

### 区域顺序

一个典型的类文件结构：
```csharp
// 1. Using 语句
// 2. Namespace
// 3. Class
//    - Constants
//    - Fields
//    - Constructors
//    - Properties
//    - Methods
//    - Events
//    - Nested Types
```

## 注释规范

### XML 文档注释

所有公共 API 必须有 XML 文档注释：
```csharp
/// <summary>
/// 平台服务接口，提供平台特定功能
/// </summary>
public interface IPlatformService
{
    /// <summary>
    /// 获取平台名称
    /// </summary>
    string PlatformName { get; }
    
    /// <summary>
    /// 打开指定 URL
    /// </summary>
    /// <param name="url">要打开的 URL</param>
    void OpenUrl(string url);
}
```

### 行内注释

使用行内注释解释复杂的逻辑：
```csharp
// 检查 Java 是否有效 - 先验证路径存在，再检查版本
if (File.Exists(path) && IsValidJavaVersion(path))
```

## 代码质量

### 可空引用类型

项目启用了可空引用类型，请正确使用：
```csharp
// 正确 - 明确表达可空性
public string? GetOptionalValue() { ... }
public string GetRequiredValue() { ... }
```

### 异常处理

- 捕获特定异常而不是通用 `Exception`
- 提供有意义的错误信息
- 不要吞掉异常

```csharp
// 正确
try
{
    File.ReadAllText(path);
}
catch (FileNotFoundException ex)
{
    _logger.LogError(ex, "File not found: {Path}", path);
    throw;
}
```

### 依赖注入

使用构造函数注入：
```csharp
public class ApplicationAdapter
{
    private readonly ILoggerAdapter _logger;
    
    public ApplicationAdapter(ILoggerAdapter logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

## Git 提交规范

### 提交信息格式

```
<type>(<scope>): <subject>

<body>

<footer>
```

### 类型 (Type)

- `feat`: 新功能
- `fix`: 修复 bug
- `docs`: 文档更新
- `style`: 代码格式（不影响代码运行的变动）
- `refactor`: 重构
- `perf`: 性能优化
- `test`: 测试相关
- `chore`: 构建过程或辅助工具的变动

### 示例

```
feat(core): add platform detection service

- Add PlatformDetector class
- Support Windows/macOS/Linux detection
- Add unit tests

Closes #123
```

## 测试规范

### 测试命名

`方法名_场景_预期结果`
```csharp
[Fact]
public void GetJavaPath_WithValidPath_ReturnsPath()
```

### 测试结构

每个测试应遵循 AAA 模式：
- Arrange: 准备测试数据
- Act: 执行被测试的方法
- Assert: 验证结果

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
