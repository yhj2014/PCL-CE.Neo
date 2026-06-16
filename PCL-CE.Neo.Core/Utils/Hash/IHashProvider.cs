namespace PCL_CE.Neo.Core.Utils.Hash;

public interface IHashProvider
{
    string ComputeHash(byte[] data);
    string ComputeHash(string data);
    byte[] ComputeHashBytes(byte[] data);
}