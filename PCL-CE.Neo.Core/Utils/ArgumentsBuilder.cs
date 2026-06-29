using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PCL_CE.Neo.Core.Utils;

public class ArgumentsBuilder
{
    private readonly List<string> _arguments = new();
    private readonly Dictionary<string, string> _namedArguments = new();
    private bool _useQuotes = true;

    public ArgumentsBuilder() { }

    public ArgumentsBuilder(bool useQuotes)
    {
        _useQuotes = useQuotes;
    }

    public ArgumentsBuilder Add(string argument)
    {
        if (!string.IsNullOrEmpty(argument))
            _arguments.Add(argument);
        return this;
    }

    public ArgumentsBuilder Add(params string[] arguments)
    {
        foreach (var arg in arguments)
        {
            Add(arg);
        }
        return this;
    }

    public ArgumentsBuilder AddIf(bool condition, string argument)
    {
        if (condition && !string.IsNullOrEmpty(argument))
            _arguments.Add(argument);
        return this;
    }

    public ArgumentsBuilder AddIf(bool condition, params string[] arguments)
    {
        if (condition)
            Add(arguments);
        return this;
    }

    public ArgumentsBuilder AddNamed(string key, string value)
    {
        if (!string.IsNullOrEmpty(key) && value != null)
            _namedArguments[key] = value;
        return this;
    }

    public ArgumentsBuilder AddNamed(string key, bool value)
    {
        if (!string.IsNullOrEmpty(key))
            _namedArguments[key] = value.ToString();
        return this;
    }

    public ArgumentsBuilder AddNamed(string key, int value)
    {
        if (!string.IsNullOrEmpty(key))
            _namedArguments[key] = value.ToString();
        return this;
    }

    public ArgumentsBuilder AddNamed(string key, long value)
    {
        if (!string.IsNullOrEmpty(key))
            _namedArguments[key] = value.ToString();
        return this;
    }

    public ArgumentsBuilder AddNamedIf(bool condition, string key, string value)
    {
        if (condition)
            AddNamed(key, value);
        return this;
    }

    public ArgumentsBuilder AddNamedIf(bool condition, string key, bool value)
    {
        if (condition)
            AddNamed(key, value);
        return this;
    }

    public ArgumentsBuilder AddNamedIf(bool condition, string key, int value)
    {
        if (condition)
            AddNamed(key, value);
        return this;
    }

    public ArgumentsBuilder AddNamedIf(bool condition, string key, long value)
    {
        if (condition)
            AddNamed(key, value);
        return this;
    }

    public ArgumentsBuilder Remove(string argument)
    {
        _arguments.Remove(argument);
        return this;
    }

    public ArgumentsBuilder RemoveNamed(string key)
    {
        _namedArguments.Remove(key);
        return this;
    }

    public ArgumentsBuilder Clear()
    {
        _arguments.Clear();
        _namedArguments.Clear();
        return this;
    }

    public ArgumentsBuilder ClearNamed()
    {
        _namedArguments.Clear();
        return this;
    }

    public string Build()
    {
        var result = new StringBuilder();

        foreach (var arg in _arguments)
        {
            if (result.Length > 0)
                result.Append(' ');
            result.Append(QuoteIfNeeded(arg));
        }

        foreach (var pair in _namedArguments)
        {
            if (result.Length > 0)
                result.Append(' ');
            result.Append($"{pair.Key}={QuoteIfNeeded(pair.Value)}");
        }

        return result.ToString();
    }

    public List<string> BuildList()
    {
        var list = new List<string>(_arguments);
        
        foreach (var pair in _namedArguments)
        {
            list.Add($"{pair.Key}={pair.Value}");
        }

        return list;
    }

    public string BuildCommandLine()
    {
        return Build();
    }

    public override string ToString()
    {
        return Build();
    }

    private string QuoteIfNeeded(string value)
    {
        if (!_useQuotes)
            return value;

        if (value.Contains(' ') || value.Contains('"') || value.Contains('\'') || value.Contains('\\'))
        {
            var escaped = value.Replace("\"", "\\\"");
            return $"\"{escaped}\"";
        }

        return value;
    }

    public int Count => _arguments.Count + _namedArguments.Count;

    public bool IsEmpty => Count == 0;
}