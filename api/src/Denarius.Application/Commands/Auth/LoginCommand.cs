using Denarius.Application.Domain.Commands.Auth;
using Denarius.Application.Domain.Queries.Auth;
using Denarius.Application.Domain.Results.Auth;
using Denarius.Application.Exceptions;
using Denarius.Application.Extensions;
using Denarius.Domain.Repositories;
using Denarius.Domain.Services;

namespace Denarius.Application.Commands.Auth;

internal class LoginCommand(IPasswordService passwordService, ITokenService tokenService, IUserRepository userRepository) : Command<LoginQuery, AuthResult>, ILoginCommand
{
    protected override async Task<AuthResult> Handle(LoginQuery query)
    {
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

    protected override void Validate(LoginQuery query)
    {
        if (!query.Email.IsValidString()) throw new BadRequestException("Email is required");
        if (!query.Email.IsValidEmail()) throw new BadRequestException("Invalid email");

        if (!query.Password.IsValidString()) throw new BadRequestException("Password is required");
        if (query.Password.Length < 5) throw new BadRequestException("Password length can't be lower than 5");
        if (query.Password.Length > 100) throw new BadRequestException("Password length can't be greater than 100");
    }
}
