namespace PCL_CE.Neo.Core.Utils.Diff;

public interface IBinaryDiff
{
    byte[] CreatePatch(byte[] oldData, byte[] newData);
    byte[] ApplyPatch(byte[] oldData, byte[] patch);
    long CalculateDiffSize(byte[] oldData, byte[] newData);
}