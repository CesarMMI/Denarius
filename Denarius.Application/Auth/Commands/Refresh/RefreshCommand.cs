using Denarius.Application.Auth.Results;
using Denarius.Application.Auth.Services;
using Denarius.Application.Shared.Exceptions;
using Denarius.Domain.Repositories;
using System.Security.Claims;

namespace Denarius.Application.Auth.Commands.Refresh;

internal class RefreshCommand(ITokenService tokenService, IUserRepository userRepository) : IRefreshCommand
{
    public async Task<AuthResult> Execute(RefreshQuery request)
    {
        request.Validate();

        int userId;
        try
        {
            var claims = tokenService.VerifyRefreshToken(request.RefreshToken);
            var sub = claims.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            userId = int.Parse(sub?.Value ?? string.Empty);
        }
        catch (Exception)
        {
            throw new UnauthorizedException("Invalid token");
        }

        var user = await userRepository.FindByIdAsync(request.UserId);
        if (user is null || userId != user.Id)
        {
            throw new UnauthorizedException("Invalid token");
        }

        string accessToken = tokenService.GenerateAccessToken(user.Id, user.Name);

        return new AuthResult
        {
            User = user.ToUserResult(),
            AccessToken = accessToken,
            RefreshToken = request.RefreshToken
        };
    }
}
