using Denarius.Application.Auth.Results;
using Denarius.Application.Auth.Services;
using Denarius.Application.Shared.Exceptions;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Auth.Commands.Register;

internal class RegisterCommand(IPasswordService passwordService, ITokenService tokenService, IUserRepository userRepository) : IRegisterCommand
{
    public async Task<AuthResult> Execute(RegisterQuery request)
    {
        request.Validate();

        if (await userRepository.FindByEmailAsync(request.Email) is not null)
        {
            throw new BadRequestException("Email already in use");
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            HashedPassword = passwordService.Hash(request.Password)
        };

        user = await userRepository.CreateAsync(user);

        return new AuthResult
        {
            User = user.ToUserResult(),
            AccessToken = tokenService.GenerateAccessToken(user.Id, user.Name),
            RefreshToken = tokenService.GenerateRefreshToken(user.Id, user.Name)
        };
    }
}
