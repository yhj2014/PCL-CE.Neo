namespace PCL_CE.Neo.Core.IO.Download;

public interface IDlResourceMapping<out TMappingValue>
{
    TMappingValue? Parse(string resId);
}