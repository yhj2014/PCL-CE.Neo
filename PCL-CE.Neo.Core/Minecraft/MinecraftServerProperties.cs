using System;
using System.Collections.Generic;
using System.IO;

namespace PCL_CE.Neo.Core.Minecraft;

public class MinecraftServerProperties
{
    private readonly Dictionary<string, string> _properties = new();

    public string this[string key]
    {
        get => _properties.TryGetValue(key, out var value) ? value : string.Empty;
        set => _properties[key] = value;
    }

    public static MinecraftServerProperties? Load(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            var properties = new MinecraftServerProperties();
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith('#'))
                    continue;

                var index = trimmedLine.IndexOf('=');
                if (index > 0)
                {
                    var key = trimmedLine.Substring(0, index).Trim();
                    var value = trimmedLine.Substring(index + 1).Trim();
                    properties[key] = value;
                }
            }

            return properties;
        }
        catch
        {
            return null;
        }
    }

    public bool Save(string filePath)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var writer = new StreamWriter(filePath);
            writer.WriteLine("# Minecraft server properties");
            writer.WriteLine($"# Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            writer.WriteLine();

            foreach (var pair in _properties)
            {
                writer.WriteLine($"{pair.Key}={pair.Value}");
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public int GetInt(string key, int defaultValue = 0)
    {
        if (_properties.TryGetValue(key, out var value) && int.TryParse(value, out var result))
            return result;
        return defaultValue;
    }

    public bool GetBool(string key, bool defaultValue = false)
    {
        if (_properties.TryGetValue(key, out var value))
            return value.Equals("true", StringComparison.OrdinalIgnoreCase);
        return defaultValue;
    }

    public string GetString(string key, string defaultValue = "")
    {
        return _properties.TryGetValue(key, out var value) ? value : defaultValue;
    }

    public void SetInt(string key, int value)
    {
        _properties[key] = value.ToString();
    }

    public void SetBool(string key, bool value)
    {
        _properties[key] = value ? "true" : "false";
    }

    public void SetString(string key, string value)
    {
        _properties[key] = value;
    }

    public bool ContainsKey(string key)
    {
        return _properties.ContainsKey(key);
    }

    public void Remove(string key)
    {
        _properties.Remove(key);
    }

    public Dictionary<string, string> ToDictionary()
    {
        return new Dictionary<string, string>(_properties);
    }
}