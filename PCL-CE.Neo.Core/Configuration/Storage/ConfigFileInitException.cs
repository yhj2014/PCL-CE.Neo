using System;

namespace PCL_CE.Neo.Core.Configuration.Storage;

public class ConfigFileInitException : Exception
{
    public string Path { get; }

    public ConfigFileInitException(string path, string message) : base(message) => Path = path;

    public ConfigFileInitException(string path, string message, Exception innerException) 
        : base(message, innerException) => Path = path;
}