using Denarius.Application.Domain.Commands.Auth;
using Denarius.Application.Domain.Queries.Auth;
using Denarius.Application.Domain.Results.Auth;
using Denarius.Application.Exceptions;
using Denarius.Application.Extensions;
using Denarius.Domain.Repositories;
using Denarius.Domain.Services;

namespace Denarius.Application.Commands.Auth;

internal class RefreshCommand(ITokenService tokenService, IUserRepository userRepository) : Command<RefreshQuery, AuthResult>, IRefreshCommand
{
    protected override async Task<AuthResult> Handle(RefreshQuery query)
    {
        var userId = tokenService.VerifyRefreshToken(query.RefreshToken);
        var user = await userRepository.FindOneAsync(user => user.Id == userId);

        if (user is null) throw new UnauthorizedException("Invalid token");

        return new AuthResult
        {
            User = user.ToResult(),
            AccessToken = tokenService.GenerateAccessToken(user.Id, user.Name),
            RefreshToken = query.RefreshToken
        };
    }

    protected override void Validate(RefreshQuery query)
    {
        if (!query.RefreshToken.IsValidString()) throw new BadRequestException("Refresh token is required");
    }
}
