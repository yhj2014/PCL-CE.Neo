namespace PCL_CE.Neo.Core.App.Cli;

public record SubcommandDefinition(string Name, IEnumerable<SubcommandDefinition>? Subcommands = null)
{
    public IReadOnlyDictionary<string, SubcommandDefinition> SubcommandMap { get; } = 
        Subcommands?.ToDictionary(s => s.Name) ?? new Dictionary<string, SubcommandDefinition>();

    public bool Contains(string name) => SubcommandMap.ContainsKey(name);
}