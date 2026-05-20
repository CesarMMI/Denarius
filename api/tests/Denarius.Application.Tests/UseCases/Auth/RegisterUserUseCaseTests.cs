using Denarius.Application.Exceptions.Users;
using Denarius.Application.Inputs.Auth;
using Denarius.Application.Interfaces;
using Denarius.Application.UseCases.Auth;
using Denarius.Domain.Entities;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Exceptions.Users;
using Denarius.Domain.Interfaces.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Auth;

public class RegisterUserUseCaseTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RegisterUserUseCase _sut;

    public RegisterUserUseCaseTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new RegisterUserUseCase(_userRepository, _passwordHasher, _unitOfWork);

        _userRepository.ExistsByEmailAsync(Arg.Any<string>()).Returns(false);
        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed_password");
    }

    // -------------------------------------------------------------------------
    // Execute — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithValidInput_ReturnsUserOutput()
    {
        var input = new RegisterUserInput("john@example.com", "secret123", "Johnny");

        var result = await _sut.Execute(input);

        Assert.Equal("john@example.com", result.Email);
        Assert.Equal("Johnny", result.Name);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task Execute_WithValidInput_CallsAddAsyncAndCommit()
    {
        var input = new RegisterUserInput("john@example.com", "secret123", "Johnny");

        await _sut.Execute(input);

        await _userRepository.Received(1).AddAsync(Arg.Any<User>());
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task Execute_WithValidInput_HashesPassword()
    {
        var input = new RegisterUserInput("john@example.com", "secret123", "Johnny");

        await _sut.Execute(input);

        _passwordHasher.Received(1).Hash("secret123");
        await _userRepository.Received(1).AddAsync(Arg.Is<User>(u => u.PasswordHash == "hashed_password"));
    }

    // -------------------------------------------------------------------------
    // Execute — duplicate email
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithDuplicateEmail_ThrowsEmailAlreadyInUseException()
    {
        _userRepository.ExistsByEmailAsync("john@example.com").Returns(true);
        var input = new RegisterUserInput("john@example.com", "secret123", "Johnny");

        await Assert.ThrowsAsync<EmailAlreadyInUseException>(() => _sut.Execute(input));
    }

    [Fact]
    public async Task Execute_WithDuplicateEmail_DoesNotCallHasherOrAddAsync()
    {
        _userRepository.ExistsByEmailAsync("john@example.com").Returns(true);
        var input = new RegisterUserInput("john@example.com", "secret123", "Johnny");

        await Assert.ThrowsAsync<EmailAlreadyInUseException>(() => _sut.Execute(input));

        _passwordHasher.DidNotReceive().Hash(Arg.Any<string>());
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>());
    }

    // -------------------------------------------------------------------------
    // Execute — domain validation
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("notanemail")]
    [InlineData("a@")]
    public async Task Execute_WithInvalidEmail_ThrowsInvalidEmailException(string email)
    {
        var input = new RegisterUserInput(email, "secret123", "Johnny");

        await Assert.ThrowsAsync<InvalidEmailException>(() => _sut.Execute(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Execute_WithInvalidName_ThrowsInvalidNameException(string? name)
    {
        var input = new RegisterUserInput("john@example.com", "secret123", name!);

        await Assert.ThrowsAsync<InvalidNameException>(() => _sut.Execute(input));
    }

    [Theory]
    [InlineData("A")]
    [InlineData(" B")]
    public async Task Execute_WithTooShortName_ThrowsInvalidNameException(string name)
    {
        var input = new RegisterUserInput("john@example.com", "secret123", name);

        await Assert.ThrowsAsync<InvalidNameException>(() => _sut.Execute(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("ab")]
    public async Task Execute_WithTooShortPassword_ThrowsInvalidPasswordException(string password)
    {
        var input = new RegisterUserInput("john@example.com", password, "Johnny");

        await Assert.ThrowsAsync<InvalidPasswordException>(() => _sut.Execute(input));
    }

    [Fact]
    public async Task Execute_WithTooShortPassword_DoesNotCallHasher()
    {
        var input = new RegisterUserInput("john@example.com", "ab", "Johnny");

        await Assert.ThrowsAsync<InvalidPasswordException>(() => _sut.Execute(input));

        _passwordHasher.DidNotReceive().Hash(Arg.Any<string>());
    }
}
