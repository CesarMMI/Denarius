using Denarius.Application.Domain.Commands.Auth;
using Denarius.Application.Domain.Queries.Auth;
using Denarius.Application.Domain.Results.Auth;
using Denarius.Application.Extensions;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Denarius.Domain.Services;
using Denarius.Domain.UnitOfWork;

namespace Denarius.Application.Commands.Auth;

internal class RegisterCommand(
    IUnitOfWork unitOfWork,
    IPasswordService passwordService,
    ITokenService tokenService,
    IUserRepository userRepository
) : IRegisterCommand
{
    public async Task<AuthResult> Execute(RegisterQuery query)
    {

        var user = await userRepository.FindOneAsync(user => user.Email == query.Email);
        if (user is not null) throw new BadRequestException("Email already in use");

        user = new User
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
            User = user.ToResult(),
            AccessToken = tokenService.GenerateAccessToken(user.Id, user.Name),
            RefreshToken = tokenService.GenerateRefreshToken(user.Id, user.Name)
        };
    }
}
