using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Minecraft.IdentityModel.Extensions.OpenId;
using PCL_CE.Neo.Core.Minecraft.IdentityModel.OAuth;
using PCL_CE.Neo.Core.Utils.Exts;
using PCL_CE.Neo.Core.IO.Net.Http;

namespace PCL_CE.Neo.Core.Minecraft.IdentityModel.Extensions.YggdrasilConnect;

public record YggdrasilOptions : OpenIdOptions
{
    private string[] _scopesRequired = ["openid", "Yggdrasil.PlayerProfiles.Select", "Yggdrasil.Server.Join"];

    public override async Task InitializeAsync(CancellationToken token)
    {
        using var response = await HttpRequest
            .Create(OpenIdDiscoveryAddress)
            .WithHeaders(Headers ?? [])
            .SendAsync(GetClient.Invoke())
            .ConfigureAwait(false);

        Meta = (await response.AsJsonAsync<YggdrasilConnectMetaData>().ConfigureAwait(false))
            ?? throw new InvalidOperationException();
        if (_scopesRequired.Except(Meta.ScopesSupported).Any()) throw new InvalidOperationException();
    }

    public override async Task<OAuthClientOptions> BuildOAuthOptionsAsync(CancellationToken token)
    {
        if (Meta is YggdrasilConnectMetaData meta)
        {
            var options = await base.BuildOAuthOptionsAsync(token);
            if (!options.ClientId.IsNullOrEmpty()) return options;
            if (meta is null) throw new InvalidOperationException();
            if (!meta.SharedClientId.IsNullOrEmpty())
            {
                options.ClientId = meta.SharedClientId;
            }

            throw new ArgumentException();
        }

        throw new InvalidCastException();
    }
}