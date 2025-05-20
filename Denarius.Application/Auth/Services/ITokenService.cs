using System.Security.Claims;

namespace Denarius.Application.Auth.Services;

public interface ITokenService
{
    public string GenerateAccessToken(int userId, string userName);
    public string GenerateRefreshToken(int userId, string userName);
    public ClaimsPrincipal VerifyRefreshToken(string token);
}
