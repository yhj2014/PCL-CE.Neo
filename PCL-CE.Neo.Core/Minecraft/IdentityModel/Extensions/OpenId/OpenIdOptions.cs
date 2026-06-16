using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using PCL_CE.Neo.Core.Minecraft.IdentityModel.Extensions.JsonWebToken;
using PCL_CE.Neo.Core.Minecraft.IdentityModel.OAuth;
using PCL_CE.Neo.Core.Network;

namespace PCL_CE.Neo.Core.Minecraft.IdentityModel.Extensions.OpenId;

public record OpenIdOptions
{
    public required string OpenIdDiscoveryAddress { get; set; }
    public required string ClientId { get; set; }
    public bool OnlyDeviceAuthorize { get; set; }
    public string? RedirectUri { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public bool EnablePkceSupport { get; set; } = true;
    public required Func<HttpClient> GetClient { get; set; }
    public OpenIdMetadata? Meta { get; internal set; }

    public virtual async Task InitializeAsync(CancellationToken token)
    {
        try
        {
            var response = await NetworkService.GetAsync(OpenIdDiscoveryAddress, GetClient.Invoke(), token);
            Meta = await response.Content.ReadFromJsonAsync<OpenIdMetadata>(cancellationToken: token);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "OpenIdOptions", "拉取 OpenID 配置信息失败");
            throw;
        }
    }

    public async Task<JsonWebKey> GetSignatureKeyAsync(string kid, CancellationToken token)
    {
        if (Meta?.JwksUri is null) throw new InvalidOperationException("OpenID 元数据未初始化");
        
        try
        {
            var response = await NetworkService.GetAsync(Meta.JwksUri, GetClient.Invoke(), token);
            var result = await response.Content.ReadFromJsonAsync<JsonWebKeys>(cancellationToken: token);
            return result?.Keys.Single(k => k.Kid == kid) ?? throw new FormatException("找不到指定的密钥");
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "OpenIdOptions", "获取签名密钥失败");
            throw;
        }
    }

    public virtual async Task<OAuthClientOptions> BuildOAuthOptionsAsync(CancellationToken token)
    {
        if (Meta is null) throw new InvalidOperationException("OpenID 元数据未初始化");
        if (!OnlyDeviceAuthorize) ArgumentException.ThrowIfNullOrEmpty(RedirectUri);
        
        return new OAuthClientOptions
        {
            GetClient = GetClient,
            ClientId = ClientId,
            RedirectUri = OnlyDeviceAuthorize ? string.Empty : RedirectUri!,
            Meta = new EndpointMeta
            {
                AuthorizeEndpoint = Meta.AuthorizationEndpoint ?? string.Empty,
                DeviceEndpoint = Meta.DeviceAuthorizationEndpoint ?? string.Empty,
                TokenEndpoint = Meta.TokenEndpoint,
            }
        };
    }
}