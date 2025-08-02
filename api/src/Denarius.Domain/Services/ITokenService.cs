namespace Denarius.Domain.Services;

public interface ITokenService
{
    public string UserIdClaimType { get; }
    public string GenerateAccessToken(int userId, string userName);
    public string GenerateRefreshToken(int userId, string userName);
    public int VerifyRefreshToken(string token);
}
