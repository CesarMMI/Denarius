using Denarius.Application.Auth.Commands.Refresh;
using Denarius.Application.Auth.Services;
using Denarius.Application.Shared.Exceptions;
using Denarius.Domain.Models;
using Denarius.Domain.Repositories;
using Moq;

namespace Denarius.Tests.Application.Auth.Commands.Refresh;

public class RefreshCommandTests
{
    private static readonly Mock<ITokenService> tokenServiceMock = new();
    private static readonly Mock<IUserRepository> userRepositoryMock = new();

    [Fact]
    public async Task Execute_ReturnsAuthResult_WhenRefreshTokenIsValid()
    {
        // Arrange
        var command = new RefreshCommand(tokenServiceMock.Object, userRepositoryMock.Object);

        var userMock = new User()
        {
            Id = 1,
            Name = "User Test",
            Email = "user@test.com",
            HashedPassword = "hashedPassword"
        };
        var query = new RefreshQuery { RefreshToken = "valid-refresh-token" };

        tokenServiceMock.Setup(x => x.VerifyRefreshToken(It.IsAny<string>())).Returns(userMock.Id);
        tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<int>(), It.IsAny<string>())).Returns("valid-access-token");
        userRepositoryMock.Setup(x => x.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(userMock);

        // Act
        var result = await command.Execute(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userMock.Name, result.User.Name);
        Assert.Equal(userMock.Email, result.User.Email);
        Assert.Equal(query.RefreshToken, result.RefreshToken);
    }

    [Fact]
    public async Task Execute_ThrowsUnauthorized_WhenUserNotFound()
    {
        // Arrange
        var command = new RefreshCommand(tokenServiceMock.Object, userRepositoryMock.Object);

        var userMock = new User()
        {
            Id = 1,
            Name = "User Test",
            Email = "user@test.com",
            HashedPassword = "hashedPassword"
        };
        var query = new RefreshQuery { RefreshToken = "valid-refresh-token" };

        tokenServiceMock.Setup(x => x.VerifyRefreshToken(It.IsAny<string>())).Returns(userMock.Id);
        userRepositoryMock.Setup(x => x.FindByIdAsync(It.IsAny<int>())).ReturnsAsync((User)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => command.Execute(query));
        Assert.Equal("Invalid token", ex.Message);
    }
}