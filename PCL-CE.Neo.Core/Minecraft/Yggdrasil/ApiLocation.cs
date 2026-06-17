using System;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.IO.Net.Http;

namespace PCL_CE.Neo.Core.Minecraft.Yggdrasil;

public static class ApiLocation
{
    public static async Task<string> TryRequestAsync(string address)
    {
        var originAddr = address.StartsWith("http") ? address : $"https://{address}";
        var originUri = new Uri(originAddr);
        using var response = await HttpRequest
            .CreateHead(originAddr)
            .SendAsync();

        if (!response.TryGetHeader("X-Authlib-Injector-Api-Location", out var location)
            || location.Length == 0)
            return originAddr;

        var resultAddr = location[0];

        if (string.IsNullOrEmpty(resultAddr)) return originAddr;
        if (resultAddr.StartsWith(originUri.Scheme)) return resultAddr;
        if (resultAddr.StartsWith("http:") && originUri.Scheme == "https")
            return resultAddr.Replace("http", "https");

        return new Uri(originUri, resultAddr).ToString();
    }
}