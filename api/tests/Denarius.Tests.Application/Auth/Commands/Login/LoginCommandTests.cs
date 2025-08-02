using Denarius.Application.Auth.Commands.Login;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Denarius.Domain.Services;
using Moq;

namespace Denarius.Tests.Application.Auth.Commands.Login;

public class LoginCommandTests
{
    private static readonly Mock<IPasswordService> passwordServiceMock = new();
    private static readonly Mock<ITokenService> tokenServiceMock = new();
    private static readonly Mock<IUserRepository> userRepositoryMock = new();

    [Fact]
    public async Task Execute_ReturnsAuthResult_WhenValid()
    {
        // Arrange
        var userMock = new User()
        {
            Id = 1,
            Name = "User Test",
            Email = "user@test.com",
            HashedPassword = "hashedPassword"
        };
        var query = new LoginQuery() { Email = userMock.Email, Password = "password123" };

        var command = new LoginCommand(passwordServiceMock.Object, tokenServiceMock.Object, userRepositoryMock.Object);

        passwordServiceMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<int>(), It.IsAny<string>())).Returns("access-token");
        tokenServiceMock.Setup(x => x.GenerateRefreshToken(It.IsAny<int>(), It.IsAny<string>())).Returns("refresh-token");
        userRepositoryMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(userMock);

        // Act
        var result = await command.Execute(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userMock.Name, result.User.Name);
        Assert.Equal(userMock.Email, result.User.Email);
    }

    [Fact]
    public async Task Execute_ThrowsUnauthorized_WhenEmailIsInvalid()
    {
        // Arrange
        var userMock = new User()
        {
            Id = 1,
            Name = "User Test",
            Email = "user@test.com",
            HashedPassword = "hashedPassword"
        };
        var query = new LoginQuery() { Email = "invalid@test.com", Password = "password123" };

        var command = new LoginCommand(passwordServiceMock.Object, tokenServiceMock.Object, userRepositoryMock.Object);

        userRepositoryMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(userMock);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => command.Execute(query));
        Assert.Equal("Invalid email or password", ex.Message);
    }

    [Fact]
    public async Task Execute_ThrowsUnauthorized_WhenPasswordIsInvalid()
    {
        // Arrange
        var userMock = new User()
        {
            Id = 1,
            Name = "User Test",
            Email = "user@test.com",
            HashedPassword = "hashedPassword"
        };
        var query = new LoginQuery() { Email = userMock.Email, Password = "password123" };

        var command = new LoginCommand(passwordServiceMock.Object, tokenServiceMock.Object, userRepositoryMock.Object);

        passwordServiceMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        userRepositoryMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(userMock);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => command.Execute(query));
        Assert.Equal("Invalid email or password", ex.Message);
    }
}