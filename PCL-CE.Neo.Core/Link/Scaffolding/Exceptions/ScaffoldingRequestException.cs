using System;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Exceptions;

public class ScaffoldingRequestException(byte statusCode, string? serverMessage = null) : Exception(_BuildMessage(
    statusCode,
    serverMessage))
{
    public byte StatusCode { get; } = statusCode;

    public string? ServerMessage { get; } = serverMessage;

    private static string _BuildMessage(byte statusCode, string? serverMessage)
    {
        var errorType = statusCode switch
        {
            >= 32 and < 64 => "Protocol-defined error",
            255 => "Unknown error",
            _ => "Generic error"
        };

        return string.IsNullOrEmpty(serverMessage)
            ? $"Scaffolding request failed with status code {statusCode} ({errorType})."
            : $"Scaffolding request failed with status code {statusCode} ({errorType}): {serverMessage}";
    }
}