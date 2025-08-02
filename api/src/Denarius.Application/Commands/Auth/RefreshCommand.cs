using Denarius.Application.Domain.Commands.Auth;
using Denarius.Application.Domain.Queries.Auth;
using Denarius.Application.Domain.Results.Auth;
using Denarius.Application.Extensions;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Repositories;
using Denarius.Domain.Services;

namespace Denarius.Application.Commands.Auth;

internal class RefreshCommand(ITokenService tokenService, IUserRepository userRepository) : IRefreshCommand
{
    public async Task<AuthResult> Execute(RefreshQuery query)
    {
        query.Validate();

        query.UserId = tokenService.VerifyRefreshToken(query.RefreshToken);
        var user = await userRepository.FindOneAsync(user => user.Id == query.UserId);
        if (user is null) throw new UnauthorizedException("Invalid token");

        return new AuthResult
        {
            User = user.ToResult(),
            AccessToken = tokenService.GenerateAccessToken(user.Id, user.Name),
            RefreshToken = query.RefreshToken
        };
    }
}
