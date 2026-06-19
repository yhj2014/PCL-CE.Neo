namespace PCL_CE.Neo.Core.Utils;

public static class EaseUtils
{
    public static double Linear(double t)
    {
        return t;
    }

    public static double EaseInQuad(double t)
    {
        return t * t;
    }

    public static double EaseOutQuad(double t)
    {
        return t * (2 - t);
    }

    public static double EaseInOutQuad(double t)
    {
        return t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;
    }

    public static double EaseInCubic(double t)
    {
        return t * t * t;
    }

    public static double EaseOutCubic(double t)
    {
        return (--t) * t * t + 1;
    }

    public static double EaseInOutCubic(double t)
    {
        return t < 0.5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
    }

    public static double EaseInExpo(double t)
    {
        return t == 0 ? 0 : Math.Pow(2, 10 * (t - 1));
    }

    public static double EaseOutExpo(double t)
    {
        return t == 1 ? 1 : 1 - Math.Pow(2, -10 * t);
    }

    public static double EaseInOutExpo(double t)
    {
        if (t == 0) return 0;
        if (t == 1) return 1;
        return t < 0.5 ? Math.Pow(2, 20 * t - 10) / 2 : (2 - Math.Pow(2, -20 * t + 10)) / 2;
    }

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
        return t < 0.5
            ? (Math.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2
            : (Math.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;
    }

    public static double EaseInBounce(double t)
    {
        return 1 - EaseOutBounce(1 - t);
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
        return t < 0.5
            ? (1 - EaseOutBounce(1 - 2 * t)) / 2
            : (1 + EaseOutBounce(2 * t - 1)) / 2;
    }
}