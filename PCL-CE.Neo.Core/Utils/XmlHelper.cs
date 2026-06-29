using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace PCL_CE.Neo.Core.Utils;

public static class XmlHelper
{
    public static string Serialize<T>(T obj, bool indent = true)
    {
        try
        {
            var settings = new XmlWriterSettings { Indent = indent };

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);

            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(xmlWriter, obj);

            return stringWriter.ToString();
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to serialize object to XML");
            throw;
        }
    }

    public static T? Deserialize<T>(string xml)
    {
        try
        {
            using var stringReader = new StringReader(xml);

            var serializer = new XmlSerializer(typeof(T));
            return (T?)serializer.Deserialize(stringReader);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to deserialize XML");
            return default;
        }
    }

    public static void SerializeToFile<T>(T obj, string filePath, bool indent = true)
    {
        try
        {
            var settings = new XmlWriterSettings { Indent = indent };

            using var xmlWriter = XmlWriter.Create(filePath, settings);

            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(xmlWriter, obj);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to serialize object to XML file: {filePath}");
            throw;
        }
    }

    public static T? DeserializeFromFile<T>(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return default;

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            var serializer = new XmlSerializer(typeof(T));
            return (T?)serializer.Deserialize(fileStream);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to deserialize XML file: {filePath}");
            return default;
        }
    }

    public static bool TryDeserialize<T>(string xml, out T? result)
    {
        try
        {
            result = Deserialize<T>(xml);
            return result != null;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    public static string FormatXml(string xml)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true });

            doc.WriteTo(xmlWriter);
            return stringWriter.ToString();
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to format XML, returning original");
            return xml;
        }
    }

    public static bool IsValidXml(string xml)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return true;
        }
        catch
        {
            return false;
        }
    }
}