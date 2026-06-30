using System;

namespace PCL_CE.Neo.Core.Utils;

public static class EaseUtils
{
    public static double Linear(double t) => t;

    public static double EaseInQuad(double t) => t * t;

    public static double EaseOutQuad(double t) => t * (2 - t);

    public static double EaseInOutQuad(double t) => t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;

    public static double EaseInCubic(double t) => t * t * t;

    public static double EaseOutCubic(double t) => (--t) * t * t + 1;

    public static double EaseInOutCubic(double t) => t < 0.5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;

    public static double EaseInQuart(double t) => t * t * t * t;

    public static double EaseOutQuart(double t) => 1 - (--t) * t * t * t;

    public static double EaseInOutQuart(double t) => t < 0.5 ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;

    public static double EaseInQuint(double t) => t * t * t * t * t;

    public static double EaseOutQuint(double t) => 1 + (--t) * t * t * t * t;

    public static double EaseInOutQuint(double t) => t < 0.5 ? 16 * t * t * t * t * t : 1 + 16 * (--t) * t * t * t * t;

    public static double EaseInSine(double t) => 1 - Math.Cos(t * Math.PI / 2);

    public static double EaseOutSine(double t) => Math.Sin(t * Math.PI / 2);

    public static double EaseInOutSine(double t) => -(Math.Cos(Math.PI * t) - 1) / 2;

    public static double EaseInExpo(double t) => t == 0 ? 0 : Math.Pow(2, 10 * (t - 1));

    public static double EaseOutExpo(double t) => t == 1 ? 1 : 1 - Math.Pow(2, -10 * t);

    public static double EaseInOutExpo(double t)
    {
        if (t == 0) return 0;
        if (t == 1) return 1;
        if (t < 0.5) return Math.Pow(2, 20 * t - 10) / 2;
        return (2 - Math.Pow(2, -20 * t + 10)) / 2;
    }

    public static double EaseInCirc(double t) => 1 - Math.Sqrt(1 - t * t);

    public static double EaseOutCirc(double t) => Math.Sqrt(1 - (--t) * t);

    public static double EaseInOutCirc(double t) => t < 0.5 ? (1 - Math.Sqrt(1 - 4 * t * t)) / 2 : (Math.Sqrt(1 - 4 * (--t) * t) + 1) / 2;

    public static double EaseInBack(double t)
    {
        const double c1 = 1.70158;
        const double c3 = c1 + 1;
        return c3 * t * t * t - c1 * t * t;
    }

    public static double EaseOutBack(double t)
    {
        const double c1 = 1.70158;
        const double c3 = c1 + 1;
        return 1 + c3 * Math.Pow(t - 1, 3) + c1 * Math.Pow(t - 1, 2);
    }

    public static double EaseInOutBack(double t)
    {
        const double c1 = 1.70158;
        const double c2 = c1 * 1.525;
        return t < 0.5 ? (Math.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2 : (Math.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;
    }

    public static double EaseInElastic(double t)
    {
        const double c4 = (2 * Math.PI) / 3;
        return t == 0 ? 0 : t == 1 ? 1 : -Math.Pow(2, 10 * t - 10) * Math.Sin((t * 10 - 10.75) * c4);
    }

    public static double EaseOutElastic(double t)
    {
        const double c4 = (2 * Math.PI) / 3;
        return t == 0 ? 0 : t == 1 ? 1 : Math.Pow(2, -10 * t) * Math.Sin((t * 10 - 0.75) * c4) + 1;
    }

    public static double EaseInOutElastic(double t)
    {
        const double c5 = (2 * Math.PI) / 4.5;
        return t == 0 ? 0 : t == 1 ? 1 : t < 0.5 ? -(Math.Pow(2, 20 * t - 10) * Math.Sin((20 * t - 11.125) * c5)) / 2 : (Math.Pow(2, -20 * t + 10) * Math.Sin((20 * t - 11.125) * c5)) / 2 + 1;
    }

    public static double EaseOutBounce(double t)
    {
        const double n1 = 7.5625;
        const double d1 = 2.75;

        if (t < 1 / d1)
            return n1 * t * t;
        if (t < 2 / d1)
            return n1 * (t -= 1.5 / d1) * t + 0.75;
        if (t < 2.5 / d1)
            return n1 * (t -= 2.25 / d1) * t + 0.9375;
        return n1 * (t -= 2.625 / d1) * t + 0.984375;
    }

    public static double EaseInBounce(double t) => 1 - EaseOutBounce(1 - t);

    public static double EaseInOutBounce(double t) => t < 0.5 ? (1 - EaseOutBounce(1 - 2 * t)) / 2 : (1 + EaseOutBounce(2 * t - 1)) / 2;

    public static Func<double, double> GetEasingFunction(EasingType type)
    {
        return type switch
        {
            EasingType.Linear => Linear,
            EasingType.EaseInQuad => EaseInQuad,
            EasingType.EaseOutQuad => EaseOutQuad,
            EasingType.EaseInOutQuad => EaseInOutQuad,
            EasingType.EaseInCubic => EaseInCubic,
            EasingType.EaseOutCubic => EaseOutCubic,
            EasingType.EaseInOutCubic => EaseInOutCubic,
            EasingType.EaseInQuart => EaseInQuart,
            EasingType.EaseOutQuart => EaseOutQuart,
            EasingType.EaseInOutQuart => EaseInOutQuart,
            EasingType.EaseInQuint => EaseInQuint,
            EasingType.EaseOutQuint => EaseOutQuint,
            EasingType.EaseInOutQuint => EaseInOutQuint,
            EasingType.EaseInSine => EaseInSine,
            EasingType.EaseOutSine => EaseOutSine,
            EasingType.EaseInOutSine => EaseInOutSine,
            EasingType.EaseInExpo => EaseInExpo,
            EasingType.EaseOutExpo => EaseOutExpo,
            EasingType.EaseInOutExpo => EaseInOutExpo,
            EasingType.EaseInCirc => EaseInCirc,
            EasingType.EaseOutCirc => EaseOutCirc,
            EasingType.EaseInOutCirc => EaseInOutCirc,
            EasingType.EaseInBack => EaseInBack,
            EasingType.EaseOutBack => EaseOutBack,
            EasingType.EaseInOutBack => EaseInOutBack,
            EasingType.EaseInElastic => EaseInElastic,
            EasingType.EaseOutElastic => EaseOutElastic,
            EasingType.EaseInOutElastic => EaseInOutElastic,
            EasingType.EaseInBounce => EaseInBounce,
            EasingType.EaseOutBounce => EaseOutBounce,
            EasingType.EaseInOutBounce => EaseInOutBounce,
            _ => Linear
        };
    }
}

public enum EasingType
{
    Linear,
    EaseInQuad,
    EaseOutQuad,
    EaseInOutQuad,
    EaseInCubic,
    EaseOutCubic,
    EaseInOutCubic,
    EaseInQuart,
    EaseOutQuart,
    EaseInOutQuart,
    EaseInQuint,
    EaseOutQuint,
    EaseInOutQuint,
    EaseInSine,
    EaseOutSine,
    EaseInOutSine,
    EaseInExpo,
    EaseOutExpo,
    EaseInOutExpo,
    EaseInCirc,
    EaseOutCirc,
    EaseInOutCirc,
    EaseInBack,
    EaseOutBack,
    EaseInOutBack,
    EaseInElastic,
    EaseOutElastic,
    EaseInOutElastic,
    EaseInBounce,
    EaseOutBounce,
    EaseInOutBounce
}