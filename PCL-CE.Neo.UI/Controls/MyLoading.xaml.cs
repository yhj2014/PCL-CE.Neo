using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;

namespace PCL_CE.Neo.UI.Controls;

public sealed partial class MyLoading : UserControl
{
    private DispatcherTimer? _animationTimer;
    private int _currentIndex = 0;

    public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
        nameof(IsLoading),
        typeof(bool),
        typeof(MyLoading),
        new PropertyMetadata(true, OnIsLoadingChanged));

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(MyLoading),
        new PropertyMetadata(string.Empty));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public MyLoading()
    {
        InitializeComponent();
        if (IsLoading)
        {
            StartAnimation();
        }
    }

    private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MyLoading loading)
        {
            if ((bool)e.NewValue)
            {
                loading.StartAnimation();
            }
            else
            {
                loading.StopAnimation();
            }
        }
    }

    private void StartAnimation()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };
        _animationTimer.Tick += OnAnimationTick;
        _animationTimer.Start();
    }

    private void StopAnimation()
    {
        _animationTimer?.Stop();
        _animationTimer = null;
    }

    private void OnAnimationTick(object? sender, object e)
    {
        _currentIndex = (_currentIndex + 1) % 3;

        var activeOpacity = 1.0;
        var inactiveOpacity = 0.3;

        Dot1.Opacity = _currentIndex == 0 ? activeOpacity : inactiveOpacity;
        Dot2.Opacity = _currentIndex == 1 ? activeOpacity : inactiveOpacity;
        Dot3.Opacity = _currentIndex == 2 ? activeOpacity : inactiveOpacity;
    }
}
