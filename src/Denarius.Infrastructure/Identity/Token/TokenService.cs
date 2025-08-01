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

    public string UserIdClaimType => "sub";

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
        var expires = DateTime.UtcNow.AddMinutes(accessExpiresMins);
        return GenerateToken(userId, userName, accessSecret, expires);
    }

    public string GenerateRefreshToken(int userId, string userName)
    {
        var expires = DateTime.UtcNow.AddMinutes(refreshExpiresMins);
        return GenerateToken(userId, userName, refreshSecret, expires);
    }

    public int VerifyRefreshToken(string token)
    {
        try
        {
            var validationParameters = TokenServiceExtensions.GetValidationParameters(issuer, refreshSecret);
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            var userIdClaim = principal.FindFirst(UserIdClaimType);

            if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out int userId))
                return 0;
            return userId;
        }
        catch (Exception ex)
        {
            var message = JwtBearerEventsHandler.GetExceptionMessage(ex);
            throw new UnauthorizedException(message);
        }
    }

    private string GenerateToken(int userId, string userName, string secret, DateTime expires)
    {
        var key = TokenServiceExtensions.GetSymmetricSecurityKey(secret);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim> {
            new(UserIdClaimType, userId.ToString()),
            new("name", userName)
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return tokenHandler.WriteToken(tokenDescriptor);
    }
}
