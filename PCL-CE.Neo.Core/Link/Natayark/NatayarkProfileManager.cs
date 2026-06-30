using System;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Link.Natayark;

public static class NatayarkProfileManager
{
    private static readonly ILogger<NatayarkProfileManager> _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<NatayarkProfileManager>.Instance;

    public static NaidUser NaidProfile { get; private set; } = new();

    private static Task? _getNaidData;

    public static async Task GetNaidDataAsync(string token, bool isRefresh = false, bool isRetry = false, ushort port = 0)
    {
        if (_getNaidData != null && !_getNaidData.IsCompleted)
        {
            await _getNaidData;
            return;
        }
        _getNaidData = Task.Run(async () =>
        {
            try
            {
                var requestData =
                    $"grant_type={(isRefresh ? "refresh_token" : "authorization_code")}" +
                    $"&client_id={GetSecret("NAID_CLIENT_ID")}" +
                    $"&client_secret={GetSecret("NAID_CLIENT_SECRET")}" +
                    $"&{(isRefresh ? "refresh_token" : "code")}={token}" +
                    (isRefresh ? "" : $"&redirect_uri=http://localhost:{port}/callback");

                var httpContent = new StringContent(requestData, Encoding.UTF8, "application/x-www-form-urlencoded");

                using var httpClient = new HttpClient();
                using var oauthResponse = await httpClient.PostAsync("https://account.naids.com/api/oauth2/token", httpContent);
                oauthResponse.EnsureSuccessStatusCode();

                var result = await oauthResponse.Content.ReadAsStringAsync()
                    ?? throw new Exception("获取 AccessToken 与 RefreshToken 失败，返回内容为空");
                var data = JsonNode.Parse(result);
                var accessToken = data?["access_token"]?.ToString();
                var refreshToken = data?["refresh_token"]?.ToString();

                if (data == null || accessToken == null || refreshToken == null)
                    throw new Exception("获取 AccessToken 与 RefreshToken 失败，解析返回内容失败");

                NaidProfile.AccessToken = accessToken;
                NaidProfile.RefreshToken = refreshToken;

                using var userDataRequest = new HttpRequestMessage(HttpMethod.Get, "https://account.naids.com/api/api/user/data");
                userDataRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", NaidProfile.AccessToken);
                using var userDataResponse = await httpClient.SendAsync(userDataRequest);
                userDataResponse.EnsureSuccessStatusCode();

                var receivedUserData = await userDataResponse.Content.ReadAsStringAsync()
                    ?? throw new Exception("获取 Natayark 用户信息失败，返回内容为空");
                var userData = (JsonNode.Parse(receivedUserData)?["data"])
                    ?? throw new Exception("获取 Natayark 用户信息失败，解析返回内容失败");

                NaidProfile.Id = userData["id"]?.GetValue<int>() ?? 0;
                NaidProfile.Username = userData["username"]?.ToString() ?? string.Empty;
                NaidProfile.Email = userData["email"]?.ToString() ?? string.Empty;
                NaidProfile.Status = userData["status"]?.GetValue<int>() ?? 0;
                NaidProfile.IsRealNamed = userData["realname"]?.GetValue<bool>() ?? false;
                NaidProfile.LastIp = userData["last_ip"]?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                if (isRetry)
                {
                    NaidProfile = new NaidUser();
                    _logger.LogWarning(ex, "获取 Natayark 用户数据失败，请尝试前往设置重新登录");
                }
                else
                {
                    if (ex.Message.Contains("invalid access token"))
                    {
                        _logger.LogWarning("Naid Access Token 无效，尝试刷新登录");
                        await Task.Delay(TimeSpan.FromMilliseconds(50));
                        await GetNaidDataAsync(NaidProfile.RefreshToken ?? "", true, true);
                    }
                    else if (ex.Message.Contains("invalid_grant"))
                    {
                        _logger.LogWarning("Naid 验证代码无效");
                    }
                    else if (ex is HttpRequestException { StatusCode: System.Net.HttpStatusCode.Unauthorized })
                    {
                        NaidProfile = new NaidUser();
                        _logger.LogWarning("Natayark 账号信息已过期，请前往设置重新登录！");
                    }
                    else
                    {
                        NaidProfile = new NaidUser();
                        _logger.LogWarning(ex, "Naid 登录失败，请尝试前往设置重新登录");
                    }
                }
                throw;
            }
        });

        await _getNaidData;
    }

    private static string GetSecret(string key)
    {
        return Environment.GetEnvironmentVariable(key) ?? string.Empty;
    }
}