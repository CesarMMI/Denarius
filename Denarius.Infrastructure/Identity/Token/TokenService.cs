using Denarius.Application.Auth.Services;
using Denarius.Application.Shared.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Denarius.Infrastructure.Identity.Token;

internal class TokenService : ITokenService
{
    private readonly string issuer;
    private readonly string accessSecret;
    private readonly string refreshSecret;
    private readonly double accessExpiresMins;
    private readonly double refreshExpiresMins;

    private readonly JwtSecurityTokenHandler tokenHandler;

    public TokenService(IConfiguration configuration)
    {
        tokenHandler = new JwtSecurityTokenHandler();
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        issuer = configuration["JWT:Issuer"]!;
        accessSecret = configuration["JWT:AccessSecret"]!;
        refreshSecret = configuration["JWT:RefreshSecret"]!;
        accessExpiresMins = double.Parse(configuration["JWT:AccessExpiresMins"]!);
        refreshExpiresMins = double.Parse(configuration["JWT:RefreshExpiresMins"]!);
    }


    public string GenerateAccessToken(int userId, string userName)
    {
        return GenerateToken(userId, userName, accessSecret, DateTime.UtcNow.AddMinutes(accessExpiresMins));
    }

    public string GenerateRefreshToken(int userId, string userName)
    {
        return GenerateToken(userId, userName, refreshSecret, DateTime.UtcNow.AddMinutes(refreshExpiresMins));
    }

    public ClaimsPrincipal VerifyRefreshToken(string token)
    {
        try
        {
            return tokenHandler.ValidateToken(token, TokenServiceExtensions.GetValidationParameters(issuer, refreshSecret), out SecurityToken validatedToken);
        }
        catch (SecurityTokenException)
        {
            throw new UnauthorizedException("Invalid token");
        }
    }

    private string GenerateToken(int userId, string userName, string secret, DateTime expires)
    {
        var claims = new List<Claim> {
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var key = TokenServiceExtensions.GetSymmetricSecurityKey(secret);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return tokenHandler.WriteToken(tokenDescriptor);
    }
}
