using System;
using System.Diagnostics;

namespace PCL_CE.Neo.Core.Utils;

public static class FrameUtils
{
    public static long Frequency { get; }

    static FrameUtils()
    {
        Frequency = Stopwatch.Frequency;
    }

    public static long NowStamp() => Stopwatch.GetTimestamp();

    public static long StampToFrameIndex(long startStamp, int fps)
    {
        var durationStamp = NowStamp() - startStamp;
        if (durationStamp <= 0) return 0;

        var index = durationStamp * fps / Frequency;

        return index;
    }

    public static long TimeSpanToFrameIndex(TimeSpan startTime, TimeSpan currentTime, int fps)
    {
        if (currentTime < startTime) return 0;

        var duration = currentTime - startTime;
        var index = (long)(duration.TotalSeconds * fps);

        return index;
    }
}