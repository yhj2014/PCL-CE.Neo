using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using PCL_CE.Neo.Core.IO.Net.Http;
using PCL_CE.Neo.Core.Minecraft.IdentityModel.Extensions.JsonWebToken;
using PCL_CE.Neo.Core.Minecraft.IdentityModel.OAuth;

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
        using var response = await HttpRequest
            .Create(OpenIdDiscoveryAddress)
            .WithHeaders(Headers ?? [])
            .SendAsync(GetClient.Invoke(), cancellationToken: token)
            .ConfigureAwait(false);

        Meta = await response
            .AsJsonAsync<OpenIdMetadata>(cancellationToken: token)
            .ConfigureAwait(false);
    }

    public async Task<JsonWebKey> GetSignatureKeyAsync(string kid, CancellationToken token)
    {
        if (Meta?.JwksUri is null) throw new InvalidOperationException();
        using var response = await HttpRequest.Create(Meta.JwksUri)
            .WithHeaders(Headers ?? [])
            .SendAsync(GetClient.Invoke())
            .ConfigureAwait(false);

        var result = await response
            .AsJsonAsync<JsonWebKeys>(cancellationToken: token)
            .ConfigureAwait(false);
        return result?.Keys.Single(k => k.Kid == kid)
               ?? throw new FormatException();
    }

    public virtual async Task<OAuthClientOptions> BuildOAuthOptionsAsync(CancellationToken token)
    {
        if (Meta is null) throw new InvalidOperationException();
        if (!OnlyDeviceAuthorize) ArgumentException.ThrowIfNullOrEmpty(RedirectUri);
        return new OAuthClientOptions
        {
            GetClient = GetClient,
            ClientId = ClientId,
            RedirectUri = OnlyDeviceAuthorize ? string.Empty : RedirectUri!,
            Meta = new EndpointMeta
            {
                AuthorizeEndpoint = Meta?.AuthorizationEndpoint ?? string.Empty,
                DeviceEndpoint = Meta?.DeviceAuthorizationEndpoint ?? string.Empty,
                TokenEndpoint = Meta!.TokenEndpoint,
            }
        };
    }
}