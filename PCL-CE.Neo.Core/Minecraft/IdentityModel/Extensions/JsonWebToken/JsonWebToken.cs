using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using PCL_CE.Neo.Core.Minecraft.IdentityModel.Extensions.OpenId;

namespace PCL_CE.Neo.Core.Minecraft.IdentityModel.Extensions.JsonWebToken;

public class JsonWebToken(string token, OpenIdMetadata meta)
{
    public delegate SecurityToken? TokenValidateCallback(OpenIdMetadata metadata, string token, JsonWebKey? key, string? clientId);

    public TokenValidateCallback SecurityTokenValidateCallback { get; set; } = static (meta, token, key, clientId) =>
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            
            var parameter = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = meta.Issuer,
                ValidateAudience = !string.IsNullOrEmpty(clientId),
                ValidAudience = clientId,
                ValidateIssuerSigningKey = key != null,
                IssuerSigningKey = key != null ? new JsonWebKeySet { Keys = { key } }.Keys[0] : null,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(60)
            };

            handler.ValidateToken(token, parameter, out var secToken);
            return secToken;
        }
        catch (Exception ex)
        {
            throw new SecurityException($"令牌验证失败：{ex.Message}", ex);
        }
    };

    private bool _verified;
    private JwtSecurityToken? _parsedToken;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    private JwtSecurityToken _ParseToken()
    {
        if (_parsedToken != null)
            return _parsedToken;

        try
        {
            if (!_tokenHandler.CanReadToken(token))
                throw new SecurityException("无法读取令牌：格式无效");

            _parsedToken = _tokenHandler.ReadJwtToken(token);
            return _parsedToken;
        }
        catch (Exception ex)
        {
            throw new SecurityException($"令牌解析失败：{ex.Message}", ex);
        }
    }

    public T? ReadTokenPayload<T>(bool allowUnverifyToken = false)
    {
        if (!allowUnverifyToken && !_verified)
            throw new SecurityException("不安全的令牌");

        try
        {
            var jwtToken = _ParseToken();

            if (jwtToken.Payload == null || jwtToken.Payload.Count == 0)
                throw new InvalidOperationException("令牌 Payload 无效");

            if (typeof(T).IsAssignableFrom(typeof(Dictionary<string, object>)))
                return (T)(object)jwtToken.Payload;

            if (typeof(T) == typeof(JwtPayload))
                return (T)(object)jwtToken.Payload;

            var payloadJson = JsonSerializer.Serialize(jwtToken.Payload);
            var result = JsonSerializer.Deserialize<T>(payloadJson);

            return result;
        }
        catch (SecurityException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SecurityException($"读取令牌 payload 失败：{ex.Message}", ex);
        }
    }

    public T? ReadTokenHeader<T>()
    {
        try
        {
            var jwtToken = _ParseToken();

            if (jwtToken.Header == null || jwtToken.Header.Count == 0)
                throw new InvalidOperationException("令牌中不存在 header 数据");

            if (typeof(T).IsAssignableFrom(typeof(Dictionary<string, object>)))
                return (T)(object)jwtToken.Header;

            if (typeof(T) == typeof(JwtHeader))
                return (T)(object)jwtToken.Header;

            var headerJson = JsonSerializer.Serialize(jwtToken.Header);
            var result = JsonSerializer.Deserialize<T>(headerJson);

            return result;
        }
        catch (Exception ex)
        {
            throw new SecurityException($"读取令牌 header 失败：{ex.Message}", ex);
        }
    }

    public SecurityToken? VerifySignature(JsonWebKey key, string? clientId = null)
    {
        try
        {
            var result = SecurityTokenValidateCallback.Invoke(meta, token, key, clientId);
            if (result != null)
                _verified = true;
            return result;
        }
        catch (Exception ex)
        {
            throw new SecurityException($"令牌签名验证失败：{ex.Message}", ex);
        }
    }

    public SecurityToken? VerifySignature()
    {
        try
        {
            var result = SecurityTokenValidateCallback.Invoke(meta, token, null, null);
            if (result != null)
                _verified = true;
            return result;
        }
        catch (Exception ex)
        {
            throw new SecurityException($"令牌验证失败：{ex.Message}", ex);
        }
    }

    public DateTime? GetExpirationTime()
    {
        try
        {
            var jwtToken = _ParseToken();
            return jwtToken.ValidTo != DateTime.MinValue ? jwtToken.ValidTo : null;
        }
        catch (Exception ex)
        {
            throw new SecurityException($"获取令牌过期时间失败：{ex.Message}", ex);
        }
    }

    public DateTime? GetIssuedAtTime()
    {
        try
        {
            var jwtToken = _ParseToken();
            return jwtToken.ValidFrom != DateTime.MinValue ? jwtToken.ValidFrom : null;
        }
        catch (Exception ex)
        {
            throw new SecurityException($"获取令牌签发时间失败：{ex.Message}", ex);
        }
    }

    public bool IsExpired()
    {
        try
        {
            var expTime = GetExpirationTime();
            return expTime.HasValue && DateTime.UtcNow > expTime.Value;
        }
        catch
        {
            return true;
        }
    }

    public string? GetClaimValue(string claimType, bool allowUnverifyToken = false)
    {
        try
        {
            var payload = ReadTokenPayload<Dictionary<string, object>>(allowUnverifyToken);
            return payload?.TryGetValue(claimType, out var value) ?? false ? value.ToString() : null;
        }
        catch (Exception ex)
        {
            throw new SecurityException($"获取声明值失败（{claimType}）：{ex.Message}", ex);
        }
    }

    public string GetTokenString() => token;

    public bool IsVerified => _verified;
}

using PCL_CE.Neo.Core.Minecraft.IdentityModel.Extensions.OpenId;