using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Diff;

public interface IBinaryDiff
{
    Task<byte[]> ApplyAsync(byte[] originData, byte[] diffData);
    Task<byte[]> MakeAsync(byte[] originData, byte[] newData);
}