namespace PCL_CE.Neo.Core.IO.Download;

public class NDlTaskSegment
{
    public long StartOffset { get; set; }
    public long EndOffset { get; set; }
    public long Downloaded { get; set; }
    public bool Completed { get; set; }
}