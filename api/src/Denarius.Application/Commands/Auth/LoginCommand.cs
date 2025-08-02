using Denarius.Application.Domain.Commands.Auth;
using Denarius.Application.Domain.Queries.Auth;
using Denarius.Application.Domain.Results.Auth;
using Denarius.Application.Extensions;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Repositories;
using Denarius.Domain.Services;

namespace Denarius.Application.Commands.Auth;

internal class LoginCommand(
    IPasswordService passwordService,
    ITokenService tokenService,
    IUserRepository userRepository
) : ILoginCommand
{
    public async Task<AuthResult> Execute(LoginQuery query)
    {
        query.Validate();

        var user = await userRepository.FindOneAsync(user => user.Email == query.Email);
        bool valid = passwordService.Verify(user?.HashedPassword ?? "", query.Password);
        if (user is null || !valid) throw new UnauthorizedException("Invalid email or password");

        return new AuthResult
        {
            User = user.ToResult(),
            AccessToken = tokenService.GenerateAccessToken(user.Id, user.Name),
            RefreshToken = tokenService.GenerateRefreshToken(user.Id, user.Name),
        };
    }
}
