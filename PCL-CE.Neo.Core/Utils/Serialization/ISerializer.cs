namespace PCL_CE.Neo.Core.Utils.Serialization;

public interface ISerializer
{
    string Serialize<T>(T obj);
    T? Deserialize<T>(string data);
    byte[] SerializeToBytes<T>(T obj);
    T? DeserializeFromBytes<T>(byte[] data);
}