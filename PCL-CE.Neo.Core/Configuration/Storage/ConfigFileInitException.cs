using System;

namespace PCL_CE.Neo.Core.Configuration.Storage;

public class ConfigFileInitException : Exception
{
    public string FilePath { get; }

    public ConfigFileInitException(string filePath, string message) : base(message)
    {
        FilePath = filePath;
    }

    public ConfigFileInitException(string filePath, string message, Exception innerException) 
        : base(message, innerException)
    {
        FilePath = filePath;
    }
}