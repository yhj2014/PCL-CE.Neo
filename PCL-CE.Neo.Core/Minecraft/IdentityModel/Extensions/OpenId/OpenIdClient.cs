using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Minecraft.IdentityModel.Extensions.Pkce;
using PCL_CE.Neo.Core.Minecraft.IdentityModel.OAuth;
using PCL_CE.Neo.Core.Utils.Exts;

namespace PCL_CE.Neo.Core.Minecraft.IdentityModel.Extensions.OpenId;

public class OpenIdClient(OpenIdOptions options) : IOAuthClient
{
    private IOAuthClient? _client;

    public async Task InitializeAsync(CancellationToken token, bool checkAddress = false)
    {
        var opt = await options.BuildOAuthOptionsAsync(token);
        if (!checkAddress || opt.Meta.AuthorizeEndpoint.IsNullOrEmpty() || opt.Meta.DeviceEndpoint.IsNullOrEmpty())
        {
            _client = options.EnablePkceSupport ? new PkceClient(opt) : new SimpleOAuthClient(opt);
            return;
        }

        throw new InvalidOperationException();
    }

    public string GetAuthorizeUrl(string[] scopes, string state, Dictionary<string, string>? extData = null)
    {
        if (_client is null) throw new InvalidOperationException();
        return _client.GetAuthorizeUrl(scopes, state, extData);
    }

    public async Task<AuthorizeResult?> AuthorizeWithCodeAsync(string code, CancellationToken token, Dictionary<string, string>? extData = null)
    {
        if (_client is null) throw new InvalidOperationException();
        return await _client.AuthorizeWithCodeAsync(code, token, extData);
    }

    public async Task<DeviceCodeData?> GetCodePairAsync(string[] scopes, CancellationToken token, Dictionary<string, string>? extData = null)
    {
        if (_client is null) throw new InvalidOperationException();
        return await _client.GetCodePairAsync(scopes, token, extData);
    }

    public async Task<AuthorizeResult?> AuthorizeWithDeviceAsync(DeviceCodeData data, CancellationToken token, Dictionary<string, string>? extData = null)
    {
        if (_client is null) throw new InvalidOperationException();
        return await _client.AuthorizeWithDeviceAsync(data, token, extData);
    }

    public async Task<AuthorizeResult?> AuthorizeWithSilentAsync(AuthorizeResult data, CancellationToken token, Dictionary<string, string>? extData = null)
    {
        if (_client is null) throw new InvalidOperationException();
        return await _client.AuthorizeWithSilentAsync(data, token, extData);
    }
}