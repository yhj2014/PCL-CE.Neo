namespace PCL_CE.Neo.Core.Utils.Validate;

public static class NullValidator
{
    public static bool IsNull(object? value)
    {
        return value is null;
    }

    public static bool IsNotNull(object? value)
    {
        return value is not null;
    }

    public static bool IsNullOrEmpty(string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static bool IsNotNullOrEmpty(string? value)
    {
        return !string.IsNullOrEmpty(value);
    }

    public static bool IsNullOrWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    public static bool IsNotNullOrWhiteSpace(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    public static bool IsNullOrEmpty<T>(IEnumerable<T>? collection)
    {
        return collection is null || !collection.Any();
    }

    public static bool IsNotNullOrEmpty<T>(IEnumerable<T>? collection)
    {
        return collection is not null && collection.Any();
    }

    public static T? ThrowIfNull<T>(T? value, string? paramName = null)
    {
        if (value is null)
            throw new ArgumentNullException(paramName);
        return value;
    }

    public static string ThrowIfNullOrEmpty(string? value, string? paramName = null)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty.", paramName);
        return value;
    }

    public static string ThrowIfNullOrWhiteSpace(string? value, string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
        return value;
    }
}