# PCL CE UI 迁移指南

本文档描述如何将 PCL CE 的 WPF UI 像素级迁移到 Uno Platform。

## 目录

1. [概述](#1-概述)
2. [像素级还原原则](#2-像素级还原原则)
3. [迁移步骤](#3-迁移步骤)
4. [控件迁移规范](#4-控件迁移规范)
5. [资源提取](#5-资源提取)
6. [验证清单](#6-验证清单)

---

## 1. 概述

### 1.1 目标

将现有的 WPF UI 完整迁移到 Uno Platform，同时保持：
- 100% 的视觉一致性
- 相同的行为和交互
- 相同的功能

### 1.2 迁移范围

**需要迁移的内容**：
- 所有 XAML 页面和控件
- 所有自定义控件
- 样式和模板
- 动画效果
- 资源文件（图片、颜色、字体等）

**特殊处理**：
- Win32 API 调用 → 平台抽象
- WPF 特定特效 → Uno 兼容实现
- WPF 内部反射 → 跨平台替代方案

---

## 2. 像素级还原原则

### 2.1 视觉一致性

**颜色**：
```xaml
<!-- 原 WPF -->
<SolidColorBrush x:Key="ColorBrush1" Color="#FF3498DB"/>

<!-- Uno Platform -->
<SolidColorBrush x:Key="ColorBrush1" Color="#FF3498DB"/>
```

- 所有颜色值必须完全一致
- 使用资源字典统一管理
- 支持主题切换时自动切换颜色

**尺寸和间距**：
- 所有 Margin 和 Padding 必须保持一致
- 使用 Grid 的 Star sizing 时比例一致
- 使用像素值而非相对值

**字体**：
- 字体家族必须相同
- 字号必须相同
- 字重必须相同

### 2.2 行为一致性

**交互**：
- 点击、悬停、按下效果必须相同
- 焦点样式必须相同
- 键盘导航必须相同

**动画**：
- 动画时长必须相同
- 缓动函数必须相同
- 动画曲线必须相同

---

## 3. 迁移步骤

### 3.1 第一步：资源提取

提取所有 UI 资源为统一定义：

```
PCL.UI/
├── Resources/
│   ├── Colors.xaml          # 所有颜色定义
│   ├── Brushes.xaml          # 画刷定义
│   ├── TextStyles.xaml      # 文本样式
│   ├── ControlStyles.xaml   # 控件样式
│   └── Dimensions.xaml      # 尺寸常量
├── Controls/                 # 自定义控件
└── Pages/                   # 页面
```

### 3.2 第二步：创建 Uno 项目

```xml
<!-- PCL.UI.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>
      net10.0-windows10.0.19041.0;
      net10.0-maccatalyst;
      net10.0-linux
    </TargetFrameworks>
    <UseWinUI>true</UseWinUI>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Uno.WinUI" Version="5.0.0" />
    <PackageReference Include="Uno.Toolkit.WinUI" Version="5.0.0" />
  </ItemGroup>
</Project>
```

### 3.3 第三步：迁移控件

按优先级迁移自定义控件：

1. **基础控件**（必须优先）
   - `MyButton` → 自定义按钮
   - `MyTextBox` → 自定义文本框
   - `MyComboBox` → 自定义下拉框

2. **容器控件**
   - `MyCard` → 卡片容器
   - `BlurBorder` → 模糊边框（需要替代方案）

3. **功能控件**
   - `MyIconButton` → 图标按钮
   - `MySearchBox` → 搜索框
   - `MySlider` → 滑块

### 3.4 第四步：迁移页面

按模块迁移页面：

1. **主页** - `PageLaunch/`
2. **下载页** - `PageDownload/`
3. **设置页** - `PageSetup/`
4. **工具页** - `PageTools/`

---

## 4. 控件迁移规范

### 4.1 MyButton

**原 WPF 实现**：
```xaml
<!-- Plain Craft Launcher 2/Controls/MyButton.xaml -->
<Style x:Key="MyButtonStyle" TargetType="Button">
    <Setter Property="Background" Value="{DynamicResource ColorBrush1}"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="Padding" Value="12,6"/>
    <Setter Property="MinWidth" Value="80"/>
    <!-- 更多属性... -->
</Style>
```

**Uno Platform 实现**：
```xaml
<!-- PCL.UI/Controls/MyButton.xaml -->
<Style x:Key="MyButtonStyle" TargetType="Button">
    <Setter Property="Background" Value="{StaticResource ColorBrush1}"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="Padding" Value="12,6"/>
    <Setter Property="MinWidth" Value="80"/>
    <!-- 保持完全一致 -->
</Style>
```

### 4.2 BlurBorder（特效控件）

**问题**：WPF 使用 Pixel Shader 实现模糊效果，Uno 不直接支持。

**替代方案**：
1. 使用 Uno 的 `AcrylicBackgroundSource`（如果可用）
2. 使用纯色半透明背景
3. 使用平台特定的模糊 API

```xaml
<!-- 替代方案：使用半透明背景 -->
<Border Background="#80000000" CornerRadius="6">
    <ContentPresenter/>
</Border>
```

### 4.3 动画迁移

**原 WPF**：
```xml
<Storyboard>
    <DoubleAnimation
        Storyboard.TargetName="MyElement"
        Storyboard.TargetProperty="Opacity"
        From="0" To="1" Duration="0:0:0.3"/>
</Storyboard>
```

**Uno Platform**：
```xml
<!-- 使用 Visual States 和 Transitions -->
<VisualState x:Name="FadeIn">
    <VisualState.Storyboard>
        <Storyboard>
            <DoubleAnimation
                Storyboard.TargetName="MyElement"
                Storyboard.TargetProperty="Opacity"
                To="1" Duration="0:0:0.3"/>
        </Storyboard>
    </VisualState.Storyboard>
</VisualState>
```

---

## 5. 资源提取

### 5.1 颜色资源

从现有代码中提取所有颜色：

**原 WPF**：
```csharp
// 在 CatColorResource.cs 或各处硬编码
public static class Colors
{
    public static Color Primary = Color.FromRgb(52, 152, 219);
    public static Color Secondary = Color.FromRgb(46, 204, 113);
    // ...
}
```

**提取为资源**：
```xml
<!-- Resources/Colors.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:system="clr-namespace:System;assembly=mscorlib">

    <!-- Primary Colors -->
    <Color x:Key="PrimaryColor">#FF3498DB</Color>
    <Color x:Key="SecondaryColor">#FF2ECC71</Color>

    <!-- Semantic Colors -->
    <Color x:Key="SuccessColor">#FF2ECC71</Color>
    <Color x:Key="WarningColor">#FFF39C12</Color>
    <Color x:Key="ErrorColor">#FFE74C3C</Color>

    <!-- Text Colors -->
    <Color x:Key="TextPrimaryColor">#FF2C3E50</Color>
    <Color x:Key="TextSecondaryColor">#FF7F8C8D</Color>

</ResourceDictionary>
```

### 5.2 尺寸资源

```xml
<!-- Resources/Dimensions.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Button Dimensions -->
    <sys:Double x:Key="ButtonMinWidth" xmlns:sys="clr-namespace:System;assembly=mscorlib">80</sys:Double>
    <Thickness x:Key="ButtonPadding">12,6</Thickness>
    <CornerRadius x:Key="ButtonCornerRadius">4</CornerRadius>

    <!-- Control Dimensions -->
    <sys:Double x:Key="IconSize" xmlns:sys="clr-namespace:System;assembly=mscorlib">24</sys:Double>
    <sys:Double x:Key="SmallIconSize" xmlns:sys="clr-namespace:System;assembly=mscorlib">16</sys:Double>

    <!-- Spacing -->
    <Thickness x:Key="DefaultMargin">8</Thickness>
    <Thickness x:Key="LargeMargin">16</Thickness>

</ResourceDictionary>
```

### 5.3 文本样式

```xml
<!-- Resources/TextStyles.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Headings -->
    <Style x:Key="HeadingStyle" TargetType="TextBlock">
        <Setter Property="FontSize" Value="24"/>
        <Setter Property="FontWeight" Value="Bold"/>
        <Setter Property="Foreground" Value="{StaticResource TextPrimaryColor}"/>
    </Style>

    <Style x:Key="SubheadingStyle" TargetType="TextBlock">
        <Setter Property="FontSize" Value="18"/>
        <Setter Property="FontWeight" Value="SemiBold"/>
        <Setter Property="Foreground" Value="{StaticResource TextPrimaryColor}"/>
    </Style>

    <!-- Body -->
    <Style x:Key="BodyStyle" TargetType="TextBlock">
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="Foreground" Value="{StaticResource TextPrimaryColor}"/>
        <Setter Property="TextWrapping" Value="Wrap"/>
    </Style>

    <!-- Caption -->
    <Style x:Key="CaptionStyle" TargetType="TextBlock">
        <Setter Property="FontSize" Value="12"/>
        <Setter Property="Foreground" Value="{StaticResource TextSecondaryColor}"/>
    </Style>

</ResourceDictionary>
```

---

## 6. 验证清单

### 6.1 视觉验证

- [ ] 所有颜色完全一致
- [ ] 所有尺寸完全一致
- [ ] 所有间距完全一致
- [ ] 所有字体完全一致
- [ ] 所有图标完全一致
- [ ] 所有特效效果一致

### 6.2 功能验证

- [ ] 所有交互正常
- [ ] 所有动画正常
- [ ] 所有键盘导航正常
- [ ] 所有主题切换正常
- [ ] 所有窗口操作正常

### 6.3 跨平台验证

- [ ] Windows 平台正常
- [ ] macOS 平台正常
- [ ] Linux 平台正常
- [ ] 平台特定功能正常

### 6.4 性能验证

- [ ] 启动时间 ≤ 原版本
- [ ] 响应时间 ≤ 原版本
- [ ] 内存占用 ≤ 原版本 120%
- [ ] 无明显卡顿

---

## 7. 常见问题

### 7.1 Uno 不支持的特性

| WPF 特性 | Uno 替代方案 |
|----------|--------------|
| Pixel Shader Effects | 使用半透明背景或平台 API |
| WPF 特定控件 | 使用 Uno 等效控件 |
| WPF 触发器 | 使用 Visual States |
| RelativeSource | 使用 x:Name 绑定 |

### 7.2 性能优化

1. 使用 `VirtualizingStackPanel` 优化长列表
2. 使用图片缓存减少加载时间
3. 延迟加载非关键资源

---

## 8. 附录

### 8.1 控件清单

| 原控件名 | 类型 | 优先级 | 状态 |
|----------|------|--------|------|
| MyButton | Button | P0 | 待迁移 |
| MyTextBox | TextBox | P0 | 待迁移 |
| MyComboBox | ComboBox | P0 | 待迁移 |
| MyIconButton | Button | P1 | 待迁移 |
| MySearchBox | TextBox | P1 | 待迁移 |
| BlurBorder | Border | P2 | 待迁移 |
| ... | ... | ... | ... |

### 8.2 参考资源

- [Uno Platform 文档](https://platform.uno/docs/)
- [WinUI 3 文档](https://learn.microsoft.com/windows/apps/winui/)
- [WPF 到 WinUI 迁移指南](https://learn.microsoft.com/windows/apps/desktop/migrate/)
