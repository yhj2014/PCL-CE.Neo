using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Network;

namespace PCL_CE.Neo.Core.Minecraft.IdentityModel.OAuth;

public sealed class SimpleOAuthClient(OAuthClientOptions options) : IOAuthClient
{
    private const string ModuleName = "SimpleOAuthClient";

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

    public async Task<AuthorizeResult?> AuthorizeWithCodeAsync(
        string code, CancellationToken token, Dictionary<string, string>? extData = null)
    {
        try
        {
            extData ??= new Dictionary<string, string>();
            extData["client_id"] = options.ClientId;
            extData["grant_type"] = "authorization_code";
            extData["code"] = code;
            var client = options.GetClient.Invoke();
            using var content = new FormUrlEncodedContent(extData);
            var response = await NetworkService.PostAsync(options.Meta.TokenEndpoint, content, client, token);
            return await response.Content.ReadFromJsonAsync<AuthorizeResult>(cancellationToken: token);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "使用授权代码获取令牌失败");
            return null;
        }
    }

    public async Task<DeviceCodeData?> GetCodePairAsync(
        string[] scopes, CancellationToken token, Dictionary<string, string>? extData = null)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(options.Meta.DeviceEndpoint);
            var client = options.GetClient.Invoke();
            extData ??= new Dictionary<string, string>();
            extData["scope"] = string.Join(" ", scopes);
            extData["client_id"] = options.ClientId;
            var content = new FormUrlEncodedContent(extData);
            var response = await NetworkService.PostAsync(options.Meta.DeviceEndpoint, content, client, token);
            return await response.Content.ReadFromJsonAsync<DeviceCodeData>(cancellationToken: token);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "获取设备代码对失败");
            return null;
        }
    }

    public async Task<AuthorizeResult?> AuthorizeWithDeviceAsync(
        DeviceCodeData data, CancellationToken token, Dictionary<string, string>? extData = null)
    {
        try
        {
            if (data.IsError) throw new OperationCanceledException(data.ErrorDescription);
            var client = options.GetClient.Invoke();
            extData ??= new Dictionary<string, string>();
            extData["client_id"] = options.ClientId;
            extData["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code";
            extData["device_code"] = data.DeviceCode!;
            var content = new FormUrlEncodedContent(extData);
            var response = await NetworkService.PostAsync(options.Meta.TokenEndpoint, content, client, token);
            return await response.Content.ReadFromJsonAsync<AuthorizeResult>(cancellationToken: token);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "验证用户授权状态失败");
            return null;
        }
    }

    public async Task<AuthorizeResult?> AuthorizeWithSilentAsync(
        AuthorizeResult data, CancellationToken token, Dictionary<string, string>? extData = null)
    {
        try
        {
            var client = options.GetClient.Invoke();
            if (data.IsError) throw new OperationCanceledException(data.ErrorDescription);
            extData ??= [];
            extData["refresh_token"] = data.RefreshToken!;
            extData["grant_type"] = "refresh_token";
            extData["client_id"] = options.ClientId;
            var content = new FormUrlEncodedContent(extData);
            var response = await NetworkService.PostAsync(options.Meta.TokenEndpoint, content, client, token);
            return await response.Content.ReadFromJsonAsync<AuthorizeResult>(cancellationToken: token);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "刷新登录失败");
            return null;
        }
    }
}