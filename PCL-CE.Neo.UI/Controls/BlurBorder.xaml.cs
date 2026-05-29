using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace PCL_CE.Neo.UI.Controls;

public sealed partial class BlurBorder : Border
{
    public static readonly DependencyProperty BlurAmountProperty = DependencyProperty.Register(
        nameof(BlurAmount),
        typeof(double),
        typeof(BlurBorder),
        new PropertyMetadata(0.0, OnBlurAmountChanged));

    public static readonly DependencyProperty BlurEnabledProperty = DependencyProperty.Register(
        nameof(BlurEnabled),
        typeof(bool),
        typeof(BlurBorder),
        new PropertyMetadata(false, OnBlurEnabledChanged));

    public double BlurAmount
    {
        get => (double)GetValue(BlurAmountProperty);
        set => SetValue(BlurAmountProperty, value);
    }

    public bool BlurEnabled
    {
        get => (bool)GetValue(BlurEnabledProperty);
        set => SetValue(BlurEnabledProperty, value);
    }

    public BlurBorder()
    {
        InitializeComponent();
    }

    private static void OnBlurAmountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // 暂时不实现
    }

    private static void OnBlurEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // 暂时不实现
    }
}
