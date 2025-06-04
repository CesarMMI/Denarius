using Denarius.Application.Auth.Commands.Register;
using Denarius.Application.Auth.Services;
using Denarius.Application.Shared.Exceptions;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Moq;

namespace Denarius.Tests.Application.Auth.Commands.Register;

public class RegisterCommandTests
{
    private static readonly Mock<IPasswordService> passwordServiceMock = new();
    private static readonly Mock<ITokenService> tokenServiceMock = new();
    private static readonly Mock<IUserRepository> userRepositoryMock = new();

    [Fact]
    public async Task Execute_ReturnsAuthResult_WhenCredentialsIsValid()
    {
        // Arrange
        var userMock = new User()
        {
            Id = 1,
            Name = "User Test",
            Email = "user@test.com",
            HashedPassword = "hashedPassword"
        };
        var query = new RegisterQuery { Name = userMock.Name, Email = userMock.Email, Password = "password12345" };

        var command = new RegisterCommand(passwordServiceMock.Object, tokenServiceMock.Object, userRepositoryMock.Object);

        passwordServiceMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashedPassword");
        tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<int>(), It.IsAny<string>())).Returns("access-token");
        tokenServiceMock.Setup(x => x.GenerateRefreshToken(It.IsAny<int>(), It.IsAny<string>())).Returns("refresh-token");
        userRepositoryMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User)null);
        userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync(userMock);

        // Act
        var result = await command.Execute(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userMock.Name, result.User.Name);
        Assert.Equal(userMock.Email, result.User.Email);
    }

    [Fact]
    public async Task Execute_ThrowsBadRequest_WhenEmailAlreadyInUse()
    {
        // Arrange
        var userMock = new User()
        {
            Id = 1,
            Name = "User Test",
            Email = "user@test.com",
            HashedPassword = "hashedPassword"
        };
        var query = new RegisterQuery { Name = userMock.Name, Email = userMock.Email, Password = "password12345" };

        var command = new RegisterCommand(passwordServiceMock.Object, tokenServiceMock.Object, userRepositoryMock.Object);

        userRepositoryMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(userMock);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadRequestException>(() => command.Execute(query));
        Assert.Equal("Email already in use", ex.Message);
    }
}