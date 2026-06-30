using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PCL_CE.Neo.Core.Utils;

public class IniParser
{
    private readonly Dictionary<string, Dictionary<string, string>> _sections = new();
    private readonly string _filePath;
    private bool _modified;

    public IniParser(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    public bool Load()
    {
        if (!File.Exists(_filePath))
            return false;

        try
        {
            var lines = File.ReadAllLines(_filePath);
            ParseLines(lines);
            _modified = false;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool LoadFromString(string content)
    {
        if (string.IsNullOrEmpty(content))
            return false;

        try
        {
            var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            ParseLines(lines);
            _modified = false;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void ParseLines(string[] lines)
    {
        _sections.Clear();
        string currentSection = string.Empty;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            if (string.IsNullOrEmpty(trimmed))
                continue;

            if (trimmed.StartsWith("#") || trimmed.StartsWith(";"))
                continue;

            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
            {
                currentSection = trimmed.Substring(1, trimmed.Length - 2).Trim();
                if (!_sections.ContainsKey(currentSection))
                {
                    _sections[currentSection] = new Dictionary<string, string>();
                }
                continue;
            }

            var equalsIndex = trimmed.IndexOf('=');
            if (equalsIndex > 0)
            {
                var key = trimmed.Substring(0, equalsIndex).Trim();
                var value = equalsIndex < trimmed.Length - 1 ? trimmed.Substring(equalsIndex + 1).Trim() : string.Empty;

                if (!string.IsNullOrEmpty(key))
                {
                    if (!_sections.ContainsKey(currentSection))
                    {
                        _sections[currentSection] = new Dictionary<string, string>();
                    }
                    _sections[currentSection][key] = value;
                }
            }
        }
    }

    public bool Save()
    {
        try
        {
            var content = ToString();
            File.WriteAllText(_filePath, content, Encoding.UTF8);
            _modified = false;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var section in _sections)
        {
            if (!string.IsNullOrEmpty(section.Key))
            {
                sb.AppendLine($"[{section.Key}]");
            }

            foreach (var pair in section.Value)
            {
                sb.AppendLine($"{pair.Key}={pair.Value}");
            }

            if (!string.IsNullOrEmpty(section.Key) && section.Value.Count > 0)
            {
                sb.AppendLine();
            }
        }

        return sb.ToString().TrimEnd();
    }

    public bool HasSection(string section)
    {
        return _sections.ContainsKey(section ?? string.Empty);
    }

    public bool HasKey(string section, string key)
    {
        if (!HasSection(section))
            return false;

        return _sections[section ?? string.Empty].ContainsKey(key ?? string.Empty);
    }

    public string GetValue(string section, string key, string defaultValue = "")
    {
        var sectionKey = section ?? string.Empty;
        var keyKey = key ?? string.Empty;

        if (_sections.TryGetValue(sectionKey, out var sectionDict) &&
            sectionDict.TryGetValue(keyKey, out var value))
        {
            return value;
        }

        return defaultValue;
    }

    public int GetIntValue(string section, string key, int defaultValue = 0)
    {
        var value = GetValue(section, key);
        if (int.TryParse(value, out var result))
            return result;

        return defaultValue;
    }

    public long GetLongValue(string section, string key, long defaultValue = 0)
    {
        var value = GetValue(section, key);
        if (long.TryParse(value, out var result))
            return result;

        return defaultValue;
    }

    public bool GetBoolValue(string section, string key, bool defaultValue = false)
    {
        var value = GetValue(section, key).Trim().ToLowerInvariant();

        return value switch
        {
            "true" => true,
            "false" => false,
            "1" => true,
            "0" => false,
            "yes" => true,
            "no" => false,
            "on" => true,
            "off" => false,
            _ => defaultValue
        };
    }

    public double GetDoubleValue(string section, string key, double defaultValue = 0)
    {
        var value = GetValue(section, key);
        if (double.TryParse(value, out var result))
            return result;

        return defaultValue;
    }

    public void SetValue(string section, string key, string value)
    {
        var sectionKey = section ?? string.Empty;
        var keyKey = key ?? string.Empty;

        if (!_sections.ContainsKey(sectionKey))
        {
            _sections[sectionKey] = new Dictionary<string, string>();
        }

        _sections[sectionKey][keyKey] = value ?? string.Empty;
        _modified = true;
    }

    public void SetIntValue(string section, string key, int value)
    {
        SetValue(section, key, value.ToString());
    }

    public void SetLongValue(string section, string key, long value)
    {
        SetValue(section, key, value.ToString());
    }

    public void SetBoolValue(string section, string key, bool value)
    {
        SetValue(section, key, value.ToString().ToLowerInvariant());
    }

    public void SetDoubleValue(string section, string key, double value)
    {
        SetValue(section, key, value.ToString());
    }

    public void RemoveKey(string section, string key)
    {
        var sectionKey = section ?? string.Empty;
        var keyKey = key ?? string.Empty;

        if (_sections.TryGetValue(sectionKey, out var sectionDict))
        {
            if (sectionDict.Remove(keyKey))
            {
                _modified = true;
            }
        }
    }

    public void RemoveSection(string section)
    {
        var sectionKey = section ?? string.Empty;

        if (_sections.Remove(sectionKey))
        {
            _modified = true;
        }
    }

    public IEnumerable<string> GetSections()
    {
        return _sections.Keys;
    }

    public IEnumerable<string> GetKeys(string section)
    {
        var sectionKey = section ?? string.Empty;

        if (_sections.TryGetValue(sectionKey, out var sectionDict))
        {
            return sectionDict.Keys;
        }

        return Enumerable.Empty<string>();
    }

    public IDictionary<string, string> GetSectionValues(string section)
    {
        var sectionKey = section ?? string.Empty;

        if (_sections.TryGetValue(sectionKey, out var sectionDict))
        {
            return new Dictionary<string, string>(sectionDict);
        }

        return new Dictionary<string, string>();
    }

    public bool IsModified => _modified;

    public static IniParser FromFile(string filePath)
    {
        var parser = new IniParser(filePath);
        parser.Load();
        return parser;
    }

    public static IniParser FromString(string content)
    {
        var parser = new IniParser(string.Empty);
        parser.LoadFromString(content);
        return parser;
    }
}