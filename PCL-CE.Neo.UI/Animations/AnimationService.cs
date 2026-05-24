using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using System;

namespace PCL_CE.Neo.UI.Animations;

public enum AnimationType
{
    FadeIn,
    FadeOut,
    SlideFromLeft,
    SlideFromRight,
    Scale,
    Rotate
}

public class AnimationService
{
    private static AnimationService? _instance;

    public static AnimationService Instance => _instance ??= new AnimationService();

    public TimeSpan DefaultDuration { get; set; } = TimeSpan.FromMilliseconds(300);

    public void Animate(UIElement element, AnimationType type, Action? onCompleted = null)
    {
        switch (type)
        {
            case AnimationType.FadeIn:
                FadeIn(element, onCompleted);
                break;
            case AnimationType.FadeOut:
                FadeOut(element, onCompleted);
                break;
            case AnimationType.SlideFromLeft:
                SlideFromLeft(element, onCompleted);
                break;
            case AnimationType.SlideFromRight:
                SlideFromRight(element, onCompleted);
                break;
            case AnimationType.Scale:
                Scale(element, onCompleted);
                break;
        }
    }

    private void FadeIn(UIElement element, Action? onCompleted)
    {
        var animation = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = DefaultDuration,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        Storyboard.SetTarget(animation, element);
        Storyboard.SetTargetProperty(animation, "Opacity");

        if (onCompleted != null)
        {
            storyboard.Completed += (s, e) => onCompleted();
        }

        storyboard.Begin();
    }

    private void FadeOut(UIElement element, Action? onCompleted)
    {
        var animation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = DefaultDuration,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        Storyboard.SetTarget(animation, element);
        Storyboard.SetTargetProperty(animation, "Opacity");

        if (onCompleted != null)
        {
            storyboard.Completed += (s, e) => onCompleted();
        }

        storyboard.Begin();
    }

    private void SlideFromLeft(UIElement element, Action? onCompleted)
    {
        var animation = new DoubleAnimation
        {
            From = -50.0,
            To = 0.0,
            Duration = DefaultDuration,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        Storyboard.SetTarget(animation, element);
        Storyboard.SetTargetProperty(animation, "(UIElement.RenderTransform).(TranslateTransform.X)");

        if (onCompleted != null)
        {
            storyboard.Completed += (s, e) => onCompleted();
        }

        storyboard.Begin();
    }

    private void SlideFromRight(UIElement element, Action? onCompleted)
    {
        var animation = new DoubleAnimation
        {
            From = 50.0,
            To = 0.0,
            Duration = DefaultDuration,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        Storyboard.SetTarget(animation, element);
        Storyboard.SetTargetProperty(animation, "(UIElement.RenderTransform).(TranslateTransform.X)");

        if (onCompleted != null)
        {
            storyboard.Completed += (s, e) => onCompleted();
        }

        storyboard.Begin();
    }

    private void Scale(UIElement element, Action? onCompleted)
    {
        var animation = new DoubleAnimation
        {
            From = 0.8,
            To = 1.0,
            Duration = DefaultDuration,
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        Storyboard.SetTarget(animation, element);
        Storyboard.SetTargetProperty(animation, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");

        if (onCompleted != null)
        {
            storyboard.Completed += (s, e) => onCompleted();
        }

        storyboard.Begin();
    }
}
