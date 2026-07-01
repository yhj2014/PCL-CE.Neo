namespace PCL_CE.Neo.Core.Utils;

public class ArgumentBuilder
{
    private readonly List<string> _arguments = new List<string>();

    public ArgumentBuilder Add(string argument)
    {
        if (!string.IsNullOrWhiteSpace(argument))
            _arguments.Add(argument);
        return this;
    }

    public ArgumentBuilder Add(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            return this;

        if (string.IsNullOrWhiteSpace(value))
            return this;

        if (value.Contains(' ') || value.Contains('"') || value.Contains('\''))
        {
            _arguments.Add($"{name}=\"{EscapeValue(value)}\"");
        }
        else
        {
            _arguments.Add($"{name}={value}");
        }

        return this;
    }

    public ArgumentBuilder Add(string name, bool value)
    {
        if (!string.IsNullOrWhiteSpace(name))
            _arguments.Add($"{name}={value.ToString().ToLowerInvariant()}");
        return this;
    }

    public ArgumentBuilder Add(string name, int value)
    {
        if (!string.IsNullOrWhiteSpace(name))
            _arguments.Add($"{name}={value}");
        return this;
    }

    public ArgumentBuilder Add(string name, long value)
    {
        if (!string.IsNullOrWhiteSpace(name))
            _arguments.Add($"{name}={value}");
        return this;
    }

    public ArgumentBuilder AddIf(bool condition, string argument)
    {
        if (condition)
            Add(argument);
        return this;
    }

    public ArgumentBuilder AddIf(bool condition, string name, string value)
    {
        if (condition)
            Add(name, value);
        return this;
    }

    public ArgumentBuilder AddIf(bool condition, string name, bool value)
    {
        if (condition)
            Add(name, value);
        return this;
    }

    public ArgumentBuilder AddRange(IEnumerable<string> arguments)
    {
        foreach (var arg in arguments)
            Add(arg);
        return this;
    }

    public ArgumentBuilder AddKeyValuePairs(IEnumerable<KeyValuePair<string, string>> pairs)
    {
        foreach (var pair in pairs)
            Add(pair.Key, pair.Value);
        return this;
    }

    public ArgumentBuilder Clear()
    {
        _arguments.Clear();
        return this;
    }

    public List<string> ToList()
    {
        return new List<string>(_arguments);
    }

    public string[] ToArray()
    {
        return _arguments.ToArray();
    }

    public override string ToString()
    {
        return string.Join(" ", _arguments.Select(EscapeArgument));
    }

    public string ToCommandLine()
    {
        return string.Join(" ", _arguments.Select(EscapeArgument));
    }

    private string EscapeArgument(string arg)
    {
        if (arg.Contains(' ') || arg.Contains('"') || arg.Contains('\'') || arg.Contains('\\'))
        {
            return $"\"{EscapeValue(arg)}\"";
        }
        return arg;
    }

    private string EscapeValue(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    public static ArgumentBuilder Create()
    {
        return new ArgumentBuilder();
    }
}