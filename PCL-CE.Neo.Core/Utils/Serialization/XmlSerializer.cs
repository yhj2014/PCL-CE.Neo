using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace PCL_CE.Neo.Core.Utils.Serialization;

public class XmlSerializer : ISerializer
{
    public string Serialize<T>(T obj)
    {
        using MemoryStream ms = new MemoryStream();
        using StreamWriter sw = new StreamWriter(ms, Encoding.UTF8);
        System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
        serializer.Serialize(sw, obj);
        ms.Position = 0;
        using StreamReader sr = new StreamReader(ms, Encoding.UTF8);
        return sr.ReadToEnd();
    }

    public T? Deserialize<T>(string data)
    {
        using MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(data));
        System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
        return (T?)serializer.Deserialize(ms);
    }

    public byte[] SerializeToBytes<T>(T obj)
    {
        string xml = Serialize(obj);
        return Encoding.UTF8.GetBytes(xml);
    }

    public T? DeserializeFromBytes<T>(byte[] data)
    {
        string xml = Encoding.UTF8.GetString(data);
        return Deserialize<T>(xml);
    }
}