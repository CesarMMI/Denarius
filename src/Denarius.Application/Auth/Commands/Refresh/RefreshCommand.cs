using Denarius.Application.Auth.Results;
using Denarius.Application.Auth.Services;
using Denarius.Application.Shared.Exceptions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Auth.Commands.Refresh;

public class RefreshCommand(ITokenService tokenService, IUserRepository userRepository) : IRefreshCommand
{
    public async Task<AuthResult> Execute(RefreshQuery query)
    {
        query.Validate();

        query.UserId = tokenService.VerifyRefreshToken(query.RefreshToken);
        var user = await userRepository.FindByIdAsync(query.UserId);

        if (user is null)
        {
            throw new UnauthorizedException("Invalid token");
        }

        return new AuthResult
        {
            User = user.ToUserResult(),
            AccessToken = tokenService.GenerateAccessToken(user.Id, user.Name),
            RefreshToken = query.RefreshToken
        };
    }
}
