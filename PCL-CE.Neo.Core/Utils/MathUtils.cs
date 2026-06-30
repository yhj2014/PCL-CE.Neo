using System;

namespace PCL_CE.Neo.Core.Utils;

public static class MathUtils
{
    public static int Clamp(int value, int min, int max)
    {
        return Math.Min(Math.Max(value, min), max);
    }

    public static long Clamp(long value, long min, long max)
    {
        return Math.Min(Math.Max(value, min), max);
    }

    public static float Clamp(float value, float min, float max)
    {
        return Math.Min(Math.Max(value, min), max);
    }

    public static double Clamp(double value, double min, double max)
    {
        return Math.Min(Math.Max(value, min), max);
    }

    public static int Lerp(int start, int end, float t)
    {
        return (int)Math.Round(start + (end - start) * t);
    }

    public static long Lerp(long start, long end, float t)
    {
        return (long)Math.Round(start + (end - start) * t);
    }

    public static float Lerp(float start, float end, float t)
    {
        return start + (end - start) * t;
    }

    public static double Lerp(double start, double end, double t)
    {
        return start + (end - start) * t;
    }

    public static float InverseLerp(float start, float end, float value)
    {
        if (Math.Abs(end - start) < float.Epsilon)
            return 0;

        return (value - start) / (end - start);
    }

    public static double InverseLerp(double start, double end, double value)
    {
        if (Math.Abs(end - start) < double.Epsilon)
            return 0;

        return (value - start) / (end - start);
    }

