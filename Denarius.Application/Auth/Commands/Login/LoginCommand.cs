using Denarius.Application.Auth.Results;
using Denarius.Application.Auth.Services;
using Denarius.Application.Shared.Exceptions;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Auth.Commands.Login;

internal class LoginCommand(IPasswordService passwordService, ITokenService tokenService, IUserRepository userRepository) : ILoginCommand
{
    public async Task<AuthResult> Execute(LoginQuery query)
    {
        query.Validate();

        var user = await userRepository.FindByEmailAsync(query.Email);

        if (user is null || !passwordService.Verify(user.HashedPassword, query.Password))
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        return new AuthResult
        {
            User = user.ToUserResult(),
            AccessToken = tokenService.GenerateAccessToken(user.Id, user.Name),
            RefreshToken = tokenService.GenerateRefreshToken(user.Id, user.Name),
        };
    }
}
