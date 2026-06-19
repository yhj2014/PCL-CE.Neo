namespace PCL_CE.Neo.Core.Utils.Validate;

public class BlacklistValidator
{
    private readonly IEnumerable<string> _blacklist;

    public BlacklistValidator(IEnumerable<string> blacklist)
    {
        _blacklist = blacklist ?? throw new ArgumentNullException(nameof(blacklist));
    }

    public bool Validate(string input)
    {
        if (string.IsNullOrEmpty(input))
            return true;

        return !_blacklist.Contains(input, StringComparer.OrdinalIgnoreCase);
    }
}