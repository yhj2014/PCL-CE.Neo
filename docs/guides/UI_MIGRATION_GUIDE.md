# UI 迁移指南

本文档介绍将 PCL CE 的 WPF UI 迁移到 Uno Platform 的详细指南。

---

## 目录

1. [架构概述](#架构概述)
2. [项目结构](#项目结构)
3. [XAML 差异](#xaml-差异)
4. [资源系统](#资源系统)
5. [主题系统](#主题系统)
6. [自定义控件](#自定义控件)
7. [页面迁移](#页面迁移)
8. [最佳实践](#最佳实践)

---

## 架构概述

### WPF 与 Uno Platform 的主要区别

| 特性 | WPF | Uno Platform |
|------|-----|-------------|
| XAML 命名空间 | `System.Windows.Controls` | `Microsoft.UI.Xaml.Controls` |
| 动画系统 | WPF Storyboard | Composition API |
| 特效系统 | WPF Effects | Composition Effects |
| 窗口管理 | `Window` 类 | `Window` 类（跨平台适配）|
| 平台支持 | Windows 仅 | Windows/macOS/Linux/WebAssembly |

---

## 项目结构

```
PCL-CE.Neo.UI/
├── App.xaml                    # 应用入口
├── App.xaml.cs
├── MainWindow.xaml             # 主窗口
├── MainWindow.xaml.cs
├── PCL-CE.Neo.UI.csproj
├── Resources/                  # 资源文件
│   ├── Colors.xaml
│   ├── DarkColors.xaml
│   ├── Dimensions.xaml
│   ├── TextStyles.xaml
│   └── ControlStyles.xaml
├── Themes/                     # 主题管理
│   └── ThemeManager.cs
├── Navigation/                 # 导航服务
│   └── NavigationService.cs
├── Controls/                   # 自定义控件
│   ├── Card.xaml
│   └── Card.xaml.cs
├── Services/                   # UI 服务
└── Pages/                      # 页面
    ├── HomePage.xaml
    └── HomePage.xaml.cs
```

---

## XAML 差异

### 1. 命名空间变更

**WPF 原代码：**
```xml
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <StackPanel>
        <Button Content="点击我"/>
    </StackPanel>
</Window>
```

**Uno Platform 新代码：**
```xml
<Page xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <StackPanel>
        <Button Content="点击我"/>
    </StackPanel>
</Page>
```

### 2. 控件属性变化

**WPF：**
```xml
<TextBlock Text="示例"
           TextWrapping="Wrap"
           FontFamily="微软雅黑"
           FontSize="14"/>
```

**Uno Platform：**
```xml
<TextBlock Text="示例"
           TextWrapping="Wrap"
           FontFamily="ms-appx:///Assets/Fonts/MyFont.ttf#MyFont"
           FontSize="14"/>
```

### 3. 事件处理

**WPF：**
```xml
<Button Click="Button_Click"/>
```

**Uno Platform（相同）：**
```xml
<Button Click="Button_Click"/>
```

---

## 资源系统

### 颜色资源

**亮色主题（Colors.xaml）：**
```xml
<Color x:Key="PrimaryColor">#FF3498DB</Color>
<Color x:Key="BackgroundColor">#FFFFFFFF</Color>
<Color x:Key="TextPrimaryColor">#FF2C3E50</Color>

<SolidColorBrush x:Key="PrimaryBrush" Color="{StaticResource PrimaryColor}"/>
<SolidColorBrush x:Key="BackgroundBrush" Color="{StaticResource BackgroundColor}"/>
```

**暗色主题（DarkColors.xaml）：**
```xml
<Color x:Key="PrimaryColor">#FF3498DB</Color>
<Color x:Key="BackgroundColor">#FF1A1A2E</Color>
<Color x:Key="TextPrimaryColor">#FFECF0F1</Color>

<SolidColorBrush x:Key="PrimaryBrush" Color="{StaticResource PrimaryColor}"/>
<SolidColorBrush x:Key="BackgroundBrush" Color="{StaticResource BackgroundColor}"/>
```

### 尺寸资源

```xml
<sys:Double x:Key="ButtonMinWidth">80</sys:Double>
<sys:Double x:Key="ButtonMinHeight">32</sys:Double>
<Thickness x:Key="ButtonPadding">12,6</Thickness>
<CornerRadius x:Key="ButtonCornerRadius">4</CornerRadius>
<CornerRadius x:Key="CardCornerRadius">8</CornerRadius>
```

### 样式资源

```xml
<Style x:Key="BodyStyle" TargetType="TextBlock">
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}"/>
    <Setter Property="TextWrapping" Value="Wrap"/>
</Style>

<Style x:Key="BaseButtonStyle" TargetType="Button">
    <Setter Property="Background" Value="{StaticResource PrimaryBrush}"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="Padding" Value="{StaticResource ButtonPadding}"/>
    <Setter Property="MinWidth" Value="{StaticResource ButtonMinWidth}"/>
    <Setter Property="MinHeight" Value="{StaticResource ButtonMinHeight}"/>
</Style>
```

---

## 主题系统

### ThemeManager 使用

```csharp
using PCL_CE.Neo.UI.Themes;

// 初始化
ThemeManager.Instance.Initialize();

// 切换到亮色主题
ThemeManager.Instance.SetTheme(AppTheme.Light);

// 切换到暗色主题
ThemeManager.Instance.SetTheme(AppTheme.Dark);

// 获取当前主题
var currentTheme = ThemeManager.Instance.CurrentTheme;
```

### 主题切换实现

ThemeManager 通过动态替换资源字典来实现主题切换：

```csharp
private void ApplyTheme(AppTheme theme)
{
    var resources = Application.Current.Resources;
    var mergedDictionaries = resources.MergedDictionaries;

    // 移除旧的颜色资源
    foreach (var dict in mergedDictionaries.ToList())
    {
        if (dict.Source?.OriginalString?.Contains("Colors.xaml") == true ||
            dict.Source?.OriginalString?.Contains("DarkColors.xaml") == true)
        {
            mergedDictionaries.Remove(dict);
        }
    }

    // 添加新的颜色资源
    var colorResource = theme == AppTheme.Dark
        ? new ResourceDictionary { Source = new Uri("ms-appx:///Resources/DarkColors.xaml") }
        : new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Colors.xaml") };

    mergedDictionaries.Insert(0, colorResource);
}
```

---

## 自定义控件

### 1. 创建 UserControl

**XAML：**
```xml
<UserControl x:Class="PCL_CE.Neo.UI.Controls.Card"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Border Background="{StaticResource BackgroundSecondaryBrush}"
            CornerRadius="{StaticResource CardCornerRadius}"
            Padding="{StaticResource DefaultMargin}"
            BorderBrush="{StaticResource BorderBrush}"
            BorderThickness="1">
        <ContentPresenter/>
    </Border>
</UserControl>
```

**C#：**
```csharp
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PCL_CE.Neo.UI.Controls;

public sealed partial class Card : UserControl
{
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header),
        typeof(string),
        typeof(Card),
        new PropertyMetadata(string.Empty));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public Card()
    {
        InitializeComponent();
    }
}
```

### 2. 使用自定义控件

```xml
<Page xmlns:controls="using:PCL_CE.Neo.UI.Controls">
    <controls:Card Header="示例卡片">
        <StackPanel>
            <TextBlock Text="卡片内容"/>
        </StackPanel>
    </controls:Card>
</Page>
```

---

## 页面迁移

### 1. 创建 Page 而非 Window

**WPF 原代码（Window）：**
```xml
<Window x:Class="PCL.MyPage"
        Title="我的页面">
    <Grid>
        <!-- 内容 -->
    </Grid>
</Window>
```

**Uno Platform 新代码（Page）：**
```xml
<Page x:Class="PCL_CE.Neo.UI.Pages.MyPage">
    <Grid>
        <!-- 内容 -->
    </Grid>
</Page>
```

### 2. 使用 NavigationService 导航

```csharp
using PCL_CE.Neo.UI.Navigation;

// 导航到新页面
NavigationService.Instance.Navigate(typeof(MyPage));

// 导航到新页面并传递参数
NavigationService.Instance.Navigate(typeof(MyPage), parameter);

// 回退
if (NavigationService.Instance.CanGoBack)
{
    NavigationService.Instance.GoBack();
}
```

### 3. 页面生命周期

```csharp
public sealed partial class MyPage : Page
{
    public MyPage()
    {
        InitializeComponent();
        // 初始化
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        // 页面导航到时
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        // 页面导航离开时
    }
}
```

---

## 最佳实践

### 1. 资源重用

- **始终使用资源字典中的样式和颜色**
- **避免硬编码的颜色和尺寸**
- **优先使用样式而非内联属性**

```xml
<!-- ✅ 推荐 -->
<TextBlock Style="{StaticResource BodyStyle}" Text="示例"/>

<!-- ❌ 避免 -->
<TextBlock FontSize="14" Foreground="#FF2C3E50" Text="示例"/>
```

### 2. MVVM 模式

使用 CommunityToolkit.Mvvm 实现 MVVM：

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class MyViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "标题";

    [RelayCommand]
    private void DoSomething()
    {
        // 命令逻辑
    }
}
```

### 3. 异步优先

所有 I/O 操作都应该是异步的：

```csharp
// ✅ 推荐
private async void Button_Click(object sender, RoutedEventArgs e)
{
    var result = await LoadDataAsync();
    DisplayResult(result);
}

// ❌ 避免（会阻塞 UI 线程）
private void Button_Click(object sender, RoutedEventArgs e)
{
    var result = LoadData();
    DisplayResult(result);
}
```

### 4. 平台特定代码

使用条件编译处理平台差异：

```csharp
#if WINDOWS
    // Windows 特定代码
    using Windows.Storage;
#elif __MACOS__
    // macOS 特定代码
    using Foundation;
#elif __LINUX__
    // Linux 特定代码
#endif
```

### 5. 性能优化

- 使用 `x:Load` 延迟加载非关键元素
- 使用虚拟化列表处理大量数据
- 避免在 UI 线程执行耗时操作

```xml
<ListView x:Load="False">
    <!-- 内容 -->
</ListView>
```

---

## 迁移检查清单

- [ ] 所有 Window 转换为 Page
- [ ] 更新所有 XAML 命名空间
- [ ] 移植所有资源（颜色、样式、模板）
- [ ] 实现主题切换支持
- [ ] 迁移自定义控件
- [ ] 使用 NavigationService 替代窗口导航
- [ ] 测试三个平台（Windows/macOS/Linux）
- [ ] 验证所有交互行为一致
- [ ] 性能测试和优化
- [ ] 无障碍支持

---

**文档版本：** 1.0
**最后更新：** 2026-05-24
