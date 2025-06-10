using Denarius.Application.Auth.Results;
using Denarius.Application.Auth.Services;
using Denarius.Application.Shared.Exceptions;
using Denarius.Application.Shared.UnitOfWork;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;

namespace Denarius.Application.Auth.Commands.Register;

public class RegisterCommand(
    IUnitOfWork unitOfWork,
    IPasswordService passwordService,
    ITokenService tokenService,
    IUserRepository userRepository
) : IRegisterCommand
{
    public async Task<AuthResult> Execute(RegisterQuery query)
    {
        if (await userRepository.FindByEmailAsync(query.Email) is not null)
            throw new BadRequestException("Email already in use");

        var user = new User
        {
            Name = query.Name,
            Email = query.Email,
            HashedPassword = passwordService.Hash(query.Password)
        };

        await unitOfWork.BeginTransactionAsync();
        try
        {
            user = await userRepository.CreateAsync(user);
            await unitOfWork.CommitAsync();
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }

        return new AuthResult
        {
            User = user.ToUserResult(),
            AccessToken = tokenService.GenerateAccessToken(user.Id, user.Name),
            RefreshToken = tokenService.GenerateRefreshToken(user.Id, user.Name)
        };
    }
}
