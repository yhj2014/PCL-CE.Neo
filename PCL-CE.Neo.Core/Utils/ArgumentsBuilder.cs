using System;
using System.Collections.Generic;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils;

public class ArgumentsBuilder
{
    private readonly List<string> _arguments = new();

    public int Count => _arguments.Count;

    public ArgumentsBuilder Add(string argument)
    {
        if (!string.IsNullOrWhiteSpace(argument))
        {
            _arguments.Add(argument);
        }
        return this;
    }

    public ArgumentsBuilder AddIf(bool condition, string argument)
    {
        if (condition && !string.IsNullOrWhiteSpace(argument))
        {
            _arguments.Add(argument);
        }
        return this;
    }

    public ArgumentsBuilder AddRange(IEnumerable<string> arguments)
    {
        if (arguments != null)
        {
            _arguments.AddRange(arguments.Where(a => !string.IsNullOrWhiteSpace(a)));
        }
        return this;
    }

    public ArgumentsBuilder AddQuoted(string argument)
    {
        if (!string.IsNullOrWhiteSpace(argument))
        {
            _arguments.Add(_QuoteArgument(argument));
        }
        return this;
    }

    public ArgumentsBuilder AddQuotedIf(bool condition, string argument)
    {
        if (condition && !string.IsNullOrWhiteSpace(argument))
        {
            _arguments.Add(_QuoteArgument(argument));
        }
        return this;
    }

    public ArgumentsBuilder AddKeyValue(string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
        {
            _arguments.Add($"{key}={_QuoteArgument(value)}");
        }
        return this;
    }

    public ArgumentsBuilder AddKeyValueIf(bool condition, string key, string? value)
    {
        if (condition && !string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
        {
            _arguments.Add($"{key}={_QuoteArgument(value)}");
        }
        return this;
    }

    public ArgumentsBuilder AddFlag(string flag, bool condition = true)
    {
        if (condition && !string.IsNullOrWhiteSpace(flag))
        {
            _arguments.Add(flag);
        }
        return this;
    }

    public ArgumentsBuilder AddOption(string option, string? value)
    {
        if (!string.IsNullOrWhiteSpace(option) && !string.IsNullOrWhiteSpace(value))
        {
            _arguments.Add(option);
            _arguments.Add(_QuoteArgument(value));
        }
        return this;
    }

    public ArgumentsBuilder AddOptionIf(bool condition, string option, string? value)
    {
        if (condition && !string.IsNullOrWhiteSpace(option) && !string.IsNullOrWhiteSpace(value))
        {
            _arguments.Add(option);
            _arguments.Add(_QuoteArgument(value));
        }
        return this;
    }

    public ArgumentsBuilder AddOption(string option, int value)
    {
        if (!string.IsNullOrWhiteSpace(option))
        {
            _arguments.Add(option);
            _arguments.Add(value.ToString());
        }
        return this;
    }

    public ArgumentsBuilder AddOptionIf(bool condition, string option, int value)
    {
        if (condition && !string.IsNullOrWhiteSpace(option))
        {
            _arguments.Add(option);
            _arguments.Add(value.ToString());
        }
        return this;
    }

    public ArgumentsBuilder Clear()
    {
        _arguments.Clear();
        return this;
    }

    public string[] ToArray()
    {
        return _arguments.ToArray();
    }

    public List<string> ToList()
    {
        return new List<string>(_arguments);
    }

    public override string ToString()
    {
        return string.Join(" ", _arguments.Select(a => 
            a.Contains(' ') || a.Contains('"') ? $"\"{a}\"" : a));
    }

    private string _QuoteArgument(string argument)
    {
        if (string.IsNullOrEmpty(argument))
            return argument;

        if (argument.Contains(' ') || argument.Contains('"') || argument.Contains('\\'))
        {
            return $"\"{argument.Replace("\"", "\\\"").Replace("\\", "\\\\")}\"";
        }

        return argument;
    }
}