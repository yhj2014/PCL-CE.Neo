using System;

namespace PCL.CE.Neo.Core.Utils;

public static class EaseUtils
{
    public static double Linear(double t) => t;

    public static double EaseInQuad(double t) => t * t;

    public static double EaseOutQuad(double t) => t * (2 - t);

    public static double EaseInOutQuad(double t) => t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;

    public static double EaseInCubic(double t) => t * t * t;

    public static double EaseOutCubic(double t) => (--t) * t * t + 1;

    public static double EaseInOutCubic(double t) => t < 0.5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;

    public static double EaseInSine(double t) => -Math.Cos(t * Math.PI / 2) + 1;

    public static double EaseOutSine(double t) => Math.Sin(t * Math.PI / 2);

    public static double EaseInOutSine(double t) => -(Math.Cos(Math.PI * t) - 1) / 2;

    public static double EaseInExpo(double t) => t == 0 ? 0 : Math.Pow(2, 10 * (t - 1));

    public static double EaseOutExpo(double t) => t == 1 ? 1 : 1 - Math.Pow(2, -10 * t);

    public static double EaseInOutExpo(double t)
    {
        if (t == 0) return 0;
        if (t == 1) return 1;
        return t < 0.5 ? Math.Pow(2, 20 * t - 10) / 2 : (2 - Math.Pow(2, -20 * t + 10)) / 2;
    }

    public static double EaseInCirc(double t) => -Math.Sqrt(1 - t * t) + 1;

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

    public static double EaseInOutBounce(double t)
    {
        return t < 0.5 ? (1 - EaseOutBounce(1 - 2 * t)) / 2 : (1 + EaseOutBounce(2 * t - 1)) / 2;
    }

    public static double GetEasedValue(double t, EaseType easeType)
    {
        return easeType switch
        {
            EaseType.Linear => Linear(t),
            EaseType.EaseInQuad => EaseInQuad(t),
            EaseType.EaseOutQuad => EaseOutQuad(t),
            EaseType.EaseInOutQuad => EaseInOutQuad(t),
            EaseType.EaseInCubic => EaseInCubic(t),
            EaseType.EaseOutCubic => EaseOutCubic(t),
            EaseType.EaseInOutCubic => EaseInOutCubic(t),
            EaseType.EaseInSine => EaseInSine(t),
            EaseType.EaseOutSine => EaseOutSine(t),
            EaseType.EaseInOutSine => EaseInOutSine(t),
            EaseType.EaseInExpo => EaseInExpo(t),
            EaseType.EaseOutExpo => EaseOutExpo(t),
            EaseType.EaseInOutExpo => EaseInOutExpo(t),
            EaseType.EaseInCirc => EaseInCirc(t),
            EaseType.EaseOutCirc => EaseOutCirc(t),
            EaseType.EaseInOutCirc => EaseInOutCirc(t),
            EaseType.EaseInBack => EaseInBack(t),
            EaseType.EaseOutBack => EaseOutBack(t),
            EaseType.EaseInOutBack => EaseInOutBack(t),
            EaseType.EaseOutElastic => EaseOutElastic(t),
            EaseType.EaseInOutElastic => EaseInOutElastic(t),
            EaseType.EaseOutBounce => EaseOutBounce(t),
            EaseType.EaseInOutBounce => EaseInOutBounce(t),
            _ => Linear(t)
        };
    }
}

public enum EaseType
{
    Linear,
    EaseInQuad,
    EaseOutQuad,
    EaseInOutQuad,
    EaseInCubic,
    EaseOutCubic,
    EaseInOutCubic,
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
    EaseOutElastic,
    EaseInOutElastic,
    EaseOutBounce,
    EaseInOutBounce
}