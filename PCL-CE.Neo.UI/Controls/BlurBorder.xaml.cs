using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Composition;

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
        if (d is BlurBorder border && border.BlurEnabled)
        {
            border.UpdateBlur();
        }
    }

    private static void OnBlurEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BlurBorder border)
        {
            border.UpdateBlur();
        }
    }

    private void UpdateBlur()
    {
        if (BlurEnabled && BlurAmount > 0)
        {
#if WINDOWS
            var compositor = Window.Current.Compositor;
            if (compositor != null)
            {
                var backdropBrush = compositor.CreateColorBrush(Colors.Transparent);
                var blurEffect = compositor.CreateGaussianBlurEffect();
                blurEffect.StdDeviation = BlurAmount;
                blurEffect.Source = backdropBrush;

                Background = new CompositionBrush?();
            }
#endif
        }
        else
        {
            Background = new SolidColorBrush(Colors.Transparent);
        }
    }
}
