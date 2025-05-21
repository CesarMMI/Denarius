using Denarius.Application.Auth.Results;
using Denarius.Application.Auth.Services;
using Denarius.Application.Shared.Exceptions;
using Denarius.Domain.Repositories;
using System.Security.Claims;

namespace Denarius.Application.Auth.Commands.Refresh;

internal class RefreshCommand(ITokenService tokenService, IUserRepository userRepository) : IRefreshCommand
{
    public async Task<AuthResult> Execute(RefreshQuery query)
    {
        query.Validate();

        try
        {
            var claims = tokenService.VerifyRefreshToken(query.RefreshToken);
            var sub = claims.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            query.UserId = int.Parse(sub?.Value ?? string.Empty);
        }
        catch (Exception)
        {
            throw new UnauthorizedException("Invalid token");
        }

        var user = await userRepository.FindByIdAsync(query.UserId);
        if (user is null)
        {
            throw new UnauthorizedException("Invalid token");
        }

        string accessToken = tokenService.GenerateAccessToken(user.Id, user.Name);

        return new AuthResult
        {
            User = user.ToUserResult(),
            AccessToken = accessToken,
            RefreshToken = query.RefreshToken
        };
    }
}
