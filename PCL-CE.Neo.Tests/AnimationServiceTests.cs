using System;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Core.Abstractions.Mock;
using Xunit;

namespace PCL_CE.Neo.Tests;

public class AnimationServiceTests
{
    [Fact]
    public async Task AnimateAsync_CompletesSuccessfully()
    {
        var service = new AnimationServiceMock();
        var element = new object();
        var completed = false;
        
        var description = new AnimationDescription
        {
            Duration = TimeSpan.FromMilliseconds(100),
            OnCompleted = () => completed = true
        };

        await service.AnimateAsync(element, description);
        
        Assert.True(completed);
    }

    [Fact]
    public void IsAnimating_ReturnsTrue_WhenAnimationInProgress()
    {
        var service = new AnimationServiceMock();
        var element = new object();

        var animTask = service.AnimateAsync(element, new AnimationDescription
        {
            Duration = TimeSpan.FromMilliseconds(500)
        });

        Assert.True(service.IsAnimating(element));
    }

    [Fact]
    public void CancelAnimation_RemovesElementFromAnimatingList()
    {
        var service = new AnimationServiceMock();
        var element = new object();

        // Start an animation
        var animTask = service.AnimateAsync(element, new AnimationDescription
        {
            Duration = TimeSpan.FromMilliseconds(500)
        });
        
        // Cancel it
        service.CancelAnimation(element);
        
        Assert.False(service.IsAnimating(element));
    }

    [Fact]
    public async Task FadeInAsync_CompletesSuccessfully()
    {
        var service = new AnimationServiceMock();
        var element = new object();
        
        var startTime = DateTime.Now;
        await service.FadeInAsync(element, 100);
        var endTime = DateTime.Now;
        
        Assert.True(endTime - startTime >= TimeSpan.FromMilliseconds(50));
    }

    [Fact]
    public async Task FadeOutAsync_CompletesSuccessfully()
    {
        var service = new AnimationServiceMock();
        var element = new object();
        
        await service.FadeOutAsync(element, 100);
        
        Assert.False(service.IsAnimating(element));
    }

    [Fact]
    public async Task ScaleAsync_CompletesSuccessfully()
    {
        var service = new AnimationServiceMock();
        var element = new object();
        
        await service.ScaleAsync(element, 1.5, 100);
        
        Assert.False(service.IsAnimating(element));
    }

    [Fact]
    public async Task MoveToAsync_CompletesSuccessfully()
    {
        var service = new AnimationServiceMock();
        var element = new object();
        
        await service.MoveToAsync(element, 100, 200, 100);
        
        Assert.False(service.IsAnimating(element));
    }

    [Fact]
    public void AnimationDescription_DefaultValues_AreSetCorrectly()
    {
        var description = new AnimationDescription();
        
        Assert.Equal(TimeSpan.FromSeconds(0.3), description.Duration);
        Assert.Equal(EasingType.Linear, description.EasingType);
        Assert.Equal("", description.PropertyName);
        Assert.Null(description.FromValue);
        Assert.Null(description.ToValue);
        Assert.Null(description.KeyFrames);
        Assert.Null(description.OnCompleted);
    }

    [Fact]
    public void AnimationDescription_CustomValues_AreSetCorrectly()
    {
        var onCompletedCalled = false;
        var description = new AnimationDescription
        {
            Duration = TimeSpan.FromSeconds(1.5),
            PropertyName = "Opacity",
            FromValue = 0.0,
            ToValue = 1.0,
            EasingType = EasingType.ElasticOut,
            KeyFrames = new List<double> { 0.0, 0.5, 1.0 },
            OnCompleted = () => onCompletedCalled = true
        };
        
        Assert.Equal(TimeSpan.FromSeconds(1.5), description.Duration);
        Assert.Equal("Opacity", description.PropertyName);
        Assert.Equal(0.0, description.FromValue);
        Assert.Equal(1.0, description.ToValue);
        Assert.Equal(EasingType.ElasticOut, description.EasingType);
        Assert.NotNull(description.KeyFrames);
        Assert.Equal(3, description.KeyFrames.Count);
        Assert.NotNull(description.OnCompleted);
        
        // Test that OnCompleted can be invoked
        description.OnCompleted();
        Assert.True(onCompletedCalled);
    }

    [Fact]
    public void EasingType_HasAllExpectedValues()
    {
        var values = Enum.GetValues<EasingType>();
        
        Assert.Contains(EasingType.Linear, values);
        Assert.Contains(EasingType.QuadraticIn, values);
        Assert.Contains(EasingType.QuadraticOut, values);
        Assert.Contains(EasingType.QuadraticInOut, values);
        Assert.Contains(EasingType.CubicIn, values);
        Assert.Contains(EasingType.CubicOut, values);
        Assert.Contains(EasingType.CubicInOut, values);
        Assert.Contains(EasingType.ElasticOut, values);
        Assert.Contains(EasingType.BounceOut, values);
    }
}
