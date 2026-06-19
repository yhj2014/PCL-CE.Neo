using System.Text;

namespace PCL_CE.Neo.Core.Utils;

public class ArgumentsBuilder
{
    private readonly List<string> _arguments = new();

    public ArgumentsBuilder Add(string argument)
    {
        if (!string.IsNullOrWhiteSpace(argument))
            _arguments.Add(argument);
        return this;
    }

    public ArgumentsBuilder Add(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            return this;

        if (string.IsNullOrWhiteSpace(value))
            _arguments.Add(name);
        else
            _arguments.Add($"{name}={value}");

        return this;
    }

    public ArgumentsBuilder AddIf(bool condition, string argument)
    {
        if (condition && !string.IsNullOrWhiteSpace(argument))
            _arguments.Add(argument);
        return this;
    }

    public ArgumentsBuilder AddIf(bool condition, string name, string value)
    {
        if (condition)
            Add(name, value);
        return this;
    }

    public ArgumentsBuilder AddRange(IEnumerable<string> arguments)
    {
        if (arguments != null)
            _arguments.AddRange(arguments.Where(arg => !string.IsNullOrWhiteSpace(arg)));
        return this;
    }

    public ArgumentsBuilder AddQuoted(string argument)
    {
        if (!string.IsNullOrWhiteSpace(argument))
            _arguments.Add(Quote(argument));
        return this;
    }

    public ArgumentsBuilder AddQuoted(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            return this;

        if (string.IsNullOrWhiteSpace(value))
            _arguments.Add(name);
        else
            _arguments.Add($"{name}={Quote(value)}");

        return this;
    }

    public string[] ToArray()
    {
        return _arguments.ToArray();
    }

    public override string ToString()
    {
        return string.Join(" ", _arguments.Select(QuoteIfNeeded));
    }

    private static string Quote(string value)
    {
        return $"\"{value.Replace("\"", "\\\"")}\"";
    }

    private static string QuoteIfNeeded(string value)
    {
        if (value.Contains(' ') || value.Contains('"') || value.Contains('\''))
            return Quote(value);
        return value;
    }
}