    public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        var t = InverseLerp(fromMin, fromMax, value);
        return Lerp(toMin, toMax, t);
    }

    public static double Remap(double value, double fromMin, double fromMax, double toMin, double toMax)
    {
        var t = InverseLerp(fromMin, fromMax, value);
        return Lerp(toMin, toMax, t);
    }

    public static float SmoothStep(float edge0, float edge1, float x)
    {
        x = Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
        return x * x * (3.0f - 2.0f * x);
    }

    public static double SmoothStep(double edge0, double edge1, double x)
    {
        x = Clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
        return x * x * (3.0 - 2.0 * x);
    }

    public static float SmoothDamp(float current, float target, ref float velocity, float smoothTime, float maxSpeed = float.MaxValue, float deltaTime = 1f)
    {
        smoothTime = Math.Max(0.0001f, smoothTime);
        var omega = 2.0f / smoothTime;

        var x = omega * deltaTime;
        var exp = 1.0f / (1.0f + x + 0.48f * x * x + 0.235f * x * x * x);

        var change = current - target;
        var originalTo = target;

        var maxChange = maxSpeed * smoothTime;
        change = Clamp(change, -maxChange, maxChange);
        target = current - change;

        var temp = (velocity + omega * change) * deltaTime;
        velocity = (velocity - omega * temp) * exp;

        var output = target + (change + temp) * exp;

        if (originalTo - current > 0.0f == output > originalTo)
        {
            output = originalTo;
            velocity = (output - originalTo) / deltaTime;
        }

        return output;
    }

    public static double SmoothDamp(double current, double target, ref double velocity, double smoothTime, double maxSpeed = double.MaxValue, double deltaTime = 1)
    {
        smoothTime = Math.Max(0.0001, smoothTime);
        var omega = 2.0 / smoothTime;

        var x = omega * deltaTime;
        var exp = 1.0 / (1.0 + x + 0.48 * x * x + 0.235 * x * x * x);

        var change = current - target;
        var originalTo = target;

        var maxChange = maxSpeed * smoothTime;
        change = Clamp(change, -maxChange, maxChange);
        target = current - change;

        var temp = (velocity + omega * change) * deltaTime;
        velocity = (velocity - omega * temp) * exp;

        var output = target + (change + temp) * exp;

        if (originalTo - current > 0.0 == output > originalTo)
        {
            output = originalTo;
            velocity = (output - originalTo) / deltaTime;
        }

        return output;
    }

    public static float RoundToNearest(float value, float nearest)
    {
        if (Math.Abs(nearest) < float.Epsilon)
            return value;

        return (float)Math.Round(value / nearest) * nearest;
    }

    public static double RoundToNearest(double value, double nearest)
    {
        if (Math.Abs(nearest) < double.Epsilon)
            return value;

        return Math.Round(value / nearest) * nearest;
    }

    public static int RoundToNearest(int value, int nearest)
    {
        if (nearest == 0)
            return value;

        return (int)Math.Round((double)value / nearest) * nearest;
    }

    public static float FloorToNearest(float value, float nearest)
    {
        if (Math.Abs(nearest) < float.Epsilon)
            return value;

        return (float)Math.Floor(value / nearest) * nearest;
    }

    public static double FloorToNearest(double value, double nearest)
    {
        if (Math.Abs(nearest) < double.Epsilon)
            return value;

        return Math.Floor(value / nearest) * nearest;
    }

    public static int FloorToNearest(int value, int nearest)
    {
        if (nearest == 0)
            return value;

        return (int)Math.Floor((double)value / nearest) * nearest;
    }

    public static float CeilToNearest(float value, float nearest)
    {
        if (Math.Abs(nearest) < float.Epsilon)
            return value;

        return (float)Math.Ceiling(value / nearest) * nearest;
    }

    public static double CeilToNearest(double value, double nearest)
    {
        if (Math.Abs(nearest) < double.Epsilon)
            return value;

        return Math.Ceiling(value / nearest) * nearest;
    }

    public static int CeilToNearest(int value, int nearest)
    {
        if (nearest == 0)
            return value;

        return (int)Math.Ceiling((double)value / nearest) * nearest;
    }

    public static bool IsPowerOfTwo(int value)
    {
        return value > 0 && (value & (value - 1)) == 0;
    }

    public static bool IsPowerOfTwo(long value)
    {
        return value > 0 && (value & (value - 1)) == 0;
    }

    public static bool IsPowerOfTwo(float value)
    {
        if (value <= 0)
            return false;

        var bits = BitConverter.SingleToInt32Bits(value);
        return (bits & (bits - 1)) == 0;
    }

    public static bool IsPowerOfTwo(double value)
    {
        if (value <= 0)
            return false;

        var bits = BitConverter.DoubleToInt64Bits(value);
        return (bits & (bits - 1)) == 0;
    }

    public static int NextPowerOfTwo(int value)
    {
        if (value <= 0)
            return 1;

        value--;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        return value + 1;
    }

    public static long NextPowerOfTwo(long value)
    {
        if (value <= 0)
            return 1;

        value--;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        value |= value >> 32;
        return value + 1;
    }

    public static float NextPowerOfTwo(float value)
    {
        return (float)Math.Pow(2, Math.Ceiling(Math.Log(value, 2)));
    }

    public static double NextPowerOfTwo(double value)
    {
        return Math.Pow(2, Math.Ceiling(Math.Log(value, 2)));
    }

    public static float DegreesToRadians(float degrees)
    {
        return degrees * (float)Math.PI / 180.0f;
    }

    public static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    public static float RadiansToDegrees(float radians)
    {
        return radians * 180.0f / (float)Math.PI;
    }

    public static double RadiansToDegrees(double radians)
    {
        return radians * 180.0 / Math.PI;
    }

    public static float NormalizeAngle(float degrees)
    {
        degrees %= 360.0f;
        if (degrees < 0)
            degrees += 360.0f;
        return degrees;
    }

    public static double NormalizeAngle(double degrees)
    {
        degrees %= 360.0;
        if (degrees < 0)
            degrees += 360.0;
        return degrees;
    }

    public static float AngleDifference(float angle1, float angle2)
    {
        var diff = NormalizeAngle(angle1) - NormalizeAngle(angle2);
        if (diff > 180)
            diff -= 360;
        if (diff < -180)
            diff += 360;
        return diff;
    }

    public static double AngleDifference(double angle1, double angle2)
    {
        var diff = NormalizeAngle(angle1) - NormalizeAngle(angle2);
        if (diff > 180)
            diff -= 360;
        if (diff < -180)
            diff += 360;
        return diff;
    }

    public static float Average(params float[] values)
    {
        if (values == null || values.Length == 0)
            return 0;

        float sum = 0;
        foreach (var value in values)
            sum += value;

        return sum / values.Length;
    }

    public static double Average(params double[] values)
    {
        if (values == null || values.Length == 0)
            return 0;

        double sum = 0;
        foreach (var value in values)
            sum += value;

        return sum / values.Length;
    }

    public static int Average(params int[] values)
    {
        if (values == null || values.Length == 0)
            return 0;

        int sum = 0;
        foreach (var value in values)
            sum += value;

        return sum / values.Length;
    }

    public static float Median(params float[] values)
    {
        if (values == null || values.Length == 0)
            return 0;

        var sorted = (float[])values.Clone();
        Array.Sort(sorted);

        var mid = sorted.Length / 2;
        return sorted.Length % 2 == 0 ? (sorted[mid - 1] + sorted[mid]) / 2.0f : sorted[mid];
    }

    public static double Median(params double[] values)
    {
        if (values == null || values.Length == 0)
            return 0;

        var sorted = (double[])values.Clone();
        Array.Sort(sorted);

        var mid = sorted.Length / 2;
        return sorted.Length % 2 == 0 ? (sorted[mid - 1] + sorted[mid]) / 2.0 : sorted[mid];
    }

    public static int Median(params int[] values)
    {
        if (values == null || values.Length == 0)
            return 0;

        var sorted = (int[])values.Clone();
        Array.Sort(sorted);

        var mid = sorted.Length / 2;
        return sorted.Length % 2 == 0 ? (sorted[mid - 1] + sorted[mid]) / 2 : sorted[mid];
    }
}