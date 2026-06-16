using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Minecraft.IdentityModel.Extensions.OpenId;
using PCL_CE.Neo.Core.Minecraft.IdentityModel.OAuth;
using PCL_CE.Neo.Core.Network;
using PCL_CE.Neo.Core.Utils.Exts;

namespace PCL_CE.Neo.Core.Minecraft.IdentityModel.Extensions.YggdrasilConnect;

public record YggdrasilOptions : OpenIdOptions
{
    private string[] _scopesRequired = ["openid", "Yggdrasil.PlayerProfiles.Select", "Yggdrasil.Server.Join"];

    public override async Task InitializeAsync(CancellationToken token)
    {
        try
        {
            var response = await NetworkService.GetAsync(OpenIdDiscoveryAddress, GetClient.Invoke(), token);
            Meta = (await response.Content.ReadFromJsonAsync<YggdrasilConnectMetaData>(cancellationToken: token))
                ?? throw new InvalidOperationException("无法获取 Yggdrasil 配置");
            
            if (_scopesRequired.Except(Meta.ScopesSupported).Any()) 
                throw new InvalidOperationException("Yggdrasil 服务不支持所需的权限");
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "YggdrasilOptions", "拉取 Yggdrasil 配置失败");
            throw;
        }
    }

    public override async Task<OAuthClientOptions> BuildOAuthOptionsAsync(CancellationToken token)
    {
        if (Meta is YggdrasilConnectMetaData meta)
        {
            var options = await base.BuildOAuthOptionsAsync(token);
            if (!options.ClientId.IsNullOrEmpty()) return options;
            if (meta is null) throw new InvalidOperationException("Yggdrasil 元数据未初始化");
            if (!meta.SharedClientId.IsNullOrEmpty())
            {
                options.ClientId = meta.SharedClientId;
                return options;
            }
            throw new ArgumentException("无法获取 ClientId");
        }
        throw new InvalidCastException("元数据类型不正确");
    }
}