namespace PCL_CE.Neo.Core.Minecraft.Yggdrasil;

public readonly struct ApiLocation
{
    public string AuthServerUrl { get; }
    public string AccountsServerUrl { get; }
    public string SessionServerUrl { get; }
    public string ServicesServerUrl { get; }

    public ApiLocation(string authServerUrl, string accountsServerUrl, string sessionServerUrl, string servicesServerUrl)
    {
        AuthServerUrl = authServerUrl;
        AccountsServerUrl = accountsServerUrl;
        SessionServerUrl = sessionServerUrl;
        ServicesServerUrl = servicesServerUrl;
    }

    public static ApiLocation Mojang => new(
        "https://authserver.mojang.com",
        "https://api.mojang.com",
        "https://sessionserver.mojang.com",
        "https://api.minecraftservices.com"
    );

    public static ApiLocation Microsoft => new(
        "https://authserver.mojang.com",
        "https://api.mojang.com",
        "https://sessionserver.mojang.com",
        "https://api.minecraftservices.com"
    );

    public static ApiLocation Offline => new("", "", "", "");
}