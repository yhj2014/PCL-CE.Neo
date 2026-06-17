using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Minecraft.IdentityModel.OAuth;

public sealed class SimpleOAuthClient(OAuthClientOptions options) : IOAuthClient
{
    public string GetAuthorizeUrl(string[] scopes, string state, Dictionary<string, string>? extData = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Meta.AuthorizeEndpoint);
        var sb = new StringBuilder();
        sb.Append(options.Meta.AuthorizeEndpoint);
        sb.Append($"?response_type=code&scope={Uri.EscapeDataString(string.Join(" ", scopes))}");
        sb.Append($"&redirect_uri={Uri.EscapeDataString(options.RedirectUri)}");
        sb.Append($"&client_id={options.ClientId}&state={state}");
        if (extData is null) return sb.ToString();
        foreach (var kvp in extData)
            sb.Append($"&{kvp.Key}={Uri.EscapeDataString(kvp.Value)}");
        return sb.ToString();
    }

    public async Task<AuthorizeResult?> AuthorizeWithCodeAsync(string code, CancellationToken token, Dictionary<string, string>? extData = null)
    {
        extData ??= new Dictionary<string, string>();
        extData["client_id"] = options.ClientId;
        extData["grant_type"] = "authorization_code";
        extData["code"] = code;
        var client = options.GetClient.Invoke();
        using var content = new FormUrlEncodedContent(extData);

        using var request = new HttpRequestMessage(HttpMethod.Post, options.Meta.TokenEndpoint);
        request.Content = content;
        if (options.Headers != null)
        {
            foreach (var header in options.Headers)
                request.Headers.Add(header.Key, header.Value);
        }

        using var response = await client.SendAsync(request, token).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<AuthorizeResult>(token).ConfigureAwait(false);
    }

    public async Task<DeviceCodeData?> GetCodePairAsync(string[] scopes, CancellationToken token, Dictionary<string, string>? extData = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(options.Meta.DeviceEndpoint);
        var client = options.GetClient.Invoke();
        extData ??= new Dictionary<string, string>();
        extData["scope"] = string.Join(" ", scopes);
        extData["client_id"] = options.ClientId;
        var content = new FormUrlEncodedContent(extData);

        using var request = new HttpRequestMessage(HttpMethod.Post, options.Meta.DeviceEndpoint);
        request.Content = content;
        if (options.Headers != null)
        {
            foreach (var header in options.Headers)
                request.Headers.Add(header.Key, header.Value);
        }

        using var response = await client.SendAsync(request, token).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<DeviceCodeData>(token).ConfigureAwait(false);
    }

    public async Task<AuthorizeResult?> AuthorizeWithDeviceAsync(DeviceCodeData data, CancellationToken token, Dictionary<string, string>? extData = null)
    {
        if (data.IsError) throw new OperationCanceledException(data.ErrorDescription);
        var client = options.GetClient.Invoke();
        extData ??= new Dictionary<string, string>();
        extData["client_id"] = options.ClientId;
        extData["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code";
        extData["device_code"] = data.DeviceCode!;

        using var content = new FormUrlEncodedContent(extData);

        using var request = new HttpRequestMessage(HttpMethod.Post, options.Meta.TokenEndpoint);
        request.Content = content;
        if (options.Headers != null)
        {
            foreach (var header in options.Headers)
                request.Headers.Add(header.Key, header.Value);
        }

        using var response = await client.SendAsync(request, token).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<AuthorizeResult>(token).ConfigureAwait(false);
    }

    public async Task<AuthorizeResult?> AuthorizeWithSilentAsync(AuthorizeResult data, CancellationToken token, Dictionary<string, string>? extData = null)
    {
        var client = options.GetClient.Invoke();
        if (data.IsError) throw new OperationCanceledException(data.ErrorDescription);
        extData ??= [];
        extData["refresh_token"] = data.RefreshToken!;
        extData["grant_type"] = "refresh_token";
        extData["client_id"] = options.ClientId;
        using var content = new FormUrlEncodedContent(extData);

        using var request = new HttpRequestMessage(HttpMethod.Post, options.Meta.TokenEndpoint);
        request.Content = content;
        if (options.Headers != null)
        {
            foreach (var header in options.Headers)
                request.Headers.Add(header.Key, header.Value);
        }

        using var response = await client.SendAsync(request, token).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<AuthorizeResult>(token).ConfigureAwait(false);
    }
}