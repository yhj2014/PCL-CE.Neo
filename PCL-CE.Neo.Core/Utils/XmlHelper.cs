using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace PCL_CE.Neo.Core.Utils;

public static class XmlHelper
{
    public static XDocument Load(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("XML file not found", filePath);

        return XDocument.Load(filePath);
    }

    public static XDocument LoadFromString(string content)
    {
        if (string.IsNullOrEmpty(content))
            throw new ArgumentNullException(nameof(content));

        return XDocument.Parse(content);
    }

    public static void Save(XDocument document, string filePath)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        document.Save(filePath);
    }

    public static string ToString(XDocument document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        return document.ToString();
    }

    public static string ToString(XDocument document, bool formatted)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        if (!formatted)
            return document.ToString();

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\r\n",
            Encoding = Encoding.UTF8
        };

        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, settings);
        document.WriteTo(writer);
        writer.Flush();
        return sw.ToString();
    }

    public static XElement? GetElement(XContainer parent, string name)
    {
        if (parent == null)
            throw new ArgumentNullException(nameof(parent));

        return parent.Elements().FirstOrDefault(e => e.Name.LocalName == name);
    }

    public static XElement? GetElement(XContainer parent, XName name)
    {
        if (parent == null)
            throw new ArgumentNullException(nameof(parent));

        return parent.Elements(name).FirstOrDefault();
    }

    public static IEnumerable<XElement> GetElements(XContainer parent, string name)
    {
        if (parent == null)
            throw new ArgumentNullException(nameof(parent));

        return parent.Elements().Where(e => e.Name.LocalName == name);
    }

    public static IEnumerable<XElement> GetElements(XContainer parent, XName name)
    {
        if (parent == null)
            throw new ArgumentNullException(nameof(parent));

        return parent.Elements(name);
    }

    public static string GetValue(XElement element)
    {
        if (element == null)
            return string.Empty;

        return element.Value;
    }

    public static string GetValue(XElement element, string defaultValue)
    {
        if (element == null)
            return defaultValue;

        return element.Value;
    }

    public static string GetAttributeValue(XElement element, string attributeName)
    {
        if (element == null)
            return string.Empty;

        var attribute = element.Attribute(attributeName);
        return attribute?.Value ?? string.Empty;
    }

    public static string GetAttributeValue(XElement element, string attributeName, string defaultValue)
    {
        if (element == null)
            return defaultValue;

        var attribute = element.Attribute(attributeName);
        return attribute?.Value ?? defaultValue;
    }

    public static int GetIntValue(XElement element)
    {
        if (element == null)
            return 0;

        if (int.TryParse(element.Value, out var result))
            return result;

        return 0;
    }

    public static int GetIntValue(XElement element, int defaultValue)
    {
        if (element == null)
            return defaultValue;

        if (int.TryParse(element.Value, out var result))
            return result;

        return defaultValue;
    }

    public static long GetLongValue(XElement element)
    {
        if (element == null)
            return 0;

        if (long.TryParse(element.Value, out var result))
            return result;

        return 0;
    }

    public static long GetLongValue(XElement element, long defaultValue)
    {
        if (element == null)
            return defaultValue;

        if (long.TryParse(element.Value, out var result))
            return result;

        return defaultValue;
    }

    public static bool GetBoolValue(XElement element)
    {
        if (element == null)
            return false;

        var value = element.Value.Trim().ToLowerInvariant();

        return value switch
        {
            "true" => true,
            "false" => false,
            "1" => true,
            "0" => false,
            "yes" => true,
            "no" => false,
            _ => false
        };
    }

    public static bool GetBoolValue(XElement element, bool defaultValue)
    {
        if (element == null)
            return defaultValue;

        var value = element.Value.Trim().ToLowerInvariant();

        return value switch
        {
            "true" => true,
            "false" => false,
            "1" => true,
            "0" => false,
            "yes" => true,
            "no" => false,
            _ => defaultValue
        };
    }

    public static double GetDoubleValue(XElement element)
    {
        if (element == null)
            return 0;

        if (double.TryParse(element.Value, out var result))
            return result;

        return 0;
    }

    public static double GetDoubleValue(XElement element, double defaultValue)
    {
        if (element == null)
            return defaultValue;

        if (double.TryParse(element.Value, out var result))
            return result;

        return defaultValue;
    }

    public static DateTime GetDateTimeValue(XElement element)
    {
        if (element == null)
            return DateTime.MinValue;

        if (DateTime.TryParse(element.Value, out var result))
            return result;

        return DateTime.MinValue;
    }

    public static DateTime GetDateTimeValue(XElement element, DateTime defaultValue)
    {
        if (element == null)
            return defaultValue;

        if (DateTime.TryParse(element.Value, out var result))
            return result;

        return defaultValue;
    }

    public static XElement CreateElement(string name, string value)
    {
        return new XElement(name, value);
    }

    public static XElement CreateElement(XName name, string value)
    {
        return new XElement(name, value);
    }

    public static XElement CreateElement(string name, params object[] content)
    {
        return new XElement(name, content);
    }

    public static XElement CreateElement(XName name, params object[] content)
    {
        return new XElement(name, content);
    }

    public static XAttribute CreateAttribute(string name, string value)
    {
        return new XAttribute(name, value);
    }

    public static XAttribute CreateAttribute(XName name, string value)
    {
        return new XAttribute(name, value);
    }

    public static XDocument CreateDocument(XElement root)
    {
        if (root == null)
            throw new ArgumentNullException(nameof(root));

        return new XDocument(root);
    }

    public static XDocument CreateDocument(string rootName)
    {
        return new XDocument(new XElement(rootName));
    }

    public static XDocument CreateDocument(XName rootName)
    {
        return new XDocument(new XElement(rootName));
    }

    public static void AddElement(XContainer parent, string name, string value)
    {
        if (parent == null)
            throw new ArgumentNullException(nameof(parent));

        parent.Add(new XElement(name, value));
    }

    public static void AddElement(XContainer parent, XName name, string value)
    {
        if (parent == null)
            throw new ArgumentNullException(nameof(parent));

        parent.Add(new XElement(name, value));
    }

    public static void AddAttribute(XElement element, string name, string value)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        element.Add(new XAttribute(name, value));
    }

    public static void AddAttribute(XElement element, XName name, string value)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        element.Add(new XAttribute(name, value));
    }

    public static void RemoveElement(XElement element)
    {
        element?.Remove();
    }

    public static void RemoveAttribute(XElement element, string name)
    {
        if (element == null)
            return;

        var attribute = element.Attribute(name);
        attribute?.Remove();
    }

    public static bool HasElement(XContainer parent, string name)
    {
        if (parent == null)
            return false;

        return parent.Elements().Any(e => e.Name.LocalName == name);
    }

    public static bool HasElement(XContainer parent, XName name)
    {
        if (parent == null)
            return false;

        return parent.Elements(name).Any();
    }

    public static bool HasAttribute(XElement element, string name)
    {
        if (element == null)
            return false;

        return element.Attribute(name) != null;
    }

    public static bool HasAttribute(XElement element, XName name)
    {
        if (element == null)
            return false;

        return element.Attribute(name) != null;
    }

    public static string SelectSingleNodeValue(XContainer parent, string xpath)
    {
        if (parent == null)
            throw new ArgumentNullException(nameof(parent));

        var element = parent.XPathSelectElement(xpath);
        return element?.Value ?? string.Empty;
    }

    public static IEnumerable<XElement> SelectNodes(XContainer parent, string xpath)
    {
        if (parent == null)
            throw new ArgumentNullException(nameof(parent));

        return parent.XPathSelectElements(xpath);
    }
}