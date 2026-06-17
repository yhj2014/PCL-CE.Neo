using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Minecraft.IdentityModel.Extensions.OpenId;
using PCL_CE.Neo.Core.Minecraft.IdentityModel.OAuth;

namespace PCL_CE.Neo.Core.Minecraft.IdentityModel.Extensions.YggdrasilConnect;

public class YggdrasilClient(YggdrasilOptions options) : IOAuthClient
{
    private OpenIdClient? _client;
    private readonly YggdrasilOptions _options = options;

    public async Task InitializeAsync(CancellationToken token)
    {
        _client = new OpenIdClient(_options);
        await _client.InitializeAsync(token, true);
    }

    public string GetAuthorizeUrl(string[] scopes, string state, Dictionary<string, string>? extData)
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