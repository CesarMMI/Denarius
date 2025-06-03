using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Denarius.Infrastructure.Identity.Token;

internal static class TokenServiceExtensions
{
    public static TokenValidationParameters GetValidationParameters(string issuer, string secret)
    {
        return new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = GetSymmetricSecurityKey(secret),
        };
    }
    public static SymmetricSecurityKey GetSymmetricSecurityKey(string secret)
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }
}
