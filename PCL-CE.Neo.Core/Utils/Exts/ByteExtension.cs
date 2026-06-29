using System;

namespace PCL.CE.Neo.Core.Utils.Exts;

public static class ByteExtension
{
    public static string ToHexString(this ReadOnlySpan<byte> bytes) => Convert.ToHexString(bytes).ToLower();
    
    public static string FromByteToB64(this ReadOnlySpan<byte> bytes) => Convert.ToBase64String(bytes);
    
    public static string FromBytesToB64UrlSafe(this ReadOnlySpan<byte> bytes) 
        => bytes.FromByteToB64().FromB64ToB64UrlSafe();
    
    public static string FromB64ToB64UrlSafe(this string base64)
    {
        return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
    
    public static string FromB64UrlSafeToB64(this string base64UrlSafe)
    {
        var padded = base64UrlSafe.Replace('-', '+').Replace('_', '/');
        while (padded.Length % 4 != 0)
        {
            padded += '=';
        }
        return padded;
    }
}