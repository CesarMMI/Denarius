using Denarius.Application.Domain.Commands.Auth;
using Denarius.Application.Domain.Queries.Auth;
using Denarius.Application.Domain.Results.Auth;
using Denarius.Application.Exceptions;
using Denarius.Application.Extensions;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Denarius.Domain.Services;

namespace Denarius.Application.Commands.Auth;

internal class RegisterCommand(IPasswordService passwordService, ITokenService tokenService, IUserRepository userRepository) : Command<RegisterQuery, AuthResult>, IRegisterCommand
{
    protected override async Task<AuthResult> Handle(RegisterQuery query)
    {
        var user = await userRepository.FindOneAsync(user => user.Email == query.Email);
        if (user is not null) throw new BadRequestException("Email already in use");

        user = new User
        {
            Name = query.Name,
            Email = query.Email,
            HashedPassword = passwordService.Hash(query.Password)
        };
        user = await userRepository.CreateAsync(user);

        return new AuthResult
        {
            User = user.ToResult(),
            AccessToken = tokenService.GenerateAccessToken(user.Id, user.Name),
            RefreshToken = tokenService.GenerateRefreshToken(user.Id, user.Name)
        };
    }

    protected override void Validate(RegisterQuery query)
    {
        if (!query.Name.IsValidString()) throw new BadRequestException("Name is required");
        if (query.Name.Length < 3) throw new BadRequestException("Name length can't be lower than 3");
        if (query.Name.Length > 50) throw new BadRequestException("Name length can't be greater than 50");

        if (!query.Email.IsValidString()) throw new BadRequestException("Email is required");
        if (!query.Email.IsValidEmail()) throw new BadRequestException("Invalid email");

        if (!query.Password.IsValidString()) throw new BadRequestException("Password is required");
        if (query.Password.Length < 5) throw new BadRequestException("Password length can't be lower than 5");
        if (query.Password.Length > 100) throw new BadRequestException("Password length can't be greater than 100");
    }
}
