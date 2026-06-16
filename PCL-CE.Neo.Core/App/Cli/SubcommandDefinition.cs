using System.Collections.Generic;

namespace PCL_CE.Neo.Core.App.Cli;

public class SubcommandDefinition
{
    public required string CommandText { get; init; }

    public required IEnumerable<SubcommandDefinition> Subcommands { private get; init; }

    private IReadOnlyDictionary<string, SubcommandDefinition>? _subcommandMap;
    public IReadOnlyDictionary<string, SubcommandDefinition> SubcommandMap
    {
        get
        {
            if (_subcommandMap != null) return _subcommandMap;
            var map = new Dictionary<string, SubcommandDefinition>();
            foreach (var c in Subcommands) map[c.CommandText] = c;
            return _subcommandMap = map.AsReadOnly();
        }
    }

    public bool Contains(string subcommandText)
    {
        return SubcommandMap.ContainsKey(subcommandText);
    }

    public static implicit operator SubcommandDefinition((string commandText, IEnumerable<SubcommandDefinition> subcommands) tuple)
    {
        return new SubcommandDefinition
        {
            CommandText = tuple.commandText,
            Subcommands = tuple.subcommands
        };
    }

    public static implicit operator SubcommandDefinition(string commandText)
    {
        return new SubcommandDefinition
        {
            CommandText = commandText,
            Subcommands = []
        };
    }
}