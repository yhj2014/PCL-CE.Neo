using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Minecraft.IdentityModel.Yggdrasil;

public sealed class YggdrasilLegacyClient(YggdrasilLegacyAuthenticateOptions options)
{
    public async Task<YggdrasilAuthenticateResult?> AuthenticateAsync(CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(options.Username);
        ArgumentException.ThrowIfNullOrEmpty(options.Password);

        var credential = new YggdrasilCredential
        {
            User = options.Username,
            Password = options.Password,
        };
        var address = $"{options.YggdrasilApiLocation}/authserver/authenticate";

        var client = options.GetClient.Invoke();
        using var response = await client.PostAsJsonAsync(address, credential, token).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<YggdrasilAuthenticateResult>(token).ConfigureAwait(false);
    }

    public async Task<YggdrasilAuthenticateResult?> RefreshAsync(CancellationToken token, Profile? selectedProfile)
    {
        ArgumentException.ThrowIfNullOrEmpty(options.AccessToken);

        var refreshData = new YggdrasilRefresh()
        {
            AccessToken = options.AccessToken
        };
        if (selectedProfile is not null) refreshData.SelectedProfile = selectedProfile;

        var address = $"{options.YggdrasilApiLocation}/authserver/refresh";

        var client = options.GetClient.Invoke();
        using var response = await client.PostAsJsonAsync(address, refreshData, token).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<YggdrasilAuthenticateResult>(token).ConfigureAwait(false);
    }

    public async Task<bool> ValidateAsync(CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(options.AccessToken);

        var validateData = new YggdrasilRefresh()
        {
            AccessToken = options.AccessToken
        };
        var address = $"{options.YggdrasilApiLocation}/authserver/validate";

        var client = options.GetClient.Invoke();
        using var response = await client.PostAsJsonAsync(address, validateData, token).ConfigureAwait(false);

        return response.StatusCode == HttpStatusCode.NoContent;
    }

    public async Task InvalidateAsync(CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(options.AccessToken);

        var validateData = new YggdrasilRefresh()
        {
            AccessToken = options.AccessToken
        };
        var address = $"{options.YggdrasilApiLocation}/authserver/invalidate";

        var client = options.GetClient.Invoke();
        using var _ = await client.PostAsJsonAsync(address, validateData, token).ConfigureAwait(false);
    }

    public async Task<(bool IsSuccess, string ErrorDescription)> SignOutAsync(CancellationToken token)
    {
        var signoutData = new JsonObject
        {
            ["username"] = options.Username,
            ["password"] = options.Password
        };
        var address = $"{options.YggdrasilApiLocation}/authserver/signout";

        var client = options.GetClient.Invoke();
        using var response = await client.PostAsJsonAsync(address, signoutData, token).ConfigureAwait(false);

        var content = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
        var data = JsonNode.Parse(content);
        return (response.StatusCode == HttpStatusCode.NoContent, data?["errorMessage"]?.ToString() ?? string.Empty);
    }
}