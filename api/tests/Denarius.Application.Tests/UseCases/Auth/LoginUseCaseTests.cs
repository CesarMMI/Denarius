using Denarius.Application.Exceptions.Users;
using Denarius.Application.Inputs.Auth;
using Denarius.Application.Interfaces;
using Denarius.Application.UseCases.Auth;
using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Auth;

public class LoginUseCaseTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly LoginUseCase _sut;

    private readonly User _user = new("john@example.com", "hashed_password", "Johnny");

    public LoginUseCaseTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _tokenService = Substitute.For<ITokenService>();
        _sut = new LoginUseCase(_userRepository, _passwordHasher, _tokenService);

        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns(_user);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _tokenService.GenerateToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>()).Returns("jwt_token");
    }

    // -------------------------------------------------------------------------
    // Execute — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithValidCredentials_ReturnsLoginOutput()
    {
        var input = new LoginInput("john@example.com", "secret");

        var result = await _sut.Execute(input);

        Assert.Equal("jwt_token", result.Token);
        Assert.Equal("john@example.com", result.User.Email);
        Assert.Equal("Johnny", result.User.Name);
    }

    [Fact]
    public async Task Execute_WithValidCredentials_CallsGenerateToken()
    {
        var input = new LoginInput("john@example.com", "secret");

        await _sut.Execute(input);

        _tokenService.Received(1).GenerateToken(_user.Id, _user.Email, _user.Name);
    }

    // -------------------------------------------------------------------------
    // Execute — unknown email
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithUnknownEmail_ThrowsInvalidCredentialsException()
    {
        _userRepository.GetByEmailAsync("unknown@example.com").Returns((User?)null);
        var input = new LoginInput("unknown@example.com", "secret");

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.Execute(input));
    }

    [Fact]
    public async Task Execute_WithUnknownEmail_DoesNotCallVerify()
    {
        _userRepository.GetByEmailAsync("unknown@example.com").Returns((User?)null);
        var input = new LoginInput("unknown@example.com", "secret");

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.Execute(input));

        _passwordHasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
    }

    // -------------------------------------------------------------------------
    // Execute — wrong password
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithWrongPassword_ThrowsInvalidCredentialsException()
    {
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);
        var input = new LoginInput("john@example.com", "wrong");

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.Execute(input));
    }

    [Fact]
    public async Task Execute_WithWrongPassword_DoesNotCallGenerateToken()
    {
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);
        var input = new LoginInput("john@example.com", "wrong");

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.Execute(input));

        _tokenService.DidNotReceive().GenerateToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());
    }
}
