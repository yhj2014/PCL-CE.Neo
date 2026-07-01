using System.Text;

namespace PCL_CE.Neo.Core.Utils;

public class ArgumentsBuilder
{
    private readonly List<string> _arguments = new List<string>();

    public ArgumentsBuilder()
    {
    }

    public ArgumentsBuilder(string? initialArgument)
    {
        if (!string.IsNullOrEmpty(initialArgument))
            _arguments.Add(initialArgument);
    }

    public ArgumentsBuilder(IEnumerable<string> initialArguments)
    {
        if (initialArguments != null)
            _arguments.AddRange(initialArguments);
    }

    public ArgumentsBuilder Add(string? argument)
    {
        if (!string.IsNullOrEmpty(argument))
            _arguments.Add(argument);

        return this;
    }

    public ArgumentsBuilder AddRange(IEnumerable<string>? arguments)
    {
        if (arguments != null)
            _arguments.AddRange(arguments.Where(a => !string.IsNullOrEmpty(a)));

        return this;
    }

    public ArgumentsBuilder AddIf(bool condition, string? argument)
    {
        if (condition && !string.IsNullOrEmpty(argument))
            _arguments.Add(argument);

        return this;
    }

    public ArgumentsBuilder AddIf(bool condition, string? trueArgument, string? falseArgument)
    {
        if (condition)
        {
            if (!string.IsNullOrEmpty(trueArgument))
                _arguments.Add(trueArgument);
        }
        else
        {
            if (!string.IsNullOrEmpty(falseArgument))
                _arguments.Add(falseArgument);
        }

        return this;
    }

    public ArgumentsBuilder AddWithValue(string? prefix, string? value)
    {
        if (string.IsNullOrEmpty(prefix))
            return this;

        if (string.IsNullOrEmpty(value))
            return this;

        _arguments.Add($"{prefix}{value}");
        return this;
    }

    public ArgumentsBuilder AddWithValue(string? prefix, string? separator, string? value)
    {
        if (string.IsNullOrEmpty(prefix))
            return this;

        if (string.IsNullOrEmpty(value))
            return this;

        separator = separator ?? string.Empty;
        _arguments.Add($"{prefix}{separator}{value}");
        return this;
    }

    public ArgumentsBuilder AddIfNotNull(string? prefix, string? value)
    {
        return AddIf(value != null, $"{prefix}{value}");
    }

    public ArgumentsBuilder AddIfNotNullOrEmpty(string? prefix, string? value)
    {
        return AddIf(!string.IsNullOrEmpty(value), $"{prefix}{value}");
    }

    public ArgumentsBuilder AddIfNotNullOrWhiteSpace(string? prefix, string? value)
    {
        return AddIf(!string.IsNullOrWhiteSpace(value), $"{prefix}{value}");
    }

    public ArgumentsBuilder AddQuoted(string? argument)
    {
        if (string.IsNullOrEmpty(argument))
            return this;

        if (argument.Contains(' ') || argument.Contains('"') || argument.Contains('\''))
            _arguments.Add($"\"{argument.Replace("\"", "\\\"")}\"");
        else
            _arguments.Add(argument);

        return this;
    }

    public ArgumentsBuilder AddQuotedIfContainsSpace(string? argument)
    {
        if (string.IsNullOrEmpty(argument))
            return this;

        if (argument.Contains(' '))
            _arguments.Add($"\"{argument.Replace("\"", "\\\"")}\"");
        else
            _arguments.Add(argument);

        return this;
    }

    public ArgumentsBuilder AddSwitch(string? switchPrefix, bool condition)
    {
        if (condition && !string.IsNullOrEmpty(switchPrefix))
            _arguments.Add(switchPrefix);

        return this;
    }

    public ArgumentsBuilder AddSwitch(string? switchPrefix, bool condition, string? trueValue, string? falseValue)
    {
        if (condition)
        {
            if (!string.IsNullOrEmpty(trueValue))
                _arguments.Add($"{switchPrefix}{trueValue}");
        }
        else
        {
            if (!string.IsNullOrEmpty(falseValue))
                _arguments.Add($"{switchPrefix}{falseValue}");
        }

        return this;
    }

    public ArgumentsBuilder AddKeyValue(string? key, string? value, string? separator = "=")
    {
        if (string.IsNullOrEmpty(key))
            return this;

        separator = separator ?? "=";

        if (string.IsNullOrEmpty(value))
            _arguments.Add(key);
        else
            _arguments.Add($"{key}{separator}{value}");

        return this;
    }

    public ArgumentsBuilder AddKeyValueQuoted(string? key, string? value, string? separator = "=")
    {
        if (string.IsNullOrEmpty(key))
            return this;

        separator = separator ?? "=";

        if (string.IsNullOrEmpty(value))
            _arguments.Add(key);
        else
            _arguments.Add($"{key}{separator}\"{value.Replace("\"", "\\\"")}\"");

        return this;
    }

    public ArgumentsBuilder AddFormatted(string? format, params object?[] args)
    {
        if (string.IsNullOrEmpty(format))
            return this;

        _arguments.Add(string.Format(format, args));
        return this;
    }

    public ArgumentsBuilder Clear()
    {
        _arguments.Clear();
        return this;
    }

    public ArgumentsBuilder RemoveLast()
    {
        if (_arguments.Count > 0)
            _arguments.RemoveAt(_arguments.Count - 1);

        return this;
    }

    public ArgumentsBuilder RemoveAt(int index)
    {
        if (index >= 0 && index < _arguments.Count)
            _arguments.RemoveAt(index);

        return this;
    }

    public ArgumentsBuilder ReplaceAt(int index, string? newArgument)
    {
        if (index >= 0 && index < _arguments.Count && !string.IsNullOrEmpty(newArgument))
            _arguments[index] = newArgument;

        return this;
    }

    public int Count => _arguments.Count;

    public string this[int index] => _arguments[index];

    public IReadOnlyList<string> Arguments => _arguments.AsReadOnly();

    public string Build()
    {
        StringBuilder sb = new StringBuilder();
        foreach (string arg in _arguments)
        {
            if (sb.Length > 0)
                sb.Append(' ');

            if (arg.Contains(' ') || arg.Contains('"'))
                sb.Append($"\"{arg.Replace("\"", "\\\"")}\"");
            else
                sb.Append(arg);
        }

        return sb.ToString();
    }

    public string[] BuildArray()
    {
        return _arguments.ToArray();
    }

    public override string ToString()
    {
        return Build();
    }

    public static string Build(params string?[] arguments)
    {
        return new ArgumentsBuilder(arguments.Where(a => !string.IsNullOrEmpty(a))).Build();
    }

    public static string[] BuildArray(params string?[] arguments)
    {
        return arguments.Where(a => !string.IsNullOrEmpty(a)).ToArray();
    }
}