using Denarius.Application.Exceptions.Users;
using Denarius.Application.Inputs.Auth;
using Denarius.Application.Interfaces;
using Denarius.Application.Interfaces.UseCases.Auth;
using Denarius.Application.Outputs.Auth;
using Denarius.Domain.Interfaces.Repositories;

namespace Denarius.Application.UseCases.Auth;

public class LoginUseCase(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService)
    : ILoginUseCase
{
    public async Task<LoginOutput> Execute(LoginInput input)
    {
        var user = await userRepository.GetByEmailAsync(input.Email);
        if (user is null)
            throw new InvalidCredentialsException();

        if (!passwordHasher.Verify(input.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        var token = tokenService.GenerateToken(user.Id, user.Email, user.Name);
        return new LoginOutput(token, UserOutput.FromEntity(user));
    }
}
