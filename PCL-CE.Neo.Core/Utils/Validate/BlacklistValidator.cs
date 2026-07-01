namespace PCL_CE.Neo.Core.Utils.Validate;

public class BlacklistValidator : IValidator<string>
{
    private readonly HashSet<string> _blacklist;
    private readonly bool _caseSensitive;

    public BlacklistValidator(IEnumerable<string> blacklist, bool caseSensitive = false)
    {
        _blacklist = caseSensitive 
            ? new HashSet<string>(blacklist) 
            : new HashSet<string>(blacklist, StringComparer.OrdinalIgnoreCase);
        _caseSensitive = caseSensitive;
    }

    public bool Validate(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return true;

        string normalizedValue = _caseSensitive ? value : value.ToLowerInvariant();
        foreach (string item in _blacklist)
        {
            string normalizedItem = _caseSensitive ? item : item.ToLowerInvariant();
            if (normalizedValue.Contains(normalizedItem))
                return false;
        }
        return true;
    }

    public string GetErrorMessage(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        string normalizedValue = _caseSensitive ? value : value.ToLowerInvariant();
        foreach (string item in _blacklist)
        {
            string normalizedItem = _caseSensitive ? item : item.ToLowerInvariant();
            if (normalizedValue.Contains(normalizedItem))
                return $"值包含黑名单中的内容: {item}";
        }
        return string.Empty;
    }
}