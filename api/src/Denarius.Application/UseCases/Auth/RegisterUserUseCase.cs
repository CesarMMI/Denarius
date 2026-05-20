using Denarius.Application.Exceptions.Users;
using Denarius.Application.Inputs.Auth;
using Denarius.Application.Interfaces;
using Denarius.Application.Interfaces.UseCases.Auth;
using Denarius.Application.Outputs.Auth;
using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces.Repositories;

namespace Denarius.Application.UseCases.Auth;

public class RegisterUserUseCase(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
    : IRegisterUserUseCase
{
    public async Task<UserOutput> Execute(RegisterUserInput input)
    {
        if (await userRepository.ExistsByEmailAsync(input.Email))
            throw new EmailAlreadyInUseException(input.Email);

        if (input.Password.Length < 3)
            throw new InvalidPasswordException("Password must be at least 3 characters.");

        var passwordHash = passwordHasher.Hash(input.Password);
        var user = new User(input.Email, passwordHash, input.Name);

        await userRepository.AddAsync(user);
        await unitOfWork.CommitAsync();

        return UserOutput.FromEntity(user);
    }
}
