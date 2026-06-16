using System;
using System.Net;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Logging;
using PCL_CE.Neo.Core.Network;

namespace PCL_CE.Neo.Core.Minecraft.IdentityModel.Yggdrasil;

public sealed class YggdrasilLegacyClient(YggdrasilLegacyAuthenticateOptions options)
{
    private const string ModuleName = "YggdrasilLegacyClient";

    public async Task<YggdrasilAuthenticateResult?> AuthenticateAsync(CancellationToken token)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(options.Username);
            ArgumentException.ThrowIfNullOrEmpty(options.Password);

            var credential = new YggdrasilCredential
            {
                User = options.Username,
                Password = options.Password,
            };
            var address = $"{options.YggdrasilApiLocation}/authserver/authenticate";

            var response = await NetworkService.PostJsonAsync(address, credential, options.GetClient.Invoke(), token);
            return await response.Content.ReadFromJsonAsync<YggdrasilAuthenticateResult>(cancellationToken: token);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "登录请求失败");
            return null;
        }
    }

    public async Task<YggdrasilAuthenticateResult?> RefreshAsync(CancellationToken token, Profile? selectedProfile)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(options.AccessToken);

            var refreshData = new YggdrasilRefresh()
            {
                AccessToken = options.AccessToken
            };
            if (selectedProfile is not null) refreshData.SelectedProfile = selectedProfile;

            var address = $"{options.YggdrasilApiLocation}/authserver/refresh";

            var response = await NetworkService.PostJsonAsync(address, refreshData, options.GetClient.Invoke(), token);
            return await response.Content.ReadFromJsonAsync<YggdrasilAuthenticateResult>(cancellationToken: token);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "刷新请求失败");
            return null;
        }
    }

    public async Task<bool> ValidateAsync(CancellationToken token)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(options.AccessToken);

            var validateData = new YggdrasilRefresh()
            {
                AccessToken = options.AccessToken
            };
            var address = $"{options.YggdrasilApiLocation}/authserver/validate";

            var response = await NetworkService.PostJsonAsync(address, validateData, options.GetClient.Invoke(), token);
            return response.StatusCode == HttpStatusCode.NoContent;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "验证请求失败");
            return false;
        }
    }

    public async Task InvalidateAsync(CancellationToken token)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(options.AccessToken);

            var validateData = new YggdrasilRefresh()
            {
                AccessToken = options.AccessToken
            };
            var address = $"{options.YggdrasilApiLocation}/authserver/invalidate";

            await NetworkService.PostJsonAsync(address, validateData, options.GetClient.Invoke(), token);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "注销请求失败");
        }
    }

    public async Task<(bool IsSuccess, string ErrorDescription)> SignOutAsync(CancellationToken token)
    {
        try
        {
            var signoutData = new JsonObject
            {
                ["username"] = options.Username,
                ["password"] = options.Password
            };
            var address = $"{options.YggdrasilApiLocation}/authserver/signout";

            var response = await NetworkService.PostJsonAsync(address, signoutData, options.GetClient.Invoke(), token);
            var data = JsonNode.Parse(await response.Content.ReadAsStringAsync(token));
            return (response.StatusCode == HttpStatusCode.NoContent, data?["errorMessage"]?.ToString() ?? string.Empty);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "登出请求失败");
            return (false, ex.Message);
        }
    }
